using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public RecipeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }
        [Authorize]
        public IActionResult Index(int pg = 1, string term = "")
        {
            Dictionary<long, bool> likedRecipes = getLikes();
            ViewBag.LikedRecipes = likedRecipes;

            term = term.ToLower();

            const int pageSize = 9;

            pg = Math.Max(pg, 1);

            var query = _context.Recipes.AsQueryable();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(rec => rec.Name.ToLower().Contains(term));
            }

            int recsCount = query.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = query.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }


        public Dictionary<long, bool> getLikes()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            List<Recipe> recipes = _context.Recipes.ToList();

            Dictionary<long, bool> likedRecipes = new Dictionary<long, bool>();

            foreach (var recipe in recipes)
            {
                bool isLikedByUser = _context.UserLikes.Any(ul => ul.Recipe.RecipeId == recipe.RecipeId && ul.User.Id == userId);

                likedRecipes[recipe.RecipeId] = isLikedByUser;
            }

            return likedRecipes;
        }

    }
}
