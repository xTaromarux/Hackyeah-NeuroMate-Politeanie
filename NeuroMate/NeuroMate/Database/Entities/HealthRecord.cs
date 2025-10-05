using SQLite;

namespace NeuroMate.Database.Entities
{
    public class HealthRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        
        // Dodatkowe właściwości dla kompatybilności z DataImportService
        public DateTime Timestamp { get; set; }
        public int HeartRate { get; set; }
        public int HRV { get; set; }
        public int Steps { get; set; }
        public int SleepMinutes { get; set; }
    }
}
