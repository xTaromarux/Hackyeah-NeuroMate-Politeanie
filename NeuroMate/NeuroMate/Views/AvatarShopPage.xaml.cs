using NeuroMate.Models;
using NeuroMate.Services;
using System.Collections.ObjectModel;

namespace NeuroMate.Views
{
    public partial class AvatarShopPage : ContentPage
    {
        private readonly IPointsService _pointsService;
        private readonly IAvatarService _avatarService;
        private readonly ObservableCollection<AvatarShopItem> _avatars = new();
        private List<Avatar> _allAvatars = new();
        private string _currentFilter = "All";

        public AvatarShopPage(IPointsService pointsService, IAvatarService avatarService)
        {
            InitializeComponent();
            _pointsService = pointsService;
            _avatarService = avatarService;
            AvatarsCollection.ItemsSource = _avatars;

            _pointsService.OnProfileChanged += () => _ = LoadDataAsync();
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

                // Załaduj awatary
                _allAvatars = await _avatarService.GetAllAvatarsAsync();
                await ApplyFilterAsync(_currentFilter);
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

        private async Task ApplyFilterAsync(string filter)
        {
            _currentFilter = filter;

            // Reset przycisków filtrów
            ResetFilterButtons();

            // Podświetl aktywny filtr
            switch (filter)
            {
                case "All":
                    AllFilterButton.BackgroundColor = Colors.Blue;
                    break;
                case "Common":
                    CommonFilterButton.BackgroundColor = Colors.Gray;
                    break;
                case "Rare":
                    RareFilterButton.BackgroundColor = Colors.Blue;
                    break;
                case "Epic":
                    EpicFilterButton.BackgroundColor = Colors.Purple;
                    break;
                case "Legendary":
                    LegendaryFilterButton.BackgroundColor = Colors.Orange;
                    break;
            }

            // Filtruj awatary
            var filteredAvatars = filter switch
            {
                "Common" => _allAvatars.Where(a => a.Rarity == AvatarRarity.Common),
                "Rare" => _allAvatars.Where(a => a.Rarity == AvatarRarity.Rare),
                "Epic" => _allAvatars.Where(a => a.Rarity == AvatarRarity.Epic),
                "Legendary" => _allAvatars.Where(a => a.Rarity == AvatarRarity.Legendary),
                _ => _allAvatars
            };

            _avatars.Clear();
            var profile = await _pointsService.GetPlayerProfileAsync();

            foreach (var avatar in filteredAvatars)
            {
                _avatars.Add(new AvatarShopItem(avatar, profile));
            }
        }

        private void ResetFilterButtons()
        {
            AllFilterButton.BackgroundColor = Colors.Transparent;
            CommonFilterButton.BackgroundColor = Colors.Transparent;
            RareFilterButton.BackgroundColor = Colors.Transparent;
            EpicFilterButton.BackgroundColor = Colors.Transparent;
            LegendaryFilterButton.BackgroundColor = Colors.Transparent;
        }

        private async void OnFilterClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string filter)
            {
                await ApplyFilterAsync(filter);
            }
        }

        private async void OnAvatarActionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string avatarId)
            {
                var avatarItem = _avatars.FirstOrDefault(a => a.Id == avatarId);
                if (avatarItem == null) return;

                try
                {
                    if (avatarItem.IsUnlocked && !avatarItem.IsCurrentlySelected)
                    {
                        // Zmień awatara
                        var success = await _avatarService.ChangeAvatarAsync(avatarId);
                        if (success)
                        {
                            await DisplayAlert("Sukces!", $"Zmieniono awatara na: {avatarItem.Name}", "OK");
                            await LoadDataAsync(); // Odśwież dane w sklepie

                            // Wyślij komunikat o zmianie awatara do całej aplikacji
                            MessagingCenter.Send<AvatarShopPage>(this, "AvatarChanged");
                        }
                    }
                    else if (!avatarItem.IsUnlocked)
                    {
                        // Kup awatara
                        var confirmed = await DisplayAlert("Potwierdzenie",
                            $"Czy chcesz kupić awatara '{avatarItem.Name}' za {avatarItem.Price} punktów?",
                            "Kup", "Anuluj");

                        if (confirmed)
                        {
                            var success = await _avatarService.PurchaseAvatarAsync(avatarId);
                            if (success)
                            {
                                await DisplayAlert("Sukces!", $"Kupiono awatara: {avatarItem.Name}!", "OK");
                                await LoadDataAsync(); // Odśwież dane w sklepie

                                // Wyślij komunikat o zakupie (zmiana punktów) do całej aplikacji
                                MessagingCenter.Send<AvatarShopPage>(this, "PointsChanged");
                            }
                            else
                            {
                                await DisplayAlert("Błąd", "Niewystarczająca liczba punktów!", "OK");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Błąd", $"Wystąpił problem: {ex.Message}", "OK");
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnLootBoxClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("LootBoxPage");
        }
    }

    // Helper class dla bindowania danych w CollectionView
    public class AvatarShopItem : Avatar
    {
        public AvatarShopItem(Avatar avatar, PlayerProfile profile)
        {
            Id = avatar.Id;
            Name = avatar.Name;
            Description = avatar.Description;
            LottieFileName = avatar.LottieFileName;
            Price = avatar.Price;
            IsUnlocked = avatar.IsUnlocked;
            IsDefault = avatar.IsDefault;
            Rarity = avatar.Rarity;
            PreviewImagePath = avatar.PreviewImagePath;

            IsCurrentlySelected = profile.CurrentAvatarId == avatar.Id;
            CanAfford = profile.TotalPoints >= avatar.Price;
        }

        public bool IsCurrentlySelected { get; set; }
        public bool CanAfford { get; set; }

        public string RarityIcon => Rarity switch
        {
            AvatarRarity.Common => "⚪",
            AvatarRarity.Rare => "🔵",
            AvatarRarity.Epic => "🟣",
            AvatarRarity.Legendary => "🟠",
            _ => "⚪"
        };

        public string PriceText => IsUnlocked ? (IsCurrentlySelected ? "Aktywny" : "Odblokowany") : $"{Price} pkt";

        public string ActionButtonText => IsUnlocked
            ? (IsCurrentlySelected ? "Aktywny" : "Wybierz")
            : "Kup";

        public Color ActionButtonColor => IsUnlocked
            ? (IsCurrentlySelected ? Colors.Gray : Colors.Blue)
            : (CanAfford ? Colors.Green : Colors.Gray);

        public bool CanPerformAction => IsUnlocked ? !IsCurrentlySelected : CanAfford;
    }
}
