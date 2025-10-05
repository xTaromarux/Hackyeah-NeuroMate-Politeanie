using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class LootBoxResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int LootBoxId { get; set; }
        public int PlayerId { get; set; }
        public string RewardType { get; set; } = string.Empty; // "Avatar", "Points"
        public string RewardValue { get; set; } = string.Empty; // AvatarId lub ilość punktów
        public bool IsWin { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public int PointsSpent { get; set; }
        public int PointsGained { get; set; }
    }
}
