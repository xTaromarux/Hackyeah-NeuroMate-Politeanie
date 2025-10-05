using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class LootBoxResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UnlockedAvatarId { get; set; }
        public bool WasNewAvatar { get; set; }
        public int PointsRefunded { get; set; }
        public DateTime OpenedAt { get; set; }
    }
}
