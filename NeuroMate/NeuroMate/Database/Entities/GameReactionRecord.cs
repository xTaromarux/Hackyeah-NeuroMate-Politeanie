using SQLite;

namespace NeuroMate.Database.Entities
{
    public class GameReactionRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ReactionTimeMs { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsValid { get; set; }
    }
}
