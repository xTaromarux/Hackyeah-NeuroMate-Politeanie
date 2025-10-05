using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class PointsHistoryData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int PointsEarned { get; set; }
        public string Source { get; set; } = string.Empty;
        public int GameScore { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
