using SQLite;

namespace NeuroMate.Database.Entities
{
    public class InterventionResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string InterventionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Completed { get; set; }
        public int ScoreBeforeIntervention { get; set; }
        public int ScoreAfterIntervention { get; set; }
        public string MetricsJson { get; set; } = string.Empty; // Stored as JSON string
    }
}
