using System.ComponentModel.DataAnnotations;

namespace CerealApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        // We'll store the hashed password here
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Roles for future development
        public string Role { get; set; } = "User";
    }
}
