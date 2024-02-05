using System.ComponentModel.DataAnnotations;

namespace JoggingTrackerAPI.Models
{
    public class JoggingTrackerModel
    {
        [Required]
        public string UserId { get; set; } = default!;
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public double Distance { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
        [Required]
        public string Location { get; set; } = string.Empty;
    }
}
