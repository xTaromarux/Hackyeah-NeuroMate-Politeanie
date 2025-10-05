using SQLite;

namespace NeuroMate.Database.Entities
{
    public class ActivityData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Steps { get; set; }
        public int CaloriesBurned { get; set; }
        public int ActiveMinutes { get; set; }
        public double DistanceKm { get; set; }
    }
}
