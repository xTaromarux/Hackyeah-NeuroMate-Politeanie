using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class ImportResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public bool Success { get; set; }
        public int RecordsCount { get; set; }
        public int AverageHRV { get; set; }
        public int TotalSteps { get; set; }
        public int AverageSleepMinutes { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string RecordsJson { get; set; } = string.Empty;
    }
}
