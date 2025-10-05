using SQLite;
using System;
using System.Collections.Generic;

namespace NeuroMate.Database.Entities
{
    public class PlayerProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TotalPoints { get; set; }
        public int PointsSpent { get; set; }
        public string CurrentAvatarId { get; set; } = string.Empty;
        public string UnlockedAvatarIdsJson { get; set; } = string.Empty;
        public int TotalGamesPlayed { get; set; }
        public int TotalLootBoxesOpened { get; set; }
        public DateTime LastPointsEarned { get; set; }
    }
}
