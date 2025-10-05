using NeuroMate.Database.Entities;
using NeuroMate.Models;
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

            // Tworzymy tabele dla wszystkich encji - używamy typów z Database.Entities
            _database.CreateTableAsync<Entities.SleepData>().Wait();
            _database.CreateTableAsync<Entities.HeartData>().Wait();
            _database.CreateTableAsync<Entities.ActivityData>().Wait();
            _database.CreateTableAsync<Entities.NeuroScoreHistory>().Wait();
            _database.CreateTableAsync<Entities.NeuroScoreComponents>().Wait();
            _database.CreateTableAsync<Entities.UserData>().Wait();
            _database.CreateTableAsync<Models.PlayerProfileData>().Wait();
            _database.CreateTableAsync<Models.PointsHistoryData>().Wait();
            _database.CreateTableAsync<Avatar>().Wait();
            _database.CreateTableAsync<LootBox>().Wait();
            _database.CreateTableAsync<LootBoxReward>().Wait();
            _database.CreateTableAsync<LootBoxResult>().Wait();
            _database.CreateTableAsync<Entities.Intervention>().Wait();
            _database.CreateTableAsync<Entities.InterventionResult>().Wait();
            _database.CreateTableAsync<Entities.GameReactionRecord>().Wait();
            _database.CreateTableAsync<Entities.HealthRecord>().Wait();
            _database.CreateTableAsync<Entities.ImportResult>().Wait();
        }

        // SleepData - używamy typów z Database.Entities
        public Task<List<Entities.SleepData>> GetAllSleepDataAsync() => _database.Table<Entities.SleepData>().ToListAsync();
        public Task SaveSleepDataAsync(Entities.SleepData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteSleepDataAsync(Entities.SleepData data) => _database.DeleteAsync(data);

        // HeartData
        public Task<List<Entities.HeartData>> GetAllHeartDataAsync() => _database.Table<Entities.HeartData>().ToListAsync();
        public Task SaveHeartDataAsync(Entities.HeartData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteHeartDataAsync(Entities.HeartData data) => _database.DeleteAsync(data);

        // ActivityData
        public Task<List<Entities.ActivityData>> GetAllActivityDataAsync() => _database.Table<Entities.ActivityData>().ToListAsync();
        public Task SaveActivityDataAsync(Entities.ActivityData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteActivityDataAsync(Entities.ActivityData data) => _database.DeleteAsync(data);

        // NeuroScoreHistory
        public Task<List<Entities.NeuroScoreHistory>> GetAllNeuroScoreHistoryAsync() => _database.Table<Entities.NeuroScoreHistory>().ToListAsync();
        public Task SaveNeuroScoreHistoryAsync(Entities.NeuroScoreHistory data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteNeuroScoreHistoryAsync(Entities.NeuroScoreHistory data) => _database.DeleteAsync(data);

        // NeuroScoreComponents
        public Task<List<Entities.NeuroScoreComponents>> GetAllNeuroScoreComponentsAsync() => _database.Table<Entities.NeuroScoreComponents>().ToListAsync();
        public Task SaveNeuroScoreComponentsAsync(Entities.NeuroScoreComponents data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteNeuroScoreComponentsAsync(Entities.NeuroScoreComponents data) => _database.DeleteAsync(data);

        // UserData
        public Task<List<Entities.UserData>> GetAllUserDataAsync() => _database.Table<Entities.UserData>().ToListAsync();
        public Task SaveUserDataAsync(Entities.UserData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteUserDataAsync(Entities.UserData data) => _database.DeleteAsync(data);

        // PlayerProfileData - używamy typu z Models
        public Task<List<Models.PlayerProfileData>> GetAllPlayerProfileDataAsync() => _database.Table<Models.PlayerProfileData>().ToListAsync();
        public Task SavePlayerProfileDataAsync(Models.PlayerProfileData data) => _database.InsertOrReplaceAsync(data);
        public Task DeletePlayerProfileDataAsync(Models.PlayerProfileData data) => _database.DeleteAsync(data);

        // PointsHistoryData - używamy typu z Models
        public Task<List<Models.PointsHistoryData>> GetAllPointsHistoryDataAsync() => _database.Table<Models.PointsHistoryData>().ToListAsync();
        public Task SavePointsHistoryDataAsync(Models.PointsHistoryData data) => _database.InsertOrReplaceAsync(data);
        public Task DeletePointsHistoryDataAsync(Models.PointsHistoryData data) => _database.DeleteAsync(data);

        // Avatar
        public Task<List<Avatar>> GetAllAvatarsAsync() => _database.Table<Avatar>().ToListAsync();
        public Task SaveAvatarAsync(Avatar data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteAvatarAsync(Avatar data) => _database.DeleteAsync(data);

        // LootBox
        public Task<List<LootBox>> GetAllLootBoxesAsync() => _database.Table<LootBox>().ToListAsync();
        public Task SaveLootBoxAsync(LootBox data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteLootBoxAsync(LootBox data) => _database.DeleteAsync(data);

        // LootBoxReward
        public Task<List<LootBoxReward>> GetAllLootBoxRewardsAsync() => _database.Table<LootBoxReward>().ToListAsync();
        public Task SaveLootBoxRewardAsync(LootBoxReward data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteLootBoxRewardAsync(LootBoxReward data) => _database.DeleteAsync(data);

        // LootBoxResult
        public Task<List<LootBoxResult>> GetAllLootBoxResultsAsync() => _database.Table<LootBoxResult>().ToListAsync();
        public Task SaveLootBoxResultAsync(LootBoxResult data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteLootBoxResultAsync(LootBoxResult data) => _database.DeleteAsync(data);

        // Intervention
        public Task<List<Entities.Intervention>> GetAllInterventionsAsync() => _database.Table<Entities.Intervention>().ToListAsync();
        public Task SaveInterventionAsync(Entities.Intervention data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteInterventionAsync(Entities.Intervention data) => _database.DeleteAsync(data);

        // InterventionResult
        public Task<List<Entities.InterventionResult>> GetAllInterventionResultsAsync() => _database.Table<Entities.InterventionResult>().ToListAsync();
        public Task SaveInterventionResultAsync(Entities.InterventionResult data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteInterventionResultAsync(Entities.InterventionResult data) => _database.DeleteAsync(data);

        // ReactionRecord
        public Task<List<Entities.GameReactionRecord>> GetAllReactionRecordsAsync() => _database.Table<Entities.GameReactionRecord>().ToListAsync();
        public Task SaveGameReactionRecordAsync(Entities.GameReactionRecord data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteGameReactionRecordAsync(Entities.GameReactionRecord data) => _database.DeleteAsync(data);

        // HealthRecord
        public Task<List<Entities.HealthRecord>> GetAllHealthRecordsAsync() => _database.Table<Entities.HealthRecord>().ToListAsync();
        public Task SaveHealthRecordAsync(Entities.HealthRecord data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteHealthRecordAsync(Entities.HealthRecord data) => _database.DeleteAsync(data);

        // ImportResult
        public Task<List<Entities.ImportResult>> GetAllImportResultsAsync() => _database.Table<Entities.ImportResult>().ToListAsync();
        public Task SaveImportResultAsync(Entities.ImportResult data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteImportResultAsync(Entities.ImportResult data) => _database.DeleteAsync(data);
    }
}
