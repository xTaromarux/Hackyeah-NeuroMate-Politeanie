using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class Avatar
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AvatarId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LottieFileName { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsDefault { get; set; }
        public int Rarity { get; set; }
        public string PreviewImagePath { get; set; } = string.Empty;
    }
}
