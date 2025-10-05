using SQLite;

namespace NeuroMate.Database.Entities
{
    public class HeartData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int HeartRate { get; set; }
        public int HRV { get; set; }
        public string Context { get; set; } = string.Empty;
    }
}
