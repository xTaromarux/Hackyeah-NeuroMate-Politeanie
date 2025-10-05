using SQLite;

namespace NeuroMate.Database.Entities
{
    public class ImportResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime ImportDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public int RecordsImported { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
