using NeuroMate.Models;

namespace NeuroMate.Services
{
    public class LootBoxService : ILootBoxService
    {
        private readonly IPointsService _pointsService;
        private readonly IAvatarService _avatarService;
        private readonly List<LootBox> _availableLootBoxes;
        private readonly Random _random = new();

        public LootBoxService(IPointsService pointsService, IAvatarService avatarService)
        {
            _pointsService = pointsService;
            _avatarService = avatarService;
            _availableLootBoxes = InitializeLootBoxes();
        }

        private List<LootBox> InitializeLootBoxes()
        {
            return new List<LootBox>
            {
                new LootBox
                {
                    Id = "bronze_box",
                    Name = "Brązowa Skrzynia",
                    Description = "Podstawowa skrzynia z szansą na zwykłe i rzadkie awatary",
                    Price = 200,
                    IconPath = "bronze_box.png",
                    PossibleRewards = new List<LootBoxReward>
                    {
                        new LootBoxReward { AvatarId = "robot_blue", DropChance = 0.4, Rarity = AvatarRarity.Common },
                        new LootBoxReward { AvatarId = "cat_smart", DropChance = 0.4, Rarity = AvatarRarity.Common },
                        new LootBoxReward { AvatarId = "owl_professor", DropChance = 0.15, Rarity = AvatarRarity.Rare },
                        new LootBoxReward { AvatarId = "fox_zen", DropChance = 0.05, Rarity = AvatarRarity.Rare }
                    }
                },
                
                new LootBox
                {
                    Id = "silver_box",
                    Name = "Srebrna Skrzynia",
                    Description = "Zaawansowana skrzynia z wyższą szansą na rzadkie awatary",
                    Price = 400,
                    IconPath = "silver_box.png",
                    PossibleRewards = new List<LootBoxReward>
                    {
                        new LootBoxReward { AvatarId = "owl_professor", DropChance = 0.3, Rarity = AvatarRarity.Rare },
                        new LootBoxReward { AvatarId = "fox_zen", DropChance = 0.3, Rarity = AvatarRarity.Rare },
                        new LootBoxReward { AvatarId = "dragon_wisdom", DropChance = 0.25, Rarity = AvatarRarity.Epic },
                        new LootBoxReward { AvatarId = "phoenix_energy", DropChance = 0.15, Rarity = AvatarRarity.Epic }
                    }
                },
                
                new LootBox
                {
                    Id = "gold_box",
                    Name = "Złota Skrzynia",
                    Description = "Ekskluzywna skrzynia z szansą na legendarne awatary!",
                    Price = 800,
                    IconPath = "gold_box.png",
                    PossibleRewards = new List<LootBoxReward>
                    {
                        new LootBoxReward { AvatarId = "dragon_wisdom", DropChance = 0.4, Rarity = AvatarRarity.Epic },
                        new LootBoxReward { AvatarId = "phoenix_energy", DropChance = 0.35, Rarity = AvatarRarity.Epic },
                        new LootBoxReward { AvatarId = "cosmic_brain", DropChance = 0.15, Rarity = AvatarRarity.Legendary },
                        new LootBoxReward { AvatarId = "time_master", DropChance = 0.1, Rarity = AvatarRarity.Legendary }
                    }
                }
            };
        }

        public async Task<List<LootBox>> GetAvailableLootBoxesAsync()
        {
            await Task.Delay(1);
            return _availableLootBoxes.ToList();
        }

        public async Task<bool> CanAffordLootBoxAsync(string lootBoxId)
        {
            var lootBox = _availableLootBoxes.FirstOrDefault(lb => lb.Id == lootBoxId);
            if (lootBox == null) return false;

            var profile = await _pointsService.GetPlayerProfileAsync();
            return profile.TotalPoints >= lootBox.Price;
        }

        public async Task<LootBoxResult> OpenLootBoxAsync(string lootBoxId)
        {
            var lootBox = _availableLootBoxes.FirstOrDefault(lb => lb.Id == lootBoxId);
            if (lootBox == null)
                throw new ArgumentException("Nieznany lootbox", nameof(lootBoxId));

            var profile = await _pointsService.GetPlayerProfileAsync();
            if (profile.TotalPoints < lootBox.Price)
                throw new InvalidOperationException("Niewystarczająca liczba punktów");

            // Odejmij punkty
            profile.TotalPoints -= lootBox.Price;
            profile.PointsSpent += lootBox.Price;
            profile.TotalLootBoxesOpened++;

            // Wygeneruj nagrodę
            var reward = await GenerateRewardAsync(lootBox);
            
            var result = new LootBoxResult
            {
                UnlockedAvatar = reward,
                OpenedAt = DateTime.Now
            };

            // Sprawdź czy awatar już był odblokowany
            if (profile.UnlockedAvatarIds.Contains(reward.Id))
            {
                result.WasNewAvatar = false;
                // Zwróć część punktów jako kompensację
                result.PointsRefunded = reward.Price / 2;
                profile.TotalPoints += result.PointsRefunded;
            }
            else
            {
                result.WasNewAvatar = true;
                profile.UnlockedAvatarIds.Add(reward.Id);
            }

            await _pointsService.SavePlayerProfileAsync(profile);
            return result;
        }

        public async Task<Avatar> GenerateRewardAsync(LootBox lootBox)
        {
            await Task.Delay(1);
            
            // Wybierz nagrodę na podstawie prawdopodobieństwa
            var totalChance = lootBox.PossibleRewards.Sum(r => r.DropChance);
            var randomValue = _random.NextDouble() * totalChance;
            
            double currentChance = 0;
            foreach (var reward in lootBox.PossibleRewards)
            {
                currentChance += reward.DropChance;
                if (randomValue <= currentChance)
                {
                    var allAvatars = await _avatarService.GetAllAvatarsAsync();
                    var selectedAvatar = allAvatars.FirstOrDefault(a => a.Id == reward.AvatarId);
                    return selectedAvatar ?? allAvatars.First();
                }
            }
            
            // Fallback - zwróć pierwszy dostępny awatar
            var avatars = await _avatarService.GetAllAvatarsAsync();
            return avatars.First();
        }

        public string GetLootBoxRarityDescription(string lootBoxId)
        {
            var lootBox = _availableLootBoxes.FirstOrDefault(lb => lb.Id == lootBoxId);
            if (lootBox == null) return "";

            var commonChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Common).Sum(r => r.DropChance);
            var rareChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Rare).Sum(r => r.DropChance);
            var epicChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Epic).Sum(r => r.DropChance);
            var legendaryChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Legendary).Sum(r => r.DropChance);

            var description = "Szanse na nagrody:\n";
            if (commonChance > 0) description += $"⚪ Zwykłe: {commonChance * 100:F0}%\n";
            if (rareChance > 0) description += $"🔵 Rzadkie: {rareChance * 100:F0}%\n";
            if (epicChance > 0) description += $"🟣 Epickie: {epicChance * 100:F0}%\n";
            if (legendaryChance > 0) description += $"🟠 Legendarne: {legendaryChance * 100:F0}%\n";

            return description.TrimEnd('\n');
        }

        public async Task<int> GetTotalLootBoxesOpenedAsync()
        {
            var profile = await _pointsService.GetPlayerProfileAsync();
            return profile.TotalLootBoxesOpened;
        }
    }
}
