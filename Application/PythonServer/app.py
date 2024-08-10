from flask import Flask, request, jsonify
from sqlalchemy import create_engine, text
import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.preprocessing import StandardScaler
import numpy as np
from collections import Counter
import random
import os

app = Flask(__name__)

DATABASE_URI = "mssql+pyodbc://DESKTOP-RKNH9AP\\SQLEXPRESS/FlavorPairingsRecommenderDB?driver=ODBC+Driver+17+for+SQL+Server"

engine = create_engine(DATABASE_URI)

def fetch_data(query):
    data = pd.read_sql(query, engine)
    return data

def clean_data(x):
    if isinstance(x, list):
        return [str.lower(i.replace(" ", "")) for i in x]
    elif isinstance(x, str):
        return str.lower(x.replace(" ", ""))
    else:
        return ''

def create_soup(x):
    return f"{x['Name']} {x['Course']} {x['Cuisine']} {x['Ingredients']}"

def get_recommendations(recipe_id, cosine_sim, recipes):
    idx = recipes.index[recipes['RecipeId'] == recipe_id][0]
    sim_scores = list(enumerate(cosine_sim[idx]))
    sim_scores = sorted(sim_scores, key=lambda x: x[1], reverse=True)
    sim_scores = sim_scores[1:6]  # get top 5 similar recipes
    recipe_indices = [i[0] for i in sim_scores]
    return recipes.iloc[recipe_indices]

def get_user_recommendations(user_id, user_likes, recipes, cosine_sim, num_recommendations=10):
    liked_recipes = user_likes[user_likes['UserId'] == user_id]['RecipeId'].tolist()
    all_recommendations = []
    for recipe_id in liked_recipes:
        recommendations = get_recommendations(recipe_id, cosine_sim, recipes)
        all_recommendations.extend(recommendations['RecipeId'].tolist())
    
    if len(all_recommendations) < num_recommendations:
        additional_recommendations = recipes[~recipes['RecipeId'].isin(all_recommendations)]['RecipeId'].tolist()
        num_needed = num_recommendations - len(all_recommendations)
        all_recommendations.extend(random.sample(additional_recommendations, num_needed))
    
    elif len(all_recommendations) > num_recommendations:
        all_recommendations = random.sample(all_recommendations, num_recommendations)
    
    return recipes[recipes['RecipeId'].isin(all_recommendations)]

@app.route("/")
def home():
    return "Hello World, from Flask!"

@app.route('/recommend', methods=['POST'])
def recommend():
    user_id = request.json.get('user_id')
    
    recipes_query = "SELECT RecipeId, Name, Course, Cuisine, Ingredients FROM Recipes"
    recipes = fetch_data(recipes_query)
    
    user_likes_query = "SELECT UserId, RecipeId FROM UserLikes"
    user_likes = fetch_data(user_likes_query)
    
    features = ['Name', 'Course', 'Cuisine', 'Ingredients']
    for feature in features:
        recipes[feature] = recipes[feature].apply(clean_data)
    
    recipes['soup'] = recipes.apply(create_soup, axis=1)
    
    recipes = recipes[recipes['soup'].str.strip().astype(bool)]
    
    count = CountVectorizer(stop_words='english')
    count_matrix = count.fit_transform(recipes['soup'])
    cosine_sim = cosine_similarity(count_matrix, count_matrix)
    
    recommendations = get_user_recommendations(user_id, user_likes, recipes, cosine_sim)
    
    recommendations_list = recommendations.to_dict(orient='records')
    
    return jsonify(recommendations_list)

#######################################################################
#############################IMPROVEMENTS SECTION######################
combined_shared_compounds = pd.read_csv('C:\\Users\\Aurelian\\Licenta\\GOOD_DATA\\Extended_Combined_SharedCompounds.csv')
recipes = pd.read_csv('C:\\Users\\Aurelian\\Licenta\\GOOD_DATA\\recipes.csv')
ingredients_pd = pd.read_csv('C:\\Users\\Aurelian\\Licenta\\GOOD_DATA\\ingredients.csv')

combined_shared_compounds.dropna()
combined_shared_compounds['Number of Shared Flavor Compounds'] = pd.to_numeric(combined_shared_compounds['Number of Shared Flavor Compounds'], errors='coerce')

ingredient_matrix = combined_shared_compounds.pivot_table(index='Ingredient1', columns='Ingredient2', values='Number of Shared Flavor Compounds', fill_value=0)

ingredient_matrix = (ingredient_matrix + ingredient_matrix.T) / 2

scaler = StandardScaler()
normalized_matrix = scaler.fit_transform(ingredient_matrix)
normalized_matrix = np.nan_to_num(normalized_matrix)

cosine_sim_matrix = cosine_similarity(normalized_matrix)
cosine_sim_df = pd.DataFrame(cosine_sim_matrix, index=ingredient_matrix.index, columns=ingredient_matrix.index)

def get_ingredient_type(ingredient):
    type_row = ingredients_pd[ingredients_pd['Ingredient'] == ingredient]
    if not type_row.empty:
        return type_row['Type'].values[0]
    return None

def compute_fit_scores_cosine(recipe_name, top_n=3):
    print(f"Processing recipe: {recipe_name}")
    
    recipe = recipes[recipes['recipeName'] == recipe_name]
    if recipe.empty:
        print(f"Recipe '{recipe_name}' not found.")
        return {}
    
    ingredients = recipe['ingredients'].values[0].split(',')
    
    additional_ingredients = ingredients_pd['Ingredient'].dropna().tolist()
    fit_scores = {}
    
    for ingredient in additional_ingredients:
        if ingredient not in cosine_sim_df.index or ingredient not in cosine_sim_df.columns or ingredient in ingredients:
            continue
        
        ingredient_type = get_ingredient_type(ingredient)
        
        temp_ingredients = [ing for ing in ingredients if ing in cosine_sim_df.index and ing in cosine_sim_df.columns]
        
        cosine_scores = [cosine_sim_df.at[ingredient, ing] for ing in temp_ingredients]
        
        if cosine_scores:
            avg_fit_score = sum(cosine_scores) / len(cosine_scores)
            if ingredient_type not in fit_scores:
                fit_scores[ingredient_type] = []
            fit_scores[ingredient_type].append((ingredient, avg_fit_score))
    
    top_recommendations = {}
    for ing_type, scores in fit_scores.items():
        sorted_scores = sorted(scores, key=lambda item: item[1], reverse=True)[:top_n]
        top_recommendations[ing_type] = sorted_scores
    
    return top_recommendations


@app.route("/ingredients")
def ingredients():
    desired_types = ['Fruit', 'Dairy', 'Spice', 'Vegetable', 'Additives and Essences', 'Animal Product', 'Beverage', 'Cereal', 'Nuts and Seeds', 'Fungus']
    
    insert_statements = []

    # Loop through all recipes in the DataFrame
    for index, row in recipes.iterrows():
        recipe_name = row['recipeName']
        
        # Query to get RecipeId for the current recipe_name
        recipe_id_query = f"SELECT RecipeId FROM Recipes WHERE Name = '{recipe_name}'"
        result = fetch_data(recipe_id_query)
        
        # Check if the query returned a result
        if not result.empty:
            recipe_id = result.iloc[0]['RecipeId']
            
            # Get the top recommendations
            top_recommendations = compute_fit_scores_cosine(recipe_name, top_n=3)
            
            # Filter recommendations by desired types
            filtered_recommendations = {k: v for k, v in top_recommendations.items() if k in desired_types}
            
            # Prepare the insert statements for each recommendation
            for ing_type, recommendations in filtered_recommendations.items():
                for ingredient, score in recommendations:
                    insert_statement = f"INSERT INTO Improvements VALUES ({recipe_id}, '{ing_type}', '{ingredient}', {score});"
                    insert_statements.append(insert_statement)
        else:
            print(f"No RecipeId found for recipe_name: {recipe_name}")

    # Define the file path
    file_path = 'C:\\Users\\Aurelian\\Licenta\\insert_improvements.sql'

    # Write the insert statements to the file
    with open(file_path, 'w') as file:
        file.write('\n'.join(insert_statements))
    
    return "The insert statements have been written to C:\\Users\\Aurelian\\Licenta\\insert_improvements.sql."


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
