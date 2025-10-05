using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class LootBox
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
