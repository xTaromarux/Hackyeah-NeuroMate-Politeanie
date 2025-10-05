using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class HealthRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int? HeartRate { get; set; }
        public int? HRV { get; set; }
        public int? Steps { get; set; }
        public int? SleepMinutes { get; set; }
    }
}
