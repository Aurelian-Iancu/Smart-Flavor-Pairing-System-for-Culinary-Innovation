using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class UserLikeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UserLikeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            // Get liked recipes for the current user
            List<Recipe> likedRecipes = _context.UserLikes
                                                .Where(ul => ul.User.Id == userId)
                                                .Select(ul => ul.Recipe)
                                                .ToList();

            return View(likedRecipes);
        }


        [HttpGet]
        [Authorize]
        public void addRecipeToUserProfile(long recipeId)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            
            AppUser activeUser = _userManager.Users.FirstOrDefault(user => user.Id == userId);
            Recipe recipe = _context.Recipes.FirstOrDefault(recipe => recipe.RecipeId == recipeId);
            UserLike newUserProfile = new UserLike();

            if (activeUser != null && recipe != null)
            {
                
                newUserProfile.User = activeUser;
                newUserProfile.Recipe = recipe;
            }
            

            _context.Add(newUserProfile);
            _context.SaveChanges();
            
        }

        [HttpGet]
        [Authorize]
        public void removeRecipeFromUserProfile(long recipeId)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            AppUser activeUser = _userManager.Users.FirstOrDefault(user => user.Id == userId);
            Recipe recipe = _context.Recipes.FirstOrDefault(recipe => recipe.RecipeId == recipeId);

            UserLike userLike = _context.UserLikes.FirstOrDefault(ul => ul.Recipe.RecipeId == recipe.RecipeId && ul.User.Id == userId);

            if(userLike != null)
            {
                _context.Remove(userLike);
                _context.SaveChanges();
            }
        }
    }
}
