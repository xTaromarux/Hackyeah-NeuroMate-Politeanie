using NeuroMate.Models;
using NeuroMate.Database;
using NeuroMate.Database.Entities;

namespace NeuroMate.Services
{
    public class NeuroScoreService : INeuroScoreService
    {
        private readonly DatabaseService _db;
        private Models.UserData _currentUserData;
        private Models.NeuroScoreComponents _lastComponents;
        private readonly List<Models.NeuroScoreHistory> _scoreHistory = new();

        // Wartości referencyjne dla normalizacji
        private const int OPTIMAL_REACTION_TIME_MS = 250;
        private const int POOR_REACTION_TIME_MS = 600;
        private const int OPTIMAL_BREAK_INTERVAL_MIN = 50;
        private const int OPTIMAL_HRV = 70;
        private const int OPTIMAL_SLEEP_MINUTES = 480; // 8h

        public NeuroScoreService(DatabaseService db)
        {
            _db = db;

            _currentUserData = new Models.UserData
            {
                NeuroScore = 50,
                MinutesNoBreak = 0,
                HRV = 0,
                LastUpdate = DateTime.Now
            };
            _lastComponents = new Models.NeuroScoreComponents();
        }

        public Task<int> CalculateNeuroScoreAsync(
            PVTStatistics pvtStats,
            int minutesNoBreak,
            int hrv,
            int? sleepMinutes = null)
        {
            var components = new Models.NeuroScoreComponents();

            // 1. Reaction Time Score (0-1, mniejszy = lepszy)
            if (pvtStats.AverageReactionMs > 0)
            {
                double rtNormalized = 1.0 - Math.Clamp(
                    (pvtStats.AverageReactionMs - OPTIMAL_REACTION_TIME_MS) /
                    (double)(POOR_REACTION_TIME_MS - OPTIMAL_REACTION_TIME_MS),
                    0, 1
                );
                components.ReactionTimeScore = rtNormalized;
            }
            else
            {
                components.ReactionTimeScore = 0.5; // Domyślna wartość
            }

            // 2. Accuracy Score (na razie uproszczone, można rozbudować)
            components.AccuracyScore = pvtStats.TrialsCount > 0 ? 0.8 : 0.5;

            // 3. Break Time Score (0-1, im bliżej optymalnego, tym lepiej)
            double breakScore = 1.0 - Math.Clamp(
                Math.Abs(minutesNoBreak - OPTIMAL_BREAK_INTERVAL_MIN) / 100.0,
                0, 1
            );
            components.BreakTimeScore = breakScore;

            // 4. HRV Score (0-1)
            if (hrv > 0)
            {
                double hrvScore = Math.Clamp(hrv / (double)OPTIMAL_HRV, 0, 1);
                components.HRVScore = hrvScore;
            }
            else
            {
                components.HRVScore = 0.5; // Brak danych
            }

            // 5. Sleep Score (opcjonalnie)
            if (sleepMinutes.HasValue && sleepMinutes.Value > 0)
            {
                double sleepScore = Math.Clamp(
                    sleepMinutes.Value / (double)OPTIMAL_SLEEP_MINUTES,
                    0, 1
                );
                components.SleepScoreNormalized = sleepScore;
            }

            // Oblicz końcowy score (0-100) używając wag
            double weightedScore =
                components.ReactionTimeScore * components.Weights["ReactionTime"] +
                components.AccuracyScore * components.Weights["Accuracy"] +
                components.BreakTimeScore * components.Weights["BreakTime"] +
                components.HRVScore * components.Weights["HRV"];

            int finalScore = (int)Math.Round(weightedScore * 100);
            finalScore = Math.Clamp(finalScore, 0, 100);

            // Zapisz komponenty
            _lastComponents = components;

            // Zapisz do historii - używam modelu z namespace Models
            var historyRecord = new Models.NeuroScoreHistory
            {
                Timestamp = DateTime.Now,
                Score = finalScore,
                Components = components,
                Trigger = "Manual calculation"
            };
            _scoreHistory.Add(historyRecord);

            // Aktualizuj dane użytkownika
            _currentUserData.NeuroScore = finalScore;
            _currentUserData.MinutesNoBreak = minutesNoBreak;
            _currentUserData.HRV = hrv;
            _currentUserData.LastUpdate = DateTime.Now;

            return Task.FromResult(finalScore);
        }

        public Models.UserData GetCurrentUserData()
        {
            return _currentUserData;
        }

        public async Task SaveUserGoalAsync(string goal)
        {
            _currentUserData.SelectedGoal = goal;
            // Można zapisać do bazy danych jeśli potrzeba
        }

        public async Task ResetAllDataAsync()
        {
            _currentUserData = new Models.UserData
            {
                NeuroScore = 50,
                MinutesNoBreak = 0,
                HRV = 0,
                LastUpdate = DateTime.Now
            };
            _scoreHistory.Clear();
        }

        public async Task<List<Models.NeuroScoreHistory>> GetScoreHistoryAsync(int days = 7)
        {
            return _scoreHistory.Where(h => h.Timestamp >= DateTime.Now.AddDays(-days)).ToList();
        }

        public Models.NeuroScoreComponents GetLastScoreComponents()
        {
            return _lastComponents;
        }
    }
}