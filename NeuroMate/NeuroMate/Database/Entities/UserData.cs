using SQLite;

namespace NeuroMate.Database.Entities
{
    public class UserData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NeuroScore { get; set; }
        public int MinutesNoBreak { get; set; }
        public int HRV { get; set; }
        public DateTime LastUpdate { get; set; }
        public string SelectedGoal { get; set; } = string.Empty;
    }
}
