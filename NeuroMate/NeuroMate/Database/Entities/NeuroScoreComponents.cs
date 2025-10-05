using SQLite;

namespace NeuroMate.Database.Entities
{
    public class NeuroScoreComponents
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SleepScore { get; set; }
        public int ActivityScore { get; set; }
        public int HeartScore { get; set; }
        public int CognitiveScore { get; set; }
        public int StressScore { get; set; }
    }
}
