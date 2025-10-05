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

            // Tworzymy tabele dla wszystkich encji
            _database.CreateTableAsync<SleepData>().Wait();
            _database.CreateTableAsync<HeartData>().Wait();
            _database.CreateTableAsync<ActivityData>().Wait();
            _database.CreateTableAsync<NeuroScoreHistory>().Wait();
            _database.CreateTableAsync<NeuroScoreComponents>().Wait();
            _database.CreateTableAsync<UserData>().Wait();
            _database.CreateTableAsync<PlayerProfile>().Wait();
            _database.CreateTableAsync<PointsHistory>().Wait();
            _database.CreateTableAsync<Avatar>().Wait();
            _database.CreateTableAsync<LootBox>().Wait();
            _database.CreateTableAsync<LootBoxReward>().Wait();
            _database.CreateTableAsync<LootBoxResult>().Wait();
            _database.CreateTableAsync<Intervention>().Wait();
            _database.CreateTableAsync<InterventionResult>().Wait();
            _database.CreateTableAsync<ReactionRecord>().Wait();
            _database.CreateTableAsync<HealthRecord>().Wait();
            _database.CreateTableAsync<ImportResult>().Wait();
        }

        // SleepData
        public Task<List<SleepData>> GetAllSleepDataAsync() => _database.Table<SleepData>().ToListAsync();
        public Task SaveSleepDataAsync(SleepData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteSleepDataAsync(SleepData data) => _database.DeleteAsync(data);

        // HeartData
        public Task<List<HeartData>> GetAllHeartDataAsync() => _database.Table<HeartData>().ToListAsync();
        public Task SaveHeartDataAsync(HeartData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteHeartDataAsync(HeartData data) => _database.DeleteAsync(data);

        // ActivityData
        public Task<List<ActivityData>> GetAllActivityDataAsync() => _database.Table<ActivityData>().ToListAsync();
        public Task SaveActivityDataAsync(ActivityData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteActivityDataAsync(ActivityData data) => _database.DeleteAsync(data);

        // NeuroScoreHistory
        public Task<List<NeuroScoreHistory>> GetAllNeuroScoreHistoryAsync() => _database.Table<NeuroScoreHistory>().ToListAsync();
        public Task SaveNeuroScoreHistoryAsync(NeuroScoreHistory data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteNeuroScoreHistoryAsync(NeuroScoreHistory data) => _database.DeleteAsync(data);

        // NeuroScoreComponents
        public Task<List<NeuroScoreComponents>> GetAllNeuroScoreComponentsAsync() => _database.Table<NeuroScoreComponents>().ToListAsync();
        public Task SaveNeuroScoreComponentsAsync(NeuroScoreComponents data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteNeuroScoreComponentsAsync(NeuroScoreComponents data) => _database.DeleteAsync(data);

        // UserData
        public Task<List<UserData>> GetAllUserDataAsync() => _database.Table<UserData>().ToListAsync();
        public Task SaveUserDataAsync(UserData data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteUserDataAsync(UserData data) => _database.DeleteAsync(data);

        // PlayerProfile
        public Task<List<PlayerProfile>> GetAllPlayerProfilesAsync() => _database.Table<PlayerProfile>().ToListAsync();
        public Task SavePlayerProfileAsync(PlayerProfile data) => _database.InsertOrReplaceAsync(data);
        public Task DeletePlayerProfileAsync(PlayerProfile data) => _database.DeleteAsync(data);

        // PointsHistory
        public Task<List<PointsHistory>> GetAllPointsHistoryAsync() => _database.Table<PointsHistory>().ToListAsync();
        public Task SavePointsHistoryAsync(PointsHistory data) => _database.InsertOrReplaceAsync(data);
        public Task DeletePointsHistoryAsync(PointsHistory data) => _database.DeleteAsync(data);

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
        public Task<List<Intervention>> GetAllInterventionsAsync() => _database.Table<Intervention>().ToListAsync();
        public Task SaveInterventionAsync(Intervention data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteInterventionAsync(Intervention data) => _database.DeleteAsync(data);

        // InterventionResult
        public Task<List<InterventionResult>> GetAllInterventionResultsAsync() => _database.Table<InterventionResult>().ToListAsync();
        public Task SaveInterventionResultAsync(InterventionResult data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteInterventionResultAsync(InterventionResult data) => _database.DeleteAsync(data);

        // ReactionRecord
        public Task<List<ReactionRecord>> GetAllReactionRecordsAsync() => _database.Table<ReactionRecord>().ToListAsync();
        public Task SaveReactionRecordAsync(ReactionRecord data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteReactionRecordAsync(ReactionRecord data) => _database.DeleteAsync(data);

        // HealthRecord
        public Task<List<HealthRecord>> GetAllHealthRecordsAsync() => _database.Table<HealthRecord>().ToListAsync();
        public Task SaveHealthRecordAsync(HealthRecord data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteHealthRecordAsync(HealthRecord data) => _database.DeleteAsync(data);

        // ImportResult
        public Task<List<ImportResult>> GetAllImportResultsAsync() => _database.Table<ImportResult>().ToListAsync();
        public Task SaveImportResultAsync(ImportResult data) => _database.InsertOrReplaceAsync(data);
        public Task DeleteImportResultAsync(ImportResult data) => _database.DeleteAsync(data);
    }
}
