using SQLite;

namespace NeuroMate.Database.Entities
{
    public class LootBoxReward
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AvatarId { get; set; } = string.Empty;
        public double DropChance { get; set; }
        public int Rarity { get; set; }
        public int LootBoxId { get; set; }
    }
}
