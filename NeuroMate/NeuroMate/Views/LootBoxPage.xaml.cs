using NeuroMate.Models;
using NeuroMate.Services;
using System.Collections.ObjectModel;
using SkiaSharp.Extended.UI.Controls;

namespace NeuroMate.Views
{
    public partial class LootBoxPage : ContentPage
    {
        private readonly ILootBoxService _lootBoxService;
        private readonly IPointsService _pointsService;
        private readonly ObservableCollection<LootBoxShopItem> _lootBoxes = new();

        public LootBoxPage(ILootBoxService lootBoxService, IPointsService pointsService)
        {
            InitializeComponent();
            _lootBoxService = lootBoxService;
            _pointsService = pointsService;
            LootBoxesCollection.ItemsSource = _lootBoxes;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                // Załaduj punkty gracza
                var profile = await _pointsService.GetPlayerProfileAsync();
                PointsLabel.Text = $"💎 Punkty: {profile.TotalPoints}";
                BoxesOpenedLabel.Text = $"Otwarto: {profile.TotalLootBoxesOpened}";

                // Załaduj dostępne lootboxy
                var lootBoxes = await _lootBoxService.GetAvailableLootBoxesAsync();
                _lootBoxes.Clear();

                foreach (var lootBox in lootBoxes)
                {
                    var canAfford = await _lootBoxService.CanAffordLootBoxAsync(lootBox.Id);
                    var dropRates = GetDropRatesText(lootBox);
                    
                    _lootBoxes.Add(new LootBoxShopItem(lootBox, canAfford, dropRates));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się załadować danych: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private string GetDropRatesText(LootBox lootBox)
        {
            var commonChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Common).Sum(r => r.DropChance);
            var rareChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Rare).Sum(r => r.DropChance);
            var epicChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Epic).Sum(r => r.DropChance);
            var legendaryChance = lootBox.PossibleRewards.Where(r => r.Rarity == AvatarRarity.Legendary).Sum(r => r.DropChance);

            var parts = new List<string>();
            if (commonChance > 0) parts.Add($"⚪ {commonChance * 100:F0}%");
            if (rareChance > 0) parts.Add($"🔵 {rareChance * 100:F0}%");
            if (epicChance > 0) parts.Add($"🟣 {epicChance * 100:F0}%");
            if (legendaryChance > 0) parts.Add($"🟠 {legendaryChance * 100:F0}%");

            return string.Join(" | ", parts);
        }

        private async void OnOpenLootBoxClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string lootBoxId)
            {
                var lootBoxItem = _lootBoxes.FirstOrDefault(lb => lb.Id == lootBoxId);
                if (lootBoxItem == null) return;

                // Potwierdzenie zakupu
                var confirmed = await DisplayAlert("Potwierdzenie",
                    $"Czy chcesz otworzyć '{lootBoxItem.Name}' za {lootBoxItem.Price} punktów?",
                    "Otwórz", "Anuluj");

                if (!confirmed) return;

                try
                {
                    // Rozpocznij animację otwierania
                    await StartOpeningAnimationAsync();

                    // Otwórz lootbox (z opóźnieniem dla efektu)
                    await Task.Delay(2000);
                    var result = await _lootBoxService.OpenLootBoxAsync(lootBoxId);

                    // Pokaż wynik
                    await ShowResultAsync(result);

                    // Odśwież dane
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    HideOverlays();
                    await DisplayAlert("Błąd", $"Nie udało się otworzyć skrzynki: {ex.Message}", "OK");
                }
            }
        }

        private async Task StartOpeningAnimationAsync()
        {
            // Ukryj wszystkie overlaye
            HideOverlays();

            // Pokaż overlay z animacją otwierania
            OpeningOverlay.IsVisible = true;
            OpeningLabel.Text = "Otwieranie skrzynki...";

            // Uruchom animację
            OpeningAnimation.IsAnimationEnabled = true;

            // Symulacja procesu otwierania z różnymi tekstami
            await Task.Delay(500);
            OpeningLabel.Text = "Losowanie nagrody...";
            
            await Task.Delay(800);
            OpeningLabel.Text = "Już prawie...";
            
            await Task.Delay(700);
        }

        private async Task ShowResultAsync(LootBoxResult result)
        {
            // Ukryj animację otwierania
            OpeningOverlay.IsVisible = false;

            // Przygotuj dane wyniku
            RewardAvatarLottie.Source = (SKLottieImageSource)SKLottieImageSource.FromFile(result.UnlockedAvatar.LottieFileName);
            RewardNameLabel.Text = result.UnlockedAvatar.Name;
            RewardDescriptionLabel.Text = result.UnlockedAvatar.Description;

            // Ustaw header i status w zależności od wyniku
            if (result.WasNewAvatar)
            {
                ResultHeaderLabel.Text = "🎉 Nowy Awatar!";
                RewardStatusLabel.Text = $"{GetRarityIcon(result.UnlockedAvatar.Rarity)} {result.UnlockedAvatar.Rarity}";
                RewardStatusLabel.TextColor = GetRarityColor(result.UnlockedAvatar.Rarity);
            }
            else
            {
                ResultHeaderLabel.Text = "📦 Duplikat";
                RewardStatusLabel.Text = $"Zwrócono: {result.PointsRefunded} punktów";
                RewardStatusLabel.TextColor = Colors.Orange;
            }

            // Pokaż overlay z wynikiem
            ResultOverlay.IsVisible = true;

            // Uruchom animację awatara
            RewardAvatarLottie.IsAnimationEnabled = true;

            // Dodaj efekt dźwiękowy lub wibracje (opcjonalnie)
            if (result.WasNewAvatar && result.UnlockedAvatar.Rarity >= AvatarRarity.Epic)
            {
                // Wibracja dla rzadkich awatarów
                try
                {
                    var duration = TimeSpan.FromMilliseconds(500);
                    Vibration.Default.Vibrate(duration);
                }
                catch { } // Ignoruj błędy wibracji
            }
        }

        private string GetRarityIcon(AvatarRarity rarity)
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

        private Color GetRarityColor(AvatarRarity rarity)
        {
            return rarity switch
            {
                AvatarRarity.Common => Colors.Gray,
                AvatarRarity.Rare => Colors.Blue,
                AvatarRarity.Epic => Colors.Purple,
                AvatarRarity.Legendary => Colors.Orange,
                _ => Colors.Gray
            };
        }

        private void HideOverlays()
        {
            OpeningOverlay.IsVisible = false;
            ResultOverlay.IsVisible = false;
            OpeningAnimation.IsAnimationEnabled = false;
            RewardAvatarLottie.IsAnimationEnabled = false;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private void OnContinueClicked(object sender, EventArgs e)
        {
            HideOverlays();
        }

        private async void OnGoToShopClicked(object sender, EventArgs e)
        {
            HideOverlays();
            await Shell.Current.GoToAsync("//AvatarShopPage");
        }
    }

    // Helper class dla bindowania danych w CollectionView
    public class LootBoxShopItem : LootBox
    {
        public LootBoxShopItem(LootBox lootBox, bool canAfford, string dropRatesText)
        {
            Id = lootBox.Id;
            Name = lootBox.Name;
            Description = lootBox.Description;
            Price = lootBox.Price;
            PossibleRewards = lootBox.PossibleRewards;
            IconPath = lootBox.IconPath;

            CanAfford = canAfford;
            DropRatesText = dropRatesText;
        }

        public bool CanAfford { get; set; }
        public string DropRatesText { get; set; } = string.Empty;
        public string PriceText => $"{Price} pkt";
        public string ButtonStyle => CanAfford ? "PrimaryButton" : "DisabledButton";
    }
}
