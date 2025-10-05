using SQLite;

namespace NeuroMate.Database.Entities
{
    public class SleepData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SleepDurationMinutes { get; set; }
        public int DeepSleepMinutes { get; set; }
        public int RemSleepMinutes { get; set; }
        public int WakeUpCount { get; set; }
        public int SleepEfficiency { get; set; }
    }
}
