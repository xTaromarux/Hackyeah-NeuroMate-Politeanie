using NeuroMate.Database;
using NeuroMate.Database.Entities;
using NeuroMate.Models;
using CommunityToolkit.Mvvm.Messaging;
using NeuroMate.Messages;

namespace NeuroMate.Services
{
    public class LootBoxService : ILootBoxService
    {
        private readonly DatabaseService _database;
        private readonly PointsService _pointsService;
        private readonly Random _random = new Random();

        public LootBoxService(DatabaseService database, PointsService pointsService)
        {
            _database = database;
            _pointsService = pointsService;
        }

        public async Task InitializeDefaultLootBoxesAsync()
        {
            var existingBoxes = await _database.GetAllLootBoxesAsync();
            if (!existingBoxes.Any())
            {
                var defaultBoxes = new List<Database.Entities.LootBox>
                {
                    new Database.Entities.LootBox 
                    { 
                        Name = "Początkujący Pakiet", 
                        Description = "Idealna dla nowych graczy - gwarantowane punkty!", 
                        Price = 10, // Bardzo tania opcja
                        Rarity = "Common",
                        ImagePath = "it_guy.png" // Użyj dostępnego obrazka
                    },
                    new Database.Entities.LootBox 
                    { 
                        Name = "Podstawowa Skrzynka", 
                        Description = "Zawiera podstawowe nagrody i małe ilości punktów", 
                        Price = 25, // Zmniejszona cena
                        Rarity = "Common",
                        ImagePath = "hackyeah_default.png" // Użyj dostępnego obrazka
                    },
                    new Database.Entities.LootBox 
                    { 
                        Name = "Srebrna Skrzynka", 
                        Description = "Lepsze szanse na rzadkie nagrody i więcej punktów", 
                        Price = 75, // Zmniejszona cena
                        Rarity = "Rare",
                        ImagePath = "hackyeah_happy.png" // Użyj dostępnego obrazka
                    },
                    new Database.Entities.LootBox 
                    { 
                        Name = "Złota Skrzynka", 
                        Description = "Najlepsze nagrody w grze i duże ilości punktów!", 
                        Price = 150, // Zmniejszona cena
                        Rarity = "Epic",
                        ImagePath = "hackyeah_wave.png" // Użyj dostępnego obrazka
                    }
                };

                foreach (var box in defaultBoxes)
                {
                    await _database.SaveLootBoxAsync(box);
                }

                await InitializeDefaultRewardsAsync();
            }
        }

