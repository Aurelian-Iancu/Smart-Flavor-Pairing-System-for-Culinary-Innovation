using Microsoft.AspNetCore.Mvc;
using Backend.Models; // Adjust this namespace based on your project structure
using System.Linq;
using System.Collections.Generic;
using Backend.Data;
using Backend.ViewModels;

namespace Backend.Controllers
{
    public class ImprovementController : Controller
    {
        private readonly AppDbContext _context;

        public ImprovementController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int id)
        {
            var recipe = _context.Recipes.FirstOrDefault(r => r.RecipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }

            var improvements = _context.Improvements
                .Where(i => i.Recipe.RecipeId == id)
                .ToList();

            if (improvements == null || improvements.Count == 0)
            {
                return NotFound();
            }

            var overallBestImprovements = improvements
                .OrderByDescending(i => i.DegreeOfImprovement)
                .Take(3)
                .Select(i => new ImprovementDetail
                {
                    Ingredient = i.Ingredient,
                    DegreeOfImprovement = i.DegreeOfImprovement
                })
                .ToList();

            var topImprovementsByType = improvements
                .GroupBy(i => i.Type)
                .Select(g => new ImprovementStatisticsVM
                {
                    Type = g.Key,
                    TopImprovements = g.OrderByDescending(i => i.DegreeOfImprovement)
                                       .Take(3)
                                       .Select(i => new ImprovementDetail
                                       {
                                           Ingredient = i.Ingredient,
                                           DegreeOfImprovement = i.DegreeOfImprovement
                                       })
                                       .ToList()
                })
                .ToList();

            var viewModel = new RecipeImprovementsVM
            {
                RecipeName = recipe.Name,
                OverallBestImprovements = overallBestImprovements,
                TopImprovementsByType = topImprovementsByType
            };

            return View(viewModel);
        }
    }

}
