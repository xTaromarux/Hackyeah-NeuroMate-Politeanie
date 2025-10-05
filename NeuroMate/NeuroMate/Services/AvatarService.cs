using NeuroMate.Models;

namespace NeuroMate.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly IPointsService _pointsService;
        private readonly List<Avatar> _allAvatars;

        public AvatarService(IPointsService pointsService)
        {
            _pointsService = pointsService;
            _allAvatars = InitializeAvatars();
            _pointsService.OnProfileChanged += _pointsService_OnProfileChanged;
        }

        private void _pointsService_OnProfileChanged()
        {
            // Asynchronicznie odświeżamy status awatarów
            _ = Task.Run(async () =>
            {
                try
                {
                    var profile = await _pointsService.GetPlayerProfileAsync();

                    // Aktualizujemy status odblokowania dla wszystkich awatarów
                    foreach (var avatar in _allAvatars)
                    {
                        avatar.IsUnlocked = profile.UnlockedAvatarIds.Contains(avatar.Id);
                    }

                    // Wywołujemy na głównym wątku, jeśli ktoś nasłuchuje
                    if (OnAvatarsUpdated != null)
                    {
                        Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
                        {
                            OnAvatarsUpdated.Invoke();
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating avatars: {ex.Message}");
                }
            });
        }

        private List<Avatar> InitializeAvatars()
        {
            return new List<Avatar>
            {
                // Domyślny awatar
                new Avatar
                {
                    Id = "hackyeah_default",
                    Name = "Hackyeah Mistrz",
                    Description = "Podstawowy awatar Hackyeah - twój przewodnik w biohackingu",
                    LottieFileName = "hackyeah_default.png",
                    Price = 0,
                    IsUnlocked = true,
                    IsDefault = true,
                    Rarity = AvatarRarity.Common,
                    PreviewImagePath = "hackyeah_default.png"
                },

                // Common Avatars (100-200 pkt)
                new Avatar
                {
                    Id = "it_guy",
                    Name = "IT Specjalista",
                    Description = "Techniczny ekspert gotowy na wyzwania programistyczne",
                    LottieFileName = "it_guy.png",
                    Price = 150,
                    IsUnlocked = false,
                    IsDefault = false,
                    Rarity = AvatarRarity.Common,
                    PreviewImagePath = "it_guy.png"
                },
                new Avatar
                {
                    Id = "hackyeah_happy",
                    Name = "Hackyeah Szczęśliwy",
                    Description = "Pozytywny awatar pełen energii i entuzjazmu",
                    LottieFileName = "hackyeah_happy.png",
                    Price = 120,
                    IsUnlocked = false,
                    IsDefault = false,
                    Rarity = AvatarRarity.Common,
                    PreviewImagePath = "hackyeah_happy.png"
                },

                // Rare Avatars (300-500 pkt)
                new Avatar
                {
                    Id = "hackyeah_sad",
                    Name = "Hackyeah Zamyślony",
                    Description = "Refleksyjny awatar do momentów głębokiego myślenia",
                    LottieFileName = "hackyeah_sad.png",
                    Price = 400,
                    IsUnlocked = false,
                    IsDefault = false,
                    Rarity = AvatarRarity.Rare,
                    PreviewImagePath = "hackyeah_sad.png"
                },
                new Avatar
                {
                    Id = "gray_tshirt",
                    Name = "Casual Style",
                    Description = "Zwykły awatar w szarej koszulce dla codziennych zadań",
                    LottieFileName = "gray_tshirt.png",
                    Price = 350,
                    IsUnlocked = false,
                    IsDefault = false,
                    Rarity = AvatarRarity.Rare,
                    PreviewImagePath = "gray_tshirt.png"
                },

                // Epic Avatar (600-800 pkt)
                new Avatar
                {
                    Id = "hackyeah_wave",
                    Name = "Hackyeah Pozdrowienia",
                    Description = "Przyjazny awatar machający na powitanie",
                    LottieFileName = "hackyeah_wave.png",
                    Price = 700,
                    IsUnlocked = false,
                    IsDefault = false,
                    Rarity = AvatarRarity.Epic,
                    PreviewImagePath = "hackyeah_wave.png"
                }
            };
        }

        public async Task<List<Avatar>> GetAllAvatarsAsync()
        {
            await Task.Delay(1);
            var profile = await _pointsService.GetPlayerProfileAsync();

            // Ustaw status odblokowania
            foreach (var avatar in _allAvatars)
            {
                avatar.IsUnlocked = profile.UnlockedAvatarIds.Contains(avatar.Id);
            }

            return _allAvatars.ToList();
        }

        public async Task<List<Avatar>> GetUnlockedAvatarsAsync()
        {
            var allAvatars = await GetAllAvatarsAsync();
            return allAvatars.Where(a => a.IsUnlocked).ToList();
        }

        public async Task<bool> PurchaseAvatarAsync(string avatarId)
        {
            var avatar = _allAvatars.FirstOrDefault(a => a.Id == avatarId);
            if (avatar == null || avatar.IsUnlocked)
                return false;

            var profile = await _pointsService.GetPlayerProfileAsync();

            if (profile.TotalPoints >= avatar.Price)
            {
                // Odejmij punkty
                profile.TotalPoints -= avatar.Price;
                profile.PointsSpent += avatar.Price;

                // Odblokuj awatara
                profile.UnlockedAvatarIds.Add(avatarId);

                // Zapisz profil
                await _pointsService.SavePlayerProfileAsync(profile);

                return true;
            }

            return false;
        }

        public async Task<bool> ChangeAvatarAsync(string avatarId)
        {
            var profile = await _pointsService.GetPlayerProfileAsync();

            if (profile.UnlockedAvatarIds.Contains(avatarId))
            {
                profile.CurrentAvatarId = avatarId;
                await _pointsService.SavePlayerProfileAsync(profile);
                return true;
            }

            return false;
        }

        public async Task<Avatar> GetCurrentAvatarAsync()
        {
            var profile = await _pointsService.GetPlayerProfileAsync();
            var currentAvatar = _allAvatars.FirstOrDefault(a => a.Id == profile.CurrentAvatarId);
            return currentAvatar ?? _allAvatars.First(a => a.IsDefault);
        }

        public async Task<bool> IsAvatarUnlockedAsync(string avatarId)
        {
            var profile = await _pointsService.GetPlayerProfileAsync();
            return profile.UnlockedAvatarIds.Contains(avatarId);
        }

        public string GetRarityColor(AvatarRarity rarity)
        {
            return rarity switch
            {
                AvatarRarity.Common => "#8E8E93",     // Szary
                AvatarRarity.Rare => "#007AFF",       // Niebieski
                AvatarRarity.Epic => "#AF52DE",       // Fioletowy
                AvatarRarity.Legendary => "#FF9500",  // Pomarańczowy
                _ => "#8E8E93"
            };
        }

        public string GetRarityIcon(AvatarRarity rarity)
        {
            return rarity switch
            {
                AvatarRarity.Common => "⚪",
                AvatarRarity.Rare => "🔵",
                AvatarRarity.Epic => "🟣",
                AvatarRarity.Legendary => "🟠",
                _ => "⚪"
            };
        }

        // Dodajemy event dla nasłuchiwania zmian w awatarach
        public event Action? OnAvatarsUpdated;
    }
}
