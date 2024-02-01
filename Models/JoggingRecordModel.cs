namespace JoggingTrackerAPI.Models
{
    public class JoggingRecordModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double Distance { get; set; }
        public double Time { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
