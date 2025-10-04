using NeuroMate.Database.Entities;
using SQLite;

namespace NeuroMate.Database
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "healthdata.db"
            );

            _database = new SQLiteAsyncConnection(dbPath);

            // Tworzymy tabele
            _database.CreateTableAsync<SleepData>().Wait();
            _database.CreateTableAsync<HeartData>().Wait();
            _database.CreateTableAsync<ActivityData>().Wait();
        }

        // ================================
        // SleepData
        // ================================
        public Task<List<SleepData>> GetAllSleepDataAsync()
        {
            return _database.Table<SleepData>().ToListAsync();
        }

        public Task SaveSleepDataAsync(SleepData data)
        {
            return _database.InsertOrReplaceAsync(data);
        }

        public Task DeleteSleepDataAsync(SleepData data)
        {
            return _database.DeleteAsync(data);
        }

        // ================================
        // HeartData
        // ================================
        public Task<List<HeartData>> GetAllHeartDataAsync()
        {
            return _database.Table<HeartData>().ToListAsync();
        }

        public Task SaveHeartDataAsync(HeartData data)
        {
            return _database.InsertOrReplaceAsync(data);
        }

        public Task DeleteHeartDataAsync(HeartData data)
        {
            return _database.DeleteAsync(data);
        }

        // ================================
        // ActivityData
        // ================================
        public Task<List<ActivityData>> GetAllActivityDataAsync()
        {
            return _database.Table<ActivityData>().ToListAsync();
        }

        public Task SaveActivityDataAsync(ActivityData data)
        {
            return _database.InsertOrReplaceAsync(data);
        }

        public Task DeleteActivityDataAsync(ActivityData data)
        {
            return _database.DeleteAsync(data);
        }
    }
}
