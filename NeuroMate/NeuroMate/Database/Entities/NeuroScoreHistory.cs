using SQLite;

namespace NeuroMate.Database.Entities
{
    public class NeuroScoreHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int OverallScore { get; set; }
        public string ComponentsJson { get; set; } = string.Empty;
    }
}
