using System.ComponentModel.DataAnnotations;

namespace JoggingTrackerAPI.Models
{
    public class UserRegister
    {
        [Required]
        public string UserLoginName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

}
