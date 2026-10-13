using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}