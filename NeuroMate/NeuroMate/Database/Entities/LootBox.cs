using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class LootBox
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string LootBoxId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public string PossibleRewardsJson { get; set; } = string.Empty;
    }
}
