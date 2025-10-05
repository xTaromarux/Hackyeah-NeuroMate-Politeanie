using NeuroMate.Models;
using NeuroMate.Services;
using NeuroMate.Database.Entities;
using NeuroMate.Database;
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
        private readonly DatabaseService _database; // Dodajemy dostęp do bazy danych

        public AvatarShopPage()
        {
            InitializeComponent();
            _pointsService = App.Services.GetService<IPointsService>();
            _avatarService = App.Services.GetService<IAvatarService>();
            _database = App.Services.GetService<DatabaseService>();
            AvatarsCollection.ItemsSource = _avatars;

            if (_pointsService != null)
                _pointsService.OnProfileChanged += () => _ = LoadDataAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Upewnij się że zaczynamy z filtrem "All"
            _currentFilter = "All";
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                // Załaduj awatary bezpośrednio z plików zamiast z bazy danych
                System.Diagnostics.Debug.WriteLine("Ładowanie awatarów bezpośrednio z plików Resources/Images...");
                
                var player = await _pointsService.GetCurrentPlayerAsync();
                
                // Definiuj awatary bezpośrednio na podstawie plików z Resources/Images
                var avatarsFromFiles = new List<Database.Entities.Avatar>
                {
                    new Database.Entities.Avatar 
                    { 
                        Id = 1,
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
                        Id = 2,
                        Name = "HackyEah Szczęśliwy", 
                        Description = "Uśmiechnięty awatar", 
                        LottieFileName = "hackyeah_happy.png",
                        PreviewImagePath = "hackyeah_happy.png",
                        Price = 50,
                        Rarity = "Common", 
                        IsUnlocked = false,
                        IsSelected = false,
                        PlayerId = player.Id 
                    },
                    new Database.Entities.Avatar 
                    { 
                        Id = 3,
                        Name = "HackyEah Smutny", 
                        Description = "Zamyślony awatar", 
                        LottieFileName = "hackyeah_sad.png",
                        PreviewImagePath = "hackyeah_sad.png",
                        Price = 75,
                        Rarity = "Rare", 
                        IsUnlocked = false,
                        IsSelected = false,
                        PlayerId = player.Id 
                    },
                    new Database.Entities.Avatar 
                    { 
                        Id = 4,
                        Name = "HackyEah Machający", 
                        Description = "Przyjazny awatar", 
                        LottieFileName = "hackyeah_wave.png",
                        PreviewImagePath = "hackyeah_wave.png",
                        Price = 100,
                        Rarity = "Rare", 
                        IsUnlocked = false,
                        IsSelected = false,
                        PlayerId = player.Id 
                    },
                    new Database.Entities.Avatar 
                    { 
                        Id = 5,
                        Name = "Programista", 
                        Description = "Ekspert IT", 
                        LottieFileName = "it_guy.png",
                        PreviewImagePath = "it_guy.png",
                        Price = 200,
                        Rarity = "Epic", 
                        IsUnlocked = false,
                        IsSelected = false,
                        PlayerId = player.Id 
                    },
                    new Database.Entities.Avatar 
                    { 
                        Id = 6,
                        Name = "Szara Koszulka", 
                        Description = "Minimalistyczny styl", 
                        LottieFileName = "gray_tshirt.png",
                        PreviewImagePath = "gray_tshirt.png",
                        Price = 150,
                        Rarity = "Epic", 
                        IsUnlocked = false,
                        IsSelected = false,
                        PlayerId = player.Id 
                    }
                };

                System.Diagnostics.Debug.WriteLine($"Załadowano {avatarsFromFiles.Count} awatarów z plików");
                
                // Sprawdź które awatary są odblokowane (pobierz z bazy danych informacje o odblokowaniu)
                var dbAvatars = await _avatarService.GetAllAvatarsAsync();
                foreach (var avatar in avatarsFromFiles)
                {
                    var dbAvatar = dbAvatars.FirstOrDefault(db => db.LottieFileName == avatar.LottieFileName);
                    if (dbAvatar != null)
                    {
                        avatar.IsUnlocked = dbAvatar.IsUnlocked;
                        avatar.IsSelected = dbAvatar.IsSelected;
                        System.Diagnostics.Debug.WriteLine($"Awatar {avatar.Name}: odblokowany={avatar.IsUnlocked}, wybrany={avatar.IsSelected}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Awatar {avatar.Name}: brak w bazie - pozostaje domyślny status");
                    }
                }

                // Załaduj punkty gracza
                var profile = await _pointsService.GetPlayerProfileAsync();
                PointsLabel.Text = $"💎 Punkty: {profile.TotalPoints}";

                // Użyj awatarów z plików zamiast z bazy
                _allAvatars = avatarsFromFiles;
                System.Diagnostics.Debug.WriteLine($"Ładuję {_allAvatars.Count} awatarów do interfejsu");
                
                await ApplyFilterAsync(_currentFilter);
                
                System.Diagnostics.Debug.WriteLine($"W kolekcji UI mamy teraz {_avatars.Count} awatarów");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania: {ex.Message}");
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

            // Debug: sprawdź ile awatarów mamy przed filtrowaniem
            System.Diagnostics.Debug.WriteLine($"Przed filtrowaniem: {_allAvatars.Count} awatarów, filtr: {filter}");

            // Filtruj awatary - używam porównania stringów ponieważ Avatar.Rarity to string
            var filteredAvatars = filter switch
            {
                "Common" => _allAvatars.Where(a => a.Rarity == "Common"),
                "Rare" => _allAvatars.Where(a => a.Rarity == "Rare"),
                "Epic" => _allAvatars.Where(a => a.Rarity == "Epic"),
                "Legendary" => _allAvatars.Where(a => a.Rarity == "Legendary"),
                _ => _allAvatars
            };

            var filteredList = filteredAvatars.ToList();
            System.Diagnostics.Debug.WriteLine($"Po filtrowaniu: {filteredList.Count} awatarów");
            foreach (var av in filteredList)
            {
                System.Diagnostics.Debug.WriteLine($"  Filtrowany awatar: {av.Name}, Rzadkość: {av.Rarity}");
            }

            _avatars.Clear();
            var profile = await _pointsService.GetPlayerProfileAsync();

            foreach (var avatar in filteredList)
            {
                _avatars.Add(new AvatarShopItem(avatar, profile));
            }
            
            System.Diagnostics.Debug.WriteLine($"Dodano do UI: {_avatars.Count} awatarów");
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
                System.Diagnostics.Debug.WriteLine($"Kliknięto przycisk awatara ID: {avatarId}");
                
                var avatarItem = _avatars.FirstOrDefault(a => a.Id.ToString() == avatarId);
                if (avatarItem == null) 
                {
                    System.Diagnostics.Debug.WriteLine($"Nie znaleziono awatara o ID: {avatarId}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Znaleziono awatar: {avatarItem.Name}, Odblokowany: {avatarItem.IsUnlocked}, Wybrany: {avatarItem.IsCurrentlySelected}");

                try
                {
                    if (avatarItem.IsUnlocked && !avatarItem.IsCurrentlySelected)
                    {
                        System.Diagnostics.Debug.WriteLine("Zmieniam awatara...");
                        // Zmień awatara - zapisz w bazie i odśwież UI
                        await SaveAvatarUnlockStatus(avatarItem, isSelected: true);
                        await DisplayAlert("Sukces!", $"Zmieniono awatara na: {avatarItem.Name}", "OK");
                        await LoadDataAsync(); // Odśwież dane w sklepie

                        // Wyślij komunikat o zmianie awatara do całej aplikacji
                        WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
                        System.Diagnostics.Debug.WriteLine("Wysłano komunikat o zmianie awatara");
                    }
                    else if (!avatarItem.IsUnlocked)
                    {
                        System.Diagnostics.Debug.WriteLine("Rozpoczynam proces kupowania awatara...");
                        
                        // Sprawdź czy ma dość punktów
                        var profile = await _pointsService.GetPlayerProfileAsync();
                        System.Diagnostics.Debug.WriteLine($"Gracz ma {profile.TotalPoints} punktów, awatar kosztuje {avatarItem.Price}");
                        
                        if (profile.TotalPoints < avatarItem.Price)
                        {
                            await DisplayAlert("Błąd", "Niewystarczająca liczba punktów!", "OK");
                            return;
                        }

                        // Kup awatara
                        var confirmed = await DisplayAlert("Potwierdzenie",
                            $"Czy chcesz kupić awatara '{avatarItem.Name}' za {avatarItem.Price} punktów?",
                            "Kup", "Anuluj");

                        if (confirmed)
                        {
                            System.Diagnostics.Debug.WriteLine("Gracz potwierdził zakup");
                            
                            // Odejmij punkty
                            var success = await _pointsService.SpendPointsAsync(avatarItem.Price);
                            System.Diagnostics.Debug.WriteLine($"Wynik odejmowania punktów: {success}");
                            
                            if (success)
                            {
                                // Zapisz awatara jako odblokowany i aktywny
                                await SaveAvatarUnlockStatus(avatarItem, isUnlocked: true, isSelected: true);
                                
                                await DisplayAlert("Sukces!", $"Kupiono awatara: {avatarItem.Name}!\nAwatar został automatycznie aktywowany.", "OK");
                                await LoadDataAsync(); // Odśwież dane w sklepie

                                // Wyślij komunikaty do całej aplikacji
                                WeakReferenceMessenger.Default.Send(new PointsChangedMessage(0));
                                WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
                                System.Diagnostics.Debug.WriteLine("Wysłano komunikaty o zakupie i zmianie awatara");
                            }
                            else
                            {
                                await DisplayAlert("Błąd", "Niewystarczająca liczba punktów!", "OK");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Gracz anulował zakup");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Awatar jest już aktywny lub nie można wykonać akcji");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd w OnAvatarActionClicked: {ex.Message}");
                    await DisplayAlert("Błąd", $"Wystąpił problem: {ex.Message}", "OK");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Nieprawidłowe parametry przyciska");
            }
        }

        // Nowa metoda do zapisywania statusu awatara w bazie
        private async Task SaveAvatarUnlockStatus(AvatarShopItem avatarItem, bool? isUnlocked = null, bool? isSelected = null)
        {
            try
            {
                // Znajdź lub utwórz awatara w bazie
                var dbAvatars = await _avatarService.GetAllAvatarsAsync();
                var dbAvatar = dbAvatars.FirstOrDefault(db => db.LottieFileName == avatarItem.LottieFileName);
                
                if (dbAvatar == null)
                {
                    // Utwórz nowy rekord w bazie
                    dbAvatar = new Database.Entities.Avatar
                    {
                        Name = avatarItem.Name,
                        Description = avatarItem.Description,
                        LottieFileName = avatarItem.LottieFileName,
                        PreviewImagePath = avatarItem.PreviewImagePath,
                        Price = avatarItem.Price,
                        Rarity = avatarItem.Rarity,
                        PlayerId = avatarItem.PlayerId,
                        IsUnlocked = isUnlocked ?? avatarItem.IsUnlocked,
                        IsSelected = isSelected ?? avatarItem.IsSelected,
                        IsDefault = avatarItem.IsDefault
                    };
                }
                else
                {
                    // Zaktualizuj istniejący rekord
                    if (isUnlocked.HasValue) dbAvatar.IsUnlocked = isUnlocked.Value;
                    if (isSelected.HasValue) 
                    {
                        // Jeśli wybieramy nowy awatar, odznacz poprzedni
                        if (isSelected.Value)
                        {
                            var previousSelected = dbAvatars.FirstOrDefault(a => a.IsSelected);
                            if (previousSelected != null)
                            {
                                previousSelected.IsSelected = false;
                                await _database.SaveAvatarAsync(previousSelected);
                            }
                        }
                        dbAvatar.IsSelected = isSelected.Value;
                    }
                }

                await _database.SaveAvatarAsync(dbAvatar);
                System.Diagnostics.Debug.WriteLine($"Zapisano status awatara {avatarItem.Name}: odblokowany={dbAvatar.IsUnlocked}, wybrany={dbAvatar.IsSelected}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd zapisywania statusu awatara: {ex.Message}");
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
                    // Zmień awatara - użyj nowego systemu
                    await SaveAvatarUnlockStatus(_currentPreviewAvatar, isSelected: true);
                    await DisplayAlert("Sukces!", $"Zmieniono awatara na: {_currentPreviewAvatar.Name}", "OK");
                    await LoadDataAsync(); // Odśwież dane w sklepie
                    await CloseAvatarPreview();

                    // Wyślij komunikat o zmianie awatara do całej aplikacji
                    WeakReferenceMessenger.Default.Send(new AvatarChangedMessage());
                }
                else if (!_currentPreviewAvatar.IsUnlocked)
                {
                    // Sprawdź czy ma dość punktów
                    var profile = await _pointsService.GetPlayerProfileAsync();
                    if (profile.TotalPoints < _currentPreviewAvatar.Price)
                    {
                        await DisplayAlert("Błąd", "Niewystarczająca liczba punktów!", "OK");
                        return;
                    }

                    // Kup awatara
                    var confirmed = await DisplayAlert("Potwierdzenie zakupu",
                        $"Czy chcesz kupić awatara '{_currentPreviewAvatar.Name}' za {_currentPreviewAvatar.Price} punktów?\n\n" +
                        $"Rzadkość: {GetRarityText(_currentPreviewAvatar.Rarity)}\n" +
                        $"Opis: {_currentPreviewAvatar.Description}",
                        "Kup", "Anuluj");

                    if (confirmed)
                    {
                        // Odejmij punkty
                        var success = await _pointsService.SpendPointsAsync(_currentPreviewAvatar.Price);
                        if (success)
                        {
                            // Zapisz awatara jako odblokowany i aktywny
                            await SaveAvatarUnlockStatus(_currentPreviewAvatar, isUnlocked: true, isSelected: true);
                            
                            await DisplayAlert("Sukces!", $"Kupiono awatara: {_currentPreviewAvatar.Name}!\n\nAwatar został automatycznie aktywowany.", "OK");
                            await LoadDataAsync(); // Odśwież dane w sklepie
                            await CloseAvatarPreview();

                            // Wyślij komunikaty do całej aplikacji
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
                Rarity = avatar.Rarity;
                PreviewImagePath = avatar.PreviewImagePath;
                PlayerId = avatar.PlayerId;

                // Sprawdź czy to aktualnie wybrany awatar
                IsCurrentlySelected = avatar.IsSelected || (profile.CurrentAvatarId == avatar.Id.ToString());
                
                // Sprawdź czy gracz ma dość punktów
                CanAfford = profile.TotalPoints >= avatar.Price;
                
                // Debug informacje
                System.Diagnostics.Debug.WriteLine($"Awatar {Name}: ID={Id}, Odblokowany={IsUnlocked}, Wybrany={IsCurrentlySelected}, Cena={Price}, MożeKupić={CanAfford}, Punkty={profile.TotalPoints}");
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
                ? (IsCurrentlySelected ? "✓ Aktywny" : "🎨 Wybierz")
                : (CanAfford ? "💎 Kup" : "💎 Kup");

            public Color ActionButtonColor => IsUnlocked
                ? (IsCurrentlySelected ? Colors.Gray : Colors.Blue)
                : (CanAfford ? Colors.Green : Colors.Orange);

            // NAPRAWKA: Zawsze pozwalaj na akcję, sprawdzenie punktów jest w kodzie
            public bool CanPerformAction => !IsCurrentlySelected; // Tylko blokuj jeśli awatar jest już aktywny
        }
    }
}
