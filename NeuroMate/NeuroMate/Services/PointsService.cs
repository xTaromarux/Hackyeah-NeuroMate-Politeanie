using NeuroMate.Models;
using NeuroMate.Database;
using NeuroMate.Database.Entities;
using System.Text.Json;

namespace NeuroMate.Services
{
    public class PointsService : IPointsService
    {
        private PlayerProfile _playerProfile;
        private readonly DatabaseService _db;
        public event Action OnProfileChanged;

        public PointsService(DatabaseService db)
        {
            _db = db;
            // Wczytaj profil gracza z bazy lub utwórz nowy
            var profiles = _db.GetAllPlayerProfileDataAsync().Result;
            var profileData = profiles.FirstOrDefault();
            _playerProfile = profileData != null ? MapToBusiness(profileData) : new PlayerProfile
            {
                TotalPoints = 500,
                PointsSpent = 0,
                CurrentAvatarId = "hackyeah_default",
                UnlockedAvatarIds = new List<string> { "hackyeah_default" },
                TotalGamesPlayed = 0,
                TotalLootBoxesOpened = 0,
                LastPointsEarned = DateTime.Now
            };
            if (profileData == null)
            {
                var newData = MapToData(_playerProfile);
                _db.SavePlayerProfileDataAsync(newData).Wait();
            }
        }

        public async Task<int> AddPointsForGameAsync(string gameType, int gameScore, int reactionTimeMs = 0)
        {
            int pointsEarned = CalculatePointsForGame(gameType, gameScore, reactionTimeMs);

            _playerProfile.TotalPoints += pointsEarned;
            _playerProfile.TotalGamesPlayed++;
            _playerProfile.LastPointsEarned = DateTime.Now;
            await _db.SavePlayerProfileDataAsync(MapToData(_playerProfile));

            var history = new PointsHistoryData
            {
                Timestamp = DateTime.Now,
                PointsEarned = pointsEarned,
                Source = gameType,
                GameScore = gameScore,
                Description = $"Gra {gameType}: {gameScore} pkt, RT: {reactionTimeMs}ms"
            };
            await _db.SavePointsHistoryDataAsync(history);

            OnProfileChanged?.Invoke();
            return pointsEarned;
        }

        private int CalculatePointsForGame(string gameType, int gameScore, int reactionTimeMs)
        {
            int basePoints = gameType switch
            {
                "PVT" => CalculatePVTPoints(reactionTimeMs, gameScore),
                "Stroop" => CalculateStroopPoints(gameScore),
                "NBack" => CalculateNBackPoints(gameScore),
                "TaskSwitching" => CalculateTaskSwitchingPoints(gameScore),
                _ => 10
            };

            if (gameScore >= 90) basePoints = (int)(basePoints * 1.5);
            else if (gameScore >= 80) basePoints = (int)(basePoints * 1.3);
            else if (gameScore >= 70) basePoints = (int)(basePoints * 1.1);

            return Math.Max(5, basePoints);
        }

        private int CalculatePVTPoints(int reactionTimeMs, int accuracy)
        {
            int basePoints = reactionTimeMs switch
            {
                < 200 => 50,
                < 300 => 40,
                < 400 => 30,
                < 500 => 20,
                _ => 10
            };
            return basePoints + (accuracy / 10);
        }

        private int CalculateStroopPoints(int accuracy)
        {
            return accuracy switch
            {
                >= 95 => 45,
                >= 90 => 40,
                >= 85 => 35,
                >= 80 => 30,
                >= 75 => 25,
                _ => 15
            };
        }

        private int CalculateNBackPoints(int accuracy)
        {
            return accuracy switch
            {
                >= 95 => 50,
                >= 90 => 45,
                >= 85 => 40,
                >= 80 => 35,
                >= 75 => 30,
                _ => 20
            };
        }

        private int CalculateTaskSwitchingPoints(int accuracy)
        {
            return accuracy switch
            {
                >= 95 => 40,
                >= 90 => 35,
                >= 85 => 30,
                >= 80 => 25,
                >= 75 => 20,
                _ => 12
            };
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync()
        {
            var profiles = await _db.GetAllPlayerProfileDataAsync();
            var profileData = profiles.FirstOrDefault();
            _playerProfile = profileData != null ? MapToBusiness(profileData) : _playerProfile;
            return _playerProfile;
        }

        public async Task<List<PointsHistory>> GetPointsHistoryAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var allHistoryData = await _db.GetAllPointsHistoryDataAsync();
            var allHistory = allHistoryData.Select(MapToBusiness).ToList();
            return allHistory.Where(h => h.Timestamp >= cutoffDate)
                             .OrderByDescending(h => h.Timestamp)
                             .ToList();
        }

        public async Task SavePlayerProfileAsync(PlayerProfile profile)
        {
            await _db.SavePlayerProfileDataAsync(MapToData(profile));
            _playerProfile = profile;
            OnProfileChanged?.Invoke();
        }

        public async Task<bool> SpendPointsAsync(int amount)
        {
            if (_playerProfile.TotalPoints >= amount)
            {
                _playerProfile.TotalPoints -= amount;
                _playerProfile.PointsSpent += amount;
                await _db.SavePlayerProfileDataAsync(MapToData(_playerProfile));
                OnProfileChanged?.Invoke();
                return true;
            }
            OnProfileChanged?.Invoke();
            return false;
        }

        // Mapowanie encji bazodanowej na model biznesowy
        private PlayerProfile MapToBusiness(PlayerProfileData data)
        {
            return new PlayerProfile
            {
                TotalPoints = data.TotalPoints,
                PointsSpent = data.PointsSpent,
                CurrentAvatarId = data.CurrentAvatarId,
                UnlockedAvatarIds = string.IsNullOrEmpty(data.UnlockedAvatarIdsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(data.UnlockedAvatarIdsJson) ?? new List<string>(),
                TotalGamesPlayed = data.TotalGamesPlayed,
                TotalLootBoxesOpened = data.TotalLootBoxesOpened,
                LastPointsEarned = data.LastPointsEarned
            };
        }

        // Mapowanie modelu biznesowego na encję bazodanową
        private PlayerProfileData MapToData(PlayerProfile profile)
        {
            return new PlayerProfileData
            {
                TotalPoints = profile.TotalPoints,
                PointsSpent = profile.PointsSpent,
                CurrentAvatarId = profile.CurrentAvatarId,
                UnlockedAvatarIdsJson = JsonSerializer.Serialize(profile.UnlockedAvatarIds),
                TotalGamesPlayed = profile.TotalGamesPlayed,
                TotalLootBoxesOpened = profile.TotalLootBoxesOpened,
                LastPointsEarned = profile.LastPointsEarned
            };
        }

        // Mapowanie encji bazodanowej na model biznesowy PointsHistory
        private PointsHistory MapToBusiness(PointsHistoryData data)
        {
            return new PointsHistory
            {
                Timestamp = data.Timestamp,
                PointsEarned = data.PointsEarned,
                Source = data.Source,
                GameScore = data.GameScore,
                Description = data.Description
            };
        }
    }
}
