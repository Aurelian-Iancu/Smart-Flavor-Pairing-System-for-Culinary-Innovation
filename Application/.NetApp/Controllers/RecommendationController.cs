using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace Backend.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public RecommendationController(AppDbContext context, IHttpClientFactory clientFactory, UserManager<AppUser> userManager)
        {
            _context = context;
            _clientFactory = clientFactory;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> GetRecommendation()
        {
            var recommendations = await FetchRecommendationsAsync();
            if (recommendations != null)
            {
                TempData["Recommendations"] = JsonConvert.SerializeObject(recommendations);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            List<Recipe> recommendations = new List<Recipe>();
            if (TempData["Recommendations"] != null)
            {
                recommendations = JsonConvert.DeserializeObject<List<Recipe>>(TempData["Recommendations"].ToString());
            }
            else
            {
                recommendations = await FetchRecommendationsAsync();
            }

            return View(recommendations);
        }

        private async Task<List<Recipe>> FetchRecommendationsAsync()
        {
            var client = _clientFactory.CreateClient();
            var url = "http://127.0.0.1:5000/recommend";
            var userId = _userManager.GetUserId(HttpContext.User);

            var requestContent = new StringContent(
                JsonConvert.SerializeObject(new { user_id = userId }),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await client.PostAsync(url, requestContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var recommendations = JsonConvert.DeserializeObject<List<Recipe>>(responseContent);

                List<Recipe> recommendationsRecipeList = new List<Recipe>();
                foreach (var recommendation in recommendations)
                {
                    Recipe recipeFromRecommendation = _context.Recipes.FirstOrDefault(r => r.RecipeId == recommendation.RecipeId);
                    if (recipeFromRecommendation != null)
                    {
                        recommendationsRecipeList.Add(recipeFromRecommendation);
                    }
                }

                return recommendationsRecipeList;
            }
            catch (HttpRequestException ex)
            {
                return null;
            }
        }
    }
}
