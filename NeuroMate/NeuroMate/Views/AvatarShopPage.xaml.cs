using NeuroMate.Models;
using NeuroMate.Services;
using NeuroMate.Database.Entities;
using NeuroMate.Messages;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace NeuroMate.Views
{
    public partial class AvatarShopPage : ContentPage
    {
        private readonly IPointsService _pointsService;
        private readonly IAvatarService _avatarService;
        private readonly ObservableCollection<AvatarShopItem> _avatars = new();
        private List<Avatar> _allAvatars = new();
        private string _currentFilter = "All";
        private AvatarShopItem? _currentPreviewAvatar;

        public AvatarShopPage()
        {
            InitializeComponent();
            _pointsService = App.Services.GetService<IPointsService>();
            _avatarService = App.Services.GetService<IAvatarService>();
            AvatarsCollection.ItemsSource = _avatars;

            if (_pointsService != null)
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

            // Filtruj awatary - używam porównania stringów ponieważ Avatar.Rarity to string
            var filteredAvatars = filter switch
            {
                "Common" => _allAvatars.Where(a => a.Rarity == "Common"),
                "Rare" => _allAvatars.Where(a => a.Rarity == "Rare"),
                "Epic" => _allAvatars.Where(a => a.Rarity == "Epic"),
                "Legendary" => _allAvatars.Where(a => a.Rarity == "Legendary"),
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
                var avatarItem = _avatars.FirstOrDefault(a => a.Id.ToString() == avatarId);
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
                            WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
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
                                WeakReferenceMessenger.Default.Send(new PointsChangedMessage(0));
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

        private async void OnAvatarImageTapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is AvatarShopItem avatarItem)
            {
                await ShowAvatarPreview(avatarItem);
            }
        }

        private async Task ShowAvatarPreview(AvatarShopItem avatarItem)
        {
            _currentPreviewAvatar = avatarItem;

            // Ustaw dane w oknie podglądu
            PreviewAvatarNameLabel.Text = avatarItem.Name;
            PreviewAvatarDescriptionLabel.Text = avatarItem.Description;
            PreviewRarityLabel.Text = avatarItem.RarityIcon;
            PreviewRarityTextLabel.Text = GetRarityText(avatarItem.Rarity);
            PreviewPriceLabel.Text = avatarItem.PriceText;

            // Ustaw przycisk akcji
            PreviewActionButton.Text = avatarItem.ActionButtonText;
            PreviewActionButton.BackgroundColor = avatarItem.ActionButtonColor;
            PreviewActionButton.IsEnabled = avatarItem.CanPerformAction;

            // Jeśli avatar jest już aktywny, zmień tekst przycisku
            if (avatarItem.IsCurrentlySelected)
            {
                PreviewActionButton.Text = "✓ Aktywny";
            }
            else if (avatarItem.IsUnlocked)
            {
                PreviewActionButton.Text = $"🎨 Wybierz {avatarItem.Name}";
            }
            else
            {
                PreviewActionButton.Text = $"💎 Kup za {avatarItem.Price} pkt";
            }

            // Spróbuj załadować animację, w przeciwnym razie użyj obrazka
            try
            {
                if (!string.IsNullOrEmpty(avatarItem.LottieFileName) && 
                    avatarItem.LottieFileName.EndsWith(".webm"))
                {
                    PreviewAvatarVideo.Source = avatarItem.LottieFileName;
                    PreviewAvatarVideo.IsVisible = true;
                    PreviewAvatarImage.IsVisible = false;
                }
                else
                {
                    PreviewAvatarImage.Source = avatarItem.LottieFileName;
                    PreviewAvatarImage.IsVisible = true;
                    PreviewAvatarVideo.IsVisible = false;
                }
            }
            catch
            {
                // Fallback do obrazka
                PreviewAvatarImage.Source = avatarItem.LottieFileName;
                PreviewAvatarImage.IsVisible = true;
                PreviewAvatarVideo.IsVisible = false;
            }

            // Animacja pokazania okna dialogowego
            AvatarPreviewOverlay.IsVisible = true;
            AvatarPreviewOverlay.Opacity = 0;
            AvatarPreviewCard.Scale = 0.8;

            await Task.WhenAll(
                AvatarPreviewOverlay.FadeTo(1, 250),
                AvatarPreviewCard.ScaleTo(1, 250, Easing.CubicOut)
            );
        }

        private string GetRarityText(string rarity)
        {
            return rarity switch
            {
                "Common" => "Zwykła",
                "Rare" => "Rzadka", 
                "Epic" => "Epicka",
                "Legendary" => "Legendarna",
                _ => "Zwykła"
            };
        }

        private async void OnPreviewActionClicked(object sender, EventArgs e)
        {
            if (_currentPreviewAvatar == null) return;

            try
            {
                if (_currentPreviewAvatar.IsUnlocked && !_currentPreviewAvatar.IsCurrentlySelected)
                {
                    // Zmień awatara
                    var success = await _avatarService.ChangeAvatarAsync(_currentPreviewAvatar.Id.ToString());
                    if (success)
                    {
                        await DisplayAlert("Sukces!", $"Zmieniono awatara na: {_currentPreviewAvatar.Name}", "OK");
                        await LoadDataAsync(); // Odśwież dane w sklepie
                        await CloseAvatarPreview();

                        // Wyślij komunikat o zmianie awatara do całej aplikacji
                        WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
                    }
                }
                else if (!_currentPreviewAvatar.IsUnlocked)
                {
                    // Kup awatara
                    var confirmed = await DisplayAlert("Potwierdzenie zakupu",
                        $"Czy chcesz kupić awatara '{_currentPreviewAvatar.Name}' za {_currentPreviewAvatar.Price} punktów?\n\n" +
                        $"Rzadkość: {GetRarityText(_currentPreviewAvatar.Rarity)}\n" +
                        $"Opis: {_currentPreviewAvatar.Description}",
                        "Kup", "Anuluj");

                    if (confirmed)
                    {
                        var success = await _avatarService.PurchaseAvatarAsync(_currentPreviewAvatar.Id.ToString());
                        if (success)
                        {
                            await DisplayAlert("Sukces!", $"Kupiono awatara: {_currentPreviewAvatar.Name}!\n\nAwatar został automatycznie aktywowany.", "OK");
                            await LoadDataAsync(); // Odśwież dane w sklepie
                            await CloseAvatarPreview();

                            // Wyślij komunikat o zakupie (zmiana punktów) do całej aplikacji
                            WeakReferenceMessenger.Default.Send(new PointsChangedMessage(0));
                            WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
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

        private async void OnPreviewOverlayTapped(object sender, EventArgs e)
        {
            await CloseAvatarPreview();
        }

        private async void OnClosePreviewClicked(object sender, EventArgs e)
        {
            await CloseAvatarPreview();
        }

        private async Task CloseAvatarPreview()
        {
            await Task.WhenAll(
                AvatarPreviewOverlay.FadeTo(0, 200),
                AvatarPreviewCard.ScaleTo(0.8, 200, Easing.CubicIn)
            );
            
            AvatarPreviewOverlay.IsVisible = false;
            _currentPreviewAvatar = null;

            // Zatrzymaj video jeśli jest odtwarzane
            try
            {
                PreviewAvatarVideo.Stop();
            }
            catch { }
        }

        // Helper class dla bindowania danych w CollectionView
        public class AvatarShopItem : Avatar
        {
            public AvatarShopItem(Avatar avatar, Models.PlayerProfile profile)
            {
                Id = avatar.Id;
                Name = avatar.Name;
                Description = avatar.Description;
                LottieFileName = avatar.LottieFileName;
                Price = avatar.Price;
                IsUnlocked = avatar.IsUnlocked;
                IsDefault = avatar.IsDefault;
                Rarity = avatar.Rarity; // Kopiuj string Rarity
                PreviewImagePath = avatar.PreviewImagePath;

                IsCurrentlySelected = profile.CurrentAvatarId == avatar.Id.ToString();
                CanAfford = profile.TotalPoints >= avatar.Price;
            }

            public bool IsCurrentlySelected { get; set; }
            public bool CanAfford { get; set; }

            public string RarityIcon => Rarity switch
            {
                "Common" => "⚪",
                "Rare" => "🔵", 
                "Epic" => "🟣",
                "Legendary" => "🟠",
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
}
