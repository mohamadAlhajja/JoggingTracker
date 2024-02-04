using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoggingTrackerAPI.Data.Entities
{
    public class JoggingTrackerEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public UserEntity? User { get; set; }
        public DateTime Date { get; set; }
        public double Distance { get; set; }
        public TimeSpan Time { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
