using NeuroMate.Database;
using NeuroMate.Database.Entities;
using NeuroMate.Models;
using CommunityToolkit.Mvvm.Messaging;
using NeuroMate.Messages;

namespace NeuroMate.Services
{
    public class PointsService : IPointsService
    {
        private readonly DatabaseService _database;
        private PlayerProfileData? _currentPlayer;

        public event Action? OnProfileChanged;

        public PointsService(DatabaseService database)
        {
            _database = database;
        }

        public async Task<PlayerProfileData> GetCurrentPlayerAsync()
        {
            if (_currentPlayer == null)
            {
                var players = await _database.GetAllPlayerProfileDataAsync();
                _currentPlayer = players.FirstOrDefault();
                
                if (_currentPlayer == null)
                {
                    _currentPlayer = new PlayerProfileData
                    {
                        Username = "Gracz",
                        Points = 500, // Startowe punkty zwiększone na potrzeby testowania
                        Level = 1,
                        Experience = 0
                    };
                    await _database.SavePlayerProfileDataAsync(_currentPlayer);
                }
            }
            return _currentPlayer;
        }

        public async Task<bool> SpendPointsAsync(int amount)
        {
            var player = await GetCurrentPlayerAsync();
            if (player.Points >= amount)
            {
                player.Points -= amount;
                await _database.SavePlayerProfileDataAsync(player);
                WeakReferenceMessenger.Default.Send(new PointsChangedMessage(player.Points));
                return true;
            }
            return false;
        }

        public async Task AddPointsAsync(int amount)
        {
            var player = await GetCurrentPlayerAsync();
            player.Points += amount;
            await _database.SavePlayerProfileDataAsync(player);
            WeakReferenceMessenger.Default.Send(new PointsChangedMessage(player.Points));
        }

        public async Task<int> GetCurrentPointsAsync()
        {
            var player = await GetCurrentPlayerAsync();
            return player.Points;
        }

        // Dodatkowe metody dla AvatarShopPage
        public async Task<Models.PlayerProfile> GetPlayerProfileAsync()
        {
            var playerData = await GetCurrentPlayerAsync();
            var avatars = await _database.GetAllAvatarsAsync();
            var unlockedAvatarIds = avatars.Where(a => a.IsUnlocked && a.PlayerId == playerData.Id)
                                          .Select(a => a.Id.ToString()).ToList();

            return new Models.PlayerProfile
            {
                TotalPoints = playerData.Points+2000,
                PointsSpent = 0, // Można dodać tracking wydanych punktów
                CurrentAvatarId = playerData.SelectedAvatarId.ToString(),
                UnlockedAvatarIds = unlockedAvatarIds,
                TotalGamesPlayed = 0, // Można dodać tracking gier
                TotalLootBoxesOpened = 0, // Można dodać tracking lootboxów
                LastPointsEarned = DateTime.Now
            };
        }

        private void NotifyProfileChanged()
        {
            OnProfileChanged?.Invoke();
        }

        // Dodatkowe metody dla MainViewModel
        public async Task<List<PointsHistory>> GetPointsHistoryAsync(int days)
        {
            // Zwracam pustą listę jako placeholder - można rozbudować o prawdziwą historię
            return new List<PointsHistory>();
        }

        public async Task<int> AddPointsForGameAsync(string gameType, int gameScore, int avgReactionMs)
        {
            // Oblicz punkty na podstawie wyniku gry
            int pointsEarned = Math.Max(5, gameScore / 10); // 5-10 punktów za grę
            
            await AddPointsAsync(pointsEarned);
            NotifyProfileChanged();
            
            return pointsEarned;
        }
    }
}
