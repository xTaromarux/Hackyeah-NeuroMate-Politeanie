using SQLite;

namespace NeuroMate.Database.Entities
{
    public class ReactionRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ReactionTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsValid { get; set; }
    }
}
