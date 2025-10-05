using NeuroMate.Database;
using NeuroMate.Database.Entities;
using NeuroMate.Models;

namespace NeuroMate.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly DatabaseService _database;
        private readonly PointsService _pointsService;

        public AvatarService(DatabaseService database, PointsService pointsService)
        {
            _database = database;
            _pointsService = pointsService;
        }

        public async Task<List<Database.Entities.Avatar>> GetAvailableAvatarsAsync()
        {
            return await _database.GetAllAvatarsAsync();
        }

        public async Task<Database.Entities.Avatar?> GetAvatarByIdAsync(int avatarId)
        {
            var avatars = await _database.GetAllAvatarsAsync();
            return avatars.FirstOrDefault(a => a.Id == avatarId);
        }

        public async Task UnlockAvatarAsync(int avatarId)
        {
            var avatar = await GetAvatarByIdAsync(avatarId);
            if (avatar != null && !avatar.IsUnlocked)
            {
                avatar.IsUnlocked = true;
                avatar.UnlockedAt = DateTime.Now;
                await _database.SaveAvatarAsync(avatar);
            }
        }

        public async Task SelectAvatarAsync(int avatarId)
        {
            var player = await _pointsService.GetCurrentPlayerAsync();
            var avatar = await GetAvatarByIdAsync(avatarId);
            
            if (avatar != null && avatar.IsUnlocked)
            {
                // Odznacz poprzednio wybrany awatar
                var currentSelected = await GetSelectedAvatarAsync();
                if (currentSelected != null)
                {
                    currentSelected.IsSelected = false;
                    await _database.SaveAvatarAsync(currentSelected);
                }

                // Zaznacz nowy awatar
                avatar.IsSelected = true;
                await _database.SaveAvatarAsync(avatar);

                // Zaktualizuj profil gracza
                player.SelectedAvatarId = avatarId;
                await _database.SavePlayerProfileDataAsync(player);
            }
        }

        public async Task<Database.Entities.Avatar?> GetSelectedAvatarAsync()
        {
            var avatars = await _database.GetAllAvatarsAsync();
            return avatars.FirstOrDefault(a => a.IsSelected);
        }

        // Dodatkowe metody dla AvatarShopPage
        public async Task<List<Database.Entities.Avatar>> GetAllAvatarsAsync()
        {
            return await _database.GetAllAvatarsAsync();
        }

        public async Task<bool> ChangeAvatarAsync(string avatarId)
        {
            if (!int.TryParse(avatarId, out int id))
                return false;

            return await ChangeAvatarAsync(id);
        }

        public async Task<bool> ChangeAvatarAsync(int avatarId)
        {
            var avatar = await GetAvatarByIdAsync(avatarId);
            if (avatar == null || !avatar.IsUnlocked)
                return false;

            await SelectAvatarAsync(avatarId);
            return true;
        }

        public async Task<bool> PurchaseAvatarAsync(string avatarId)
        {
            if (!int.TryParse(avatarId, out int id))
                return false;

            var avatar = await GetAvatarByIdAsync(id);
            if (avatar == null || avatar.IsUnlocked)
                return false;

            var player = await _pointsService.GetCurrentPlayerAsync();
            if (player.Points < avatar.Price)
                return false;

            // Spend points
            var success = await _pointsService.SpendPointsAsync(avatar.Price);
            if (!success)
                return false;

            // Unlock avatar
            await UnlockAvatarAsync(id);
            
            // Auto-select purchased avatar
            await SelectAvatarAsync(id);

            return true;
        }

        // Nowa metoda do resetowania awatarów
        public async Task ResetAvatarsAsync()
        {
            // Usuń wszystkie istniejące awatary
            var existingAvatars = await _database.GetAllAvatarsAsync();
            foreach (var avatar in existingAvatars)
            {
                await _database.DeleteAvatarAsync(avatar);
            }

            // Zainicjalizuj nowe awatary
            await InitializeDefaultAvatarsAsync();
        }

        private async Task InitializeDefaultAvatarsAsync()
        {
            var player = await _pointsService.GetCurrentPlayerAsync();
            
            var defaultAvatars = new List<Database.Entities.Avatar>
            {
                new Database.Entities.Avatar 
                { 
                    Name = "HackyEah Domyślny", 
                    Description = "Twój podstawowy awatar", 
                    LottieFileName = "hackyeah_default.png",
                    PreviewImagePath = "hackyeah_default.png",
                    Price = 0,
                    Rarity = "Common", 
                    IsUnlocked = true, 
                    IsSelected = true, 
                    IsDefault = true,
                    PlayerId = player.Id 
                },
                new Database.Entities.Avatar 
                { 
                    Name = "HackyEah Szczęśliwy", 
                    Description = "Uśmiechnięty awatar", 
                    LottieFileName = "hackyeah_happy.png",
                    PreviewImagePath = "hackyeah_happy.png",
                    Price = 50,
                    Rarity = "Common", 
                    PlayerId = player.Id 
                },
                new Database.Entities.Avatar 
                { 
                    Name = "HackyEah Smutny", 
                    Description = "Zamyślony awatar", 
                    LottieFileName = "hackyeah_sad.png",
                    PreviewImagePath = "hackyeah_sad.png",
                    Price = 75,
                    Rarity = "Rare", 
                    PlayerId = player.Id 
                },
                new Database.Entities.Avatar 
                { 
                    Name = "HackyEah Machający", 
                    Description = "Przyjazny awatar", 
                    LottieFileName = "hackyeah_wave.png",
                    PreviewImagePath = "hackyeah_wave.png",
                    Price = 100,
                    Rarity = "Rare", 
                    PlayerId = player.Id 
                },
                new Database.Entities.Avatar 
                { 
                    Name = "Programista", 
                    Description = "Ekspert IT", 
                    LottieFileName = "it_guy.png",
                    PreviewImagePath = "it_guy.png",
                    Price = 200,
                    Rarity = "Epic", 
                    PlayerId = player.Id 
                },
                new Database.Entities.Avatar 
                { 
                    Name = "Szara Koszulka", 
                    Description = "Minimalistyczny styl", 
                    LottieFileName = "gray_tshirt.png",
                    PreviewImagePath = "gray_tshirt.png",
                    Price = 150,
                    Rarity = "Epic", 
                    PlayerId = player.Id 
                }
            };

            foreach (var avatar in defaultAvatars)
            {
                await _database.SaveAvatarAsync(avatar);
            }
        }
    }
}
