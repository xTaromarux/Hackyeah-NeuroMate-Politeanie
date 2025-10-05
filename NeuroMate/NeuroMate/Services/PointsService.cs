using NeuroMate.Models;

namespace NeuroMate.Services
{
    public class PointsService : IPointsService
    {
        private PlayerProfile _playerProfile;
        private readonly List<PointsHistory> _pointsHistory = new();
        public event Action OnProfileChanged;

        public PointsService()
        {
            _playerProfile = new PlayerProfile
            {
                TotalPoints = 500, // Zmieniam z 50000 na 500 dla łatwiejszego testowania
                PointsSpent = 0,
                CurrentAvatarId = "hackyeah_default",
                UnlockedAvatarIds = new List<string> { "hackyeah_default" },
                TotalGamesPlayed = 0,
                TotalLootBoxesOpened = 0,
                LastPointsEarned = DateTime.Now
            };
        }

        public async Task<int> AddPointsForGameAsync(string gameType, int gameScore, int reactionTimeMs = 0)
        {
            await Task.Delay(1); // Symulacja async

            int pointsEarned = CalculatePointsForGame(gameType, gameScore, reactionTimeMs);

            _playerProfile.TotalPoints += pointsEarned;
            _playerProfile.TotalGamesPlayed++;
            _playerProfile.LastPointsEarned = DateTime.Now;

            // Dodaj do historii
            _pointsHistory.Add(new PointsHistory
            {
                Timestamp = DateTime.Now,
                PointsEarned = pointsEarned,
                Source = gameType,
                GameScore = gameScore,
                Description = $"Gra {gameType}: {gameScore} pkt, RT: {reactionTimeMs}ms"
            });

            OnProfileChanged.Invoke();
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
                _ => 10 // Domyślne punkty
            };

            // Bonus za wysokie wyniki
            if (gameScore >= 90) basePoints = (int)(basePoints * 1.5);
            else if (gameScore >= 80) basePoints = (int)(basePoints * 1.3);
            else if (gameScore >= 70) basePoints = (int)(basePoints * 1.1);

            return Math.Max(5, basePoints); // Minimum 5 punktów
        }

        private int CalculatePVTPoints(int reactionTimeMs, int accuracy)
        {
            // Im szybsza reakcja, tym więcej punktów
            int basePoints = reactionTimeMs switch
            {
                < 200 => 50,
                < 300 => 40,
                < 400 => 30,
                < 500 => 20,
                _ => 10
            };

            // Bonus za accuracy
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
                >= 95 => 50, // N-back jest trudniejsze
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
            await Task.Delay(1);
            return _playerProfile;
        }

        public async Task<List<PointsHistory>> GetPointsHistoryAsync(int days = 30)
        {
            await Task.Delay(1);
            var cutoffDate = DateTime.Now.AddDays(-days);
            return _pointsHistory.Where(h => h.Timestamp >= cutoffDate)
                                .OrderByDescending(h => h.Timestamp)
                                .ToList();
        }

        public async Task SavePlayerProfileAsync(PlayerProfile profile)
        {
            await Task.Delay(1);
            _playerProfile = profile;
            OnProfileChanged.Invoke();
        }

        public async Task<bool> SpendPointsAsync(int amount)
        {
            await Task.Delay(1);
            if (_playerProfile.TotalPoints >= amount)
            {
                _playerProfile.TotalPoints -= amount;
                _playerProfile.PointsSpent += amount;
                OnProfileChanged.Invoke();
                return true;
            }
            OnProfileChanged.Invoke();
            return false;
        }
    }
}
