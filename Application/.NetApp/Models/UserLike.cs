using Backend.Migrations;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class UserLike
    {
        [Key]
        public long UserLikesId { get; set; }
        [Required]
        public AppUser User { get; set; }
        [Required]
        public Recipe Recipe { get; set; }
    }
}
