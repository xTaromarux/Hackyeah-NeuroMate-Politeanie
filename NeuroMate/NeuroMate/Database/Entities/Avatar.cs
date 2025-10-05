using SQLite;
using System;
using NeuroMate.Models;

namespace NeuroMate.Database.Entities
{
    public class Avatar
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Rarity { get; set; } = "Common"; // Przechowujemy jako string w bazie
        public bool IsUnlocked { get; set; } = false;
        public bool IsSelected { get; set; } = false;
        public DateTime UnlockedAt { get; set; }
        public int PlayerId { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Dodatkowe właściwości dla kompatybilności z AvatarShopPage
        public string LottieFileName { get; set; } = string.Empty;
        public int Price { get; set; } = 100;
        public bool IsDefault { get; set; } = false;
        public string PreviewImagePath { get; set; } = string.Empty;
        
        // Właściwość pomocnicza do konwersji rzadkości
        [Ignore]
        public AvatarRarity RarityEnum 
        { 
            get => Enum.TryParse<AvatarRarity>(Rarity, out var result) ? result : AvatarRarity.Common;
            set => Rarity = value.ToString();
        }
    }
}
