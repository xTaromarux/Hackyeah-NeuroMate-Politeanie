using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class NeuroScoreHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int Score { get; set; }
        public string Trigger { get; set; } = string.Empty;
        // Mo¿na dodaæ serializacjê komponentów jako JSON
        public string ComponentsJson { get; set; } = string.Empty;
    }
}
