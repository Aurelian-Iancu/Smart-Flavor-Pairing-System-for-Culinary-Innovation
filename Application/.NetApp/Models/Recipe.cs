using Backend.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Recipe
    {
        [Key]
        public long RecipeId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Required]
        public int TotalTimeInSeconds { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string? Course { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string? Cuisine { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string? Ingredients { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string? Link { get; set; }
    }
}
