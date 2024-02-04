namespace JoggingTrackerAPI.Models
{
    public class JoggingTrackerResource
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public DateTime Date { get; set; }
        public double Distance { get; set; }
        public TimeSpan Time { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
