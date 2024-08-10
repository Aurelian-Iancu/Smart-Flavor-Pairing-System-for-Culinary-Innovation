using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Improvement
    {
        [Key]
        public long ImprovementId { get; set; }
        [Required]
        public Recipe Recipe { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Ingredient { get; set; }
        [Required]
        public float DegreeOfImprovement { get; set; }
    }
}
