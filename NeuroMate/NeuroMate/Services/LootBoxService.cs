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
                        Name = "Podstawowa Skrzynka", 
                        Description = "Zawiera podstawowe nagrody", 
                        Price = 50, 
                        Rarity = "Common",
                        ImagePath = "lootbox_common.png"
                    },
                    new Database.Entities.LootBox 
                    { 
                        Name = "Srebrna Skrzynka", 
                        Description = "Lepsze szanse na rzadkie nagrody", 
                        Price = 100, 
                        Rarity = "Rare",
                        ImagePath = "lootbox_rare.png"
                    },
                    new Database.Entities.LootBox 
                    { 
                        Name = "Złota Skrzynka", 
                        Description = "Najlepsze nagrody w grze!", 
                        Price = 200, 
                        Rarity = "Epic",
                        ImagePath = "lootbox_epic.png"
                    }
                };

                foreach (var box in defaultBoxes)
                {
                    await _database.SaveLootBoxAsync(box);
                }

                await InitializeDefaultAvatarsAsync();
                await InitializeDefaultRewardsAsync();
            }
        }

        private async Task InitializeDefaultAvatarsAsync()
        {
            var player = await _pointsService.GetCurrentPlayerAsync();
            var existingAvatars = await _database.GetAllAvatarsAsync();
            
            if (!existingAvatars.Any())
            {
                var defaultAvatars = new List<Database.Entities.Avatar>
                {
                    new Database.Entities.Avatar { Name = "Robocik Podstawowy", Description = "Twój pierwszy awatar", ImagePath = "avatar_robot_basic.png", Rarity = "Common", IsUnlocked = true, IsSelected = true, PlayerId = player.Id },
                    new Database.Entities.Avatar { Name = "Koteczek", Description = "Słodki kotek", ImagePath = "avatar_cat.png", Rarity = "Common", PlayerId = player.Id },
                    new Database.Entities.Avatar { Name = "Mysz Laboratoryjna", Description = "Mądra mysz", ImagePath = "avatar_mouse.png", Rarity = "Rare", PlayerId = player.Id },
                    new Database.Entities.Avatar { Name = "Robocik Srebrny", Description = "Ulepszony robot", ImagePath = "avatar_robot_silver.png", Rarity = "Rare", PlayerId = player.Id },
                    new Database.Entities.Avatar { Name = "Smok Mały", Description = "Młody smok", ImagePath = "avatar_dragon.png", Rarity = "Epic", PlayerId = player.Id },
                    new Database.Entities.Avatar { Name = "Robocik Złoty", Description = "Najlepszy robot", ImagePath = "avatar_robot_gold.png", Rarity = "Epic", PlayerId = player.Id },
                    new Database.Entities.Avatar { Name = "Feniks", Description = "Legendarna istota", ImagePath = "avatar_phoenix.png", Rarity = "Legendary", PlayerId = player.Id }
                };

                foreach (var avatar in defaultAvatars)
                {
                    await _database.SaveAvatarAsync(avatar);
                }
            }
        }

        private async Task InitializeDefaultRewardsAsync()
        {
            var boxes = await _database.GetAllLootBoxesAsync();
            
            foreach (var box in boxes)
            {
                var rewards = new List<Database.Entities.LootBoxReward>();
                
                if (box.Rarity == "Common")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "25", DropChance = 0.4f, Rarity = "Common" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Avatar", RewardValue = "2", DropChance = 0.3f, Rarity = "Common" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Avatar", RewardValue = "3", DropChance = 0.2f, Rarity = "Rare" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "10", DropChance = 0.1f, Rarity = "Common" }
                    });
                }
                else if (box.Rarity == "Rare")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Avatar", RewardValue = "4", DropChance = 0.3f, Rarity = "Rare" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Avatar", RewardValue = "5", DropChance = 0.25f, Rarity = "Epic" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "75", DropChance = 0.25f, Rarity = "Rare" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "50", DropChance = 0.2f, Rarity = "Common" }
                    });
                }
                else if (box.Rarity == "Epic")
                {
                    rewards.AddRange(new[]
                    {
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Avatar", RewardValue = "6", DropChance = 0.4f, Rarity = "Epic" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Avatar", RewardValue = "7", DropChance = 0.2f, Rarity = "Legendary" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "150", DropChance = 0.3f, Rarity = "Epic" },
                        new Database.Entities.LootBoxReward { LootBoxId = box.Id, RewardType = "Points", RewardValue = "100", DropChance = 0.1f, Rarity = "Rare" }
                    });
                }

                foreach (var reward in rewards)
                {
                    await _database.SaveLootBoxRewardAsync(reward);
                }
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

            // Sprawdź czy gracz ma wystarczająco punktów
            var canAfford = await _pointsService.SpendPointsAsync(lootBox.Price);
            if (!canAfford)
                throw new InvalidOperationException("Niewystarczająco punktów");

            var player = await _pointsService.GetCurrentPlayerAsync();
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
    }
}
