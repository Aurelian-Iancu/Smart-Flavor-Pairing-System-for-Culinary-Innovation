namespace Backend.ViewModels
{
    public class RecipeImprovementsVM
    {
        public string RecipeName { get; set; }
        public List<ImprovementDetail> OverallBestImprovements { get; set; }
        public List<ImprovementStatisticsVM> TopImprovementsByType { get; set; }
    }
}