        private async Task InitializeDefaultRewardsAsync()
        {
            var boxes = await _database.GetAllLootBoxesAsync();
            
            foreach (var box in boxes)
            {
                // Sprawdź czy już są nagrody dla tej skrzynki
                var existingRewards = await _database.GetAllLootBoxRewardsAsync();
                if (existingRewards.Any(r => r.LootBoxId == box.Id))
                    continue; // Pomiń jeśli już są nagrody
                
                var rewards = new List<Database.Entities.LootBoxReward>();
                
                if (box.Name == "Początkujący Pakiet")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "15", DropChance = 0.6f, Rarity = "Common", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "20", DropChance = 0.3f, Rarity = "Common", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "30", DropChance = 0.1f, Rarity = "Rare", IsActive = true }
                    });
                }
                else if (box.Rarity == "Common")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "30", DropChance = 0.5f, Rarity = "Common", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "50", DropChance = 0.3f, Rarity = "Common", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "75", DropChance = 0.2f, Rarity = "Rare", IsActive = true }
                    });
                }
                else if (box.Rarity == "Rare")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "100", DropChance = 0.4f, Rarity = "Rare", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "125", DropChance = 0.3f, Rarity = "Rare", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "75", DropChance = 0.3f, Rarity = "Common", IsActive = true }
                    });
                }
                else if (box.Rarity == "Epic")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "200", DropChance = 0.4f, Rarity = "Epic", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "300", DropChance = 0.3f, Rarity = "Epic", IsActive = true },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "150", DropChance = 0.3f, Rarity = "Rare", IsActive = true }
                    });
                }

                foreach (var reward in rewards)
                {
                    await _database.SaveLootBoxRewardAsync(reward);
                }
                
                System.Diagnostics.Debug.WriteLine($"[LootBox] Dodano {rewards.Count} nagród dla {box.Name}");
            }
        }

        public async Task<List<Database.Entities.LootBox>> GetAvailableLootBoxesAsync()
        {
            return await _database.GetAllLootBoxesAsync();
        }

        public async Task<Database.Entities.Avatar?> GetAvatarByIdAsync(int avatarId)
        {
            var avatars = await _database.GetAllAvatarsAsync();
            return avatars.FirstOrDefault(a => a.Id == avatarId);
        }

        public async Task<Database.Entities.LootBoxResult> OpenLootBoxAsync(int lootBoxId)
        {
            var lootBox = (await _database.GetAllLootBoxesAsync()).FirstOrDefault(lb => lb.Id == lootBoxId);
            if (lootBox == null)
                throw new ArgumentException("LootBox nie istnieje");

            // NAPRAWKA: Najpierw sprawdź punkty bez ich wydawania
            var player = await _pointsService.GetCurrentPlayerAsync();
            var playerProfile = await _pointsService.GetPlayerProfileAsync();
            
            System.Diagnostics.Debug.WriteLine($"[LootBox] Gracz ma {playerProfile.TotalPoints} punktów, lootbox kosztuje {lootBox.Price}");
            
            if (playerProfile.TotalPoints < lootBox.Price)
            {
                System.Diagnostics.Debug.WriteLine($"[LootBox] Niewystarczająco punktów!");
                throw new InvalidOperationException("Niewystarczająco punktów");
            }

            // Teraz wydaj punkty (wiemy że ma wystarczająco)
            var success = await _pointsService.SpendPointsAsync(lootBox.Price);
            if (!success)
            {
                System.Diagnostics.Debug.WriteLine($"[LootBox] Błąd podczas wydawania punktów");
                throw new InvalidOperationException("Błąd podczas wydawania punktów");
            }

            System.Diagnostics.Debug.WriteLine($"[LootBox] Pomyślnie wydano {lootBox.Price} punktów");

            var rewards = await GetLootBoxRewardsAsync(lootBoxId);
            
            // Losuj nagrodę
            var selectedReward = SelectRandomReward(rewards);
            var result = new Database.Entities.LootBoxResult
            {
                LootBoxId = lootBoxId,
                PlayerId = player.Id,
                RewardType = selectedReward.RewardType,
                RewardValue = selectedReward.RewardValue,
                IsWin = true,
                PointsSpent = lootBox.Price
            };

            // Przetwórz nagrodę
            if (selectedReward.RewardType == "Avatar")
            {
                var avatarId = int.Parse(selectedReward.RewardValue);
                var avatar = await GetAvatarByIdAsync(avatarId);
                
                if (avatar != null)
                {
                    if (avatar.IsUnlocked)
                    {
                        // Jeśli awatar już odblokowany, daj punkty zwrotne
                        result.RewardType = "Points";
                        result.RewardValue = "50";
                        result.PointsGained = 50;
                        await _pointsService.AddPointsAsync(50);
                    }
                    else
                    {
                        // Odblokuj awatar
                        avatar.IsUnlocked = true;
                        avatar.UnlockedAt = DateTime.Now;
                        await _database.SaveAvatarAsync(avatar);
                        WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
                    }
                }
            }
            else if (selectedReward.RewardType == "Points")
            {
                var pointsAmount = int.Parse(selectedReward.RewardValue);
                result.PointsGained = pointsAmount;
                await _pointsService.AddPointsAsync(pointsAmount);
            }

            await _database.SaveLootBoxResultAsync(result);
            return result;
        }

        private async Task<List<Database.Entities.LootBoxReward>> GetLootBoxRewardsAsync(int lootBoxId)
        {
            var allRewards = await _database.GetAllLootBoxRewardsAsync();
            return allRewards.Where(r => r.LootBoxId == lootBoxId && r.IsActive).ToList();
        }

        private Database.Entities.LootBoxReward SelectRandomReward(List<Database.Entities.LootBoxReward> rewards)
        {
            var totalChance = rewards.Sum(r => r.DropChance);
            var randomValue = _random.NextSingle() * totalChance;
            
            float currentChance = 0;
            foreach (var reward in rewards)
            {
                currentChance += reward.DropChance;
                if (randomValue <= currentChance)
                {
                    return reward;
                }
            }
            
            return rewards.Last(); // Fallback
        }

        // Metoda do resetowania danych loot boxów na potrzeby debugowania
        public async Task ResetLootBoxDataAsync()
        {
            await _database.ResetLootBoxDataAsync();
            await InitializeDefaultLootBoxesAsync();
        }
    }
}
