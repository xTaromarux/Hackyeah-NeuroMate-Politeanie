using SQLite;

namespace NeuroMate.Database.Entities
{
    public class Intervention
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string InterventionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; }
        public int DurationSeconds { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string AvatarMessage { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
}
