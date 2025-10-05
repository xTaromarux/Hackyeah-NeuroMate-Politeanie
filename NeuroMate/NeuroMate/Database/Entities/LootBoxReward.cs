using SQLite;

namespace NeuroMate.Database.Entities
{
    public class LootBoxReward
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int LootBoxId { get; set; }
        public string RewardType { get; set; } = string.Empty; // "Avatar", "Points"
        public string RewardValue { get; set; } = string.Empty; // AvatarId lub ilość punktów
        public float DropChance { get; set; } // 0.0 - 1.0 (procent szansy)
        public string Rarity { get; set; } = "Common";
        public bool IsActive { get; set; } = true;
    }
}
