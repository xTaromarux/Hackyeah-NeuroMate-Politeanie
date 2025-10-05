using Microsoft.Maui.Controls;
using NeuroMate.Services;
using NeuroMate.Database.Entities;
using NeuroMate.Database;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NeuroMate.Messages;

namespace NeuroMate.Views
{
    public partial class LootBoxPage : ContentPage
    {
        private readonly LootBoxViewModel _viewModel;

        public LootBoxPage(LootBoxService lootBoxService, PointsService pointsService)
        {
            InitializeComponent();
            _viewModel = new LootBoxViewModel(lootBoxService, pointsService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
            
            // Zaktualizuj punkty w headerze
            PointsLabel.Text = $"💎 Punkty: {_viewModel.CurrentPoints}";
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                
                // Resetuj loot boxy
                await _viewModel.LootBoxService.ResetLootBoxDataAsync();
                
                // Przeładuj dane
                await _viewModel.LoadDataAsync();
                PointsLabel.Text = $"💎 Punkty: {_viewModel.CurrentPoints}";
                
                await DisplayAlert("Sukces", "Loot boxy zostały zresetowane!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się zresetować: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnAddPointsClicked(object sender, EventArgs e)
        {
            try
            {
                // Dodaj 1000 punktów
                await _viewModel.PointsService.AddPointsAsync(1000);
                
                // Odśwież wyświetlanie
                await _viewModel.LoadDataAsync();
                PointsLabel.Text = $"💎 Punkty: {_viewModel.CurrentPoints}";
                
                await DisplayAlert("Sukces", "Dodano 1000 punktów!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się dodać punktów: {ex.Message}", "OK");
            }
        }

        private async void OnResetPlayerClicked(object sender, EventArgs e)
        {
            try
            {
                var confirmed = await DisplayAlert("Potwierdzenie", 
                    "Czy na pewno chcesz zresetować dane gracza?\n\nTo usunie:\n- Profil gracza\n- Historię punktów\n- Wszystkie loot boxy\n\nGracz zostanie utworzony na nowo z 5000 punktów.", 
                    "Tak, resetuj", "Anuluj");
                
                if (!confirmed) return;
                
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                
                // Resetuj dane gracza i loot boxy
                var databaseService = _viewModel.LootBoxService.GetType()
                    .GetField("_database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_viewModel.LootBoxService) as DatabaseService;
                
                if (databaseService != null)
                {
                    await databaseService.ResetPlayerDataAsync();
                    await databaseService.ResetLootBoxDataAsync();
                    
                    // Sprawdź status bazy
                    var status = await databaseService.GetDatabaseStatusAsync();
                    System.Diagnostics.Debug.WriteLine($"[Database] Status po resecie: {status}");
                }
                
                // Przeładuj wszystko
                await _viewModel.LootBoxService.InitializeDefaultLootBoxesAsync();
                await _viewModel.LoadDataAsync();
                PointsLabel.Text = $"💎 Punkty: {_viewModel.CurrentPoints}";
                
                await DisplayAlert("Sukces", 
                    $"Dane gracza zostały zresetowane!\n\nNowy gracz ma {_viewModel.CurrentPoints} punktów.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się zresetować gracza: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnAvatarShopClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("AvatarShopPage");
        }

        private async void OnLootBoxImageTapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is LootBoxDisplayModel lootBoxModel)
            {
                await ShowLootBoxPreview(lootBoxModel);
            }
        }

        private async void OnLootBoxButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LootBoxDisplayModel lootBoxModel)
            {
                System.Diagnostics.Debug.WriteLine($"[LootBoxPage] Kliknięto przycisk dla {lootBoxModel.Name}");
                
                try
                {
                    // Sprawdź czy mamy wystarczająco punktów
                    var playerProfile = await _viewModel.PointsService.GetPlayerProfileAsync();
                    if (playerProfile.TotalPoints < lootBoxModel.Price)
                    {
                        await DisplayAlert("Błąd", "Nie masz wystarczająco punktów!", "OK");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"[LootBoxPage] Uruchamianie animacji ruletki...");
                    await ShowRouletteAnimationAsync(lootBoxModel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LootBoxPage] Błąd: {ex.Message}");
                    await DisplayAlert("Błąd", $"Wystąpił błąd: {ex.Message}", "OK");
                }
            }
        }

        private async Task ShowLootBoxPreview(LootBoxDisplayModel lootBoxModel)
        {
            var confirmed = await DisplayAlert("Podgląd skrzynki",
                $"🎁 {lootBoxModel.Name}\n\n" +
                $"💰 Cena: {lootBoxModel.Price} punktów\n" +
                $"🌟 Rzadkość: {lootBoxModel.Rarity}\n\n" +
                $"📝 {lootBoxModel.Description}\n\n" +
                "Czy chcesz otworzyć tę skrzynkę?",
                "Otwórz", "Anuluj");

            if (confirmed)
            {
                await _viewModel.OpenLootBoxCommand.ExecuteAsync(lootBoxModel);
            }
        }

        private void OnOverlayTapped(object sender, EventArgs e)
        {
            OnClosePopupClicked(sender, e);
        }

        private async void OnClosePopupClicked(object sender, EventArgs e)
        {
            await CloseResultPopup();
        }

        private async Task CloseResultPopup()
        {
            // Animacja zamknięcia
            var popup = ResultPopup.Children[0] as Frame;
            if (popup != null)
            {
                await Task.WhenAll(
                    popup.ScaleTo(0.8, 200, Easing.CubicIn),
                    popup.FadeTo(0, 200)
                );
            }
            
            ResultPopup.IsVisible = false;
            
            // Odśwież dane po zamknięciu popup'a
            await _viewModel.LoadDataAsync();
            PointsLabel.Text = $"💎 Punkty: {_viewModel.CurrentPoints}";
        }

        public async Task ShowRewardAsync(LootBoxResult result, LootBox lootBox)
        {
            try
            {
                // Pokaż animację otwierania
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                
                await ShowOpeningAnimationAsync();
                
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;

                // Przygotuj dane nagrody
                if (result.RewardType == "Avatar")
                {
                    var avatarId = int.Parse(result.RewardValue);
                    var avatar = await _viewModel.GetAvatarByIdAsync(avatarId);
                    
                    if (avatar != null)
                    {
                        RewardImage.Source = avatar.LottieFileName ?? avatar.PreviewImagePath ?? "avatar_robot_basic.png";
                        RewardName.Text = avatar.Name;
                        RewardDescription.Text = avatar.Description;
                        RewardLabel.Text = result.PointsGained > 0 ? 
                            $"🎁 Otrzymałeś {result.PointsGained} punktów (duplikat):" : 
                            "🎊 Odblokowałeś nowego awatara:";
                    }
                }
                else
                {
                    RewardImage.Source = "coin_icon.png";
                    RewardName.Text = $"{result.RewardValue} Punktów";
                    RewardDescription.Text = "Dodano do Twojego konta";
                    RewardLabel.Text = "💰 Wygrałeś punkty:";
                }

                // Pokaż popup z animacją
                ResultPopup.IsVisible = true;
                var popup = ResultPopup.Children[0] as Frame;
                if (popup != null)
                {
                    popup.Scale = 0.5;
                    popup.Opacity = 0;
                    
                    await Task.WhenAll(
                        popup.ScaleTo(1.0, 400, Easing.BounceOut),
                        popup.FadeTo(1.0, 300)
                    );
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                await DisplayAlert("Błąd", $"Wystąpił problem podczas pokazywania nagrody: {ex.Message}", "OK");
            }
        }

        private async Task ShowOpeningAnimationAsync()
        {
            // Symulacja animacji otwierania z lepszymi efektami
            await Task.Delay(1500);
        }

        // ===== NOWE METODY DLA ANIMACJI RULETKI =====

        public async Task ShowRouletteAnimationAsync(LootBoxDisplayModel lootBoxModel)
        {
            try
            {
                // Przygotuj elementy ruletki
                await PrepareRouletteItemsAsync(lootBoxModel.Id);
                
                // Pokaż popup ruletki
                RoulettePopup.IsVisible = true;
                RouletteStatusLabel.Text = "Losowanie nagrody...";
                CancelRouletteButton.IsVisible = false;
                
                // Animacja pojawiania się popup'a
                var popup = RoulettePopup.Children[0] as Frame;
                if (popup != null)
                {
                    popup.Scale = 0.5;
                    popup.Opacity = 0;
                    await Task.WhenAll(
                        popup.ScaleTo(1.0, 300, Easing.BounceOut),
                        popup.FadeTo(1.0, 250)
                    );
                }

                await Task.Delay(500); // Krótka pauza na pokazanie elementów

                // Uruchom animację ruletki
                var result = await _viewModel.LootBoxService.OpenLootBoxAsync(lootBoxModel.Id);
                await AnimateRouletteAsync(result);

                // Ukryj ruletę i pokaż wynik
                await HideRouletteAndShowResultAsync(result, lootBoxModel.ToLootBox());
            }
            catch (Exception ex)
            {
                RoulettePopup.IsVisible = false;
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas animacji: {ex.Message}", "OK");
            }
        }

        private async Task PrepareRouletteItemsAsync(int lootBoxId)
        {
            // Pobierz dostępne nagrody dla tej skrzynki
            var rewards = await GetLootBoxRewardsForRouletteAsync(lootBoxId);
            
            // Pobierz awatary bezpośrednio z bazy danych
            var databaseService = _viewModel.LootBoxService.GetType()
                .GetField("_database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_viewModel.LootBoxService) as DatabaseService;
            
            List<Avatar> allAvatars = new List<Avatar>();
            if (databaseService != null)
            {
                allAvatars = await databaseService.GetAllAvatarsAsync();
            }
            
            var rouletteItems = new List<RouletteItemModel>();

            // Dodaj elementy do ruletki (dużo elementów dla efektu przewijania)
            for (int cycle = 0; cycle < 8; cycle++) // 8 cykli elementów
            {
                foreach (var reward in rewards)
                {
                    var item = new RouletteItemModel
                    {
                        RewardType = reward.RewardType,
                        RewardValue = reward.RewardValue,
                        Rarity = reward.Rarity
                    };

                    if (reward.RewardType == "Avatar")
                    {
                        var avatarId = int.Parse(reward.RewardValue);
                        var avatar = allAvatars.FirstOrDefault(a => a.Id == avatarId);
                        if (avatar != null)
                        {
                            item.Name = avatar.Name;
                            item.ImagePath = avatar.ImagePath;
                            item.Value = "AWATAR";
                        }
                        else
                        {
                            item.Name = "Awatar";
                            item.ImagePath = "hackyeah_default.png";
                            item.Value = "AWATAR";
                        }
                    }
                    else if (reward.RewardType == "Points")
                    {
                        item.Name = "Punkty";
                        item.ImagePath = "coin_icon.png";
                        item.Value = $"{reward.RewardValue} pkt";
                    }

                    rouletteItems.Add(item);
                }
            }

            // Wymieszaj elementy dla lepszego efektu
            var random = new Random();
            rouletteItems = rouletteItems.OrderBy(x => random.Next()).ToList();

            _viewModel.RouletteItems.Clear();
            foreach (var item in rouletteItems)
            {
                _viewModel.RouletteItems.Add(item);
            }
        }

        private async Task<List<LootBoxReward>> GetLootBoxRewardsForRouletteAsync(int lootBoxId)
        {
            try
            {
                // Użyj refleksji żeby dostać się do prywatnej metody
                var method = _viewModel.LootBoxService.GetType()
                    .GetMethod("GetLootBoxRewardsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method != null)
                {
                    var task = method.Invoke(_viewModel.LootBoxService, new object[] { lootBoxId }) as Task<List<LootBoxReward>>;
                    return await task ?? new List<LootBoxReward>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Roulette] Błąd pobierania nagród: {ex.Message}");
            }

            // Fallback - podstawowe nagrody
            return new List<LootBoxReward>
            {
                new LootBoxReward { RewardType = "Points", RewardValue = "25", Rarity = "Common" },
                new LootBoxReward { RewardType = "Points", RewardValue = "50", Rarity = "Rare" },
                new LootBoxReward { RewardType = "Points", RewardValue = "100", Rarity = "Epic" }
            };
        }

        private async Task AnimateRouletteAsync(LootBoxResult winningResult)
        {
            if (RouletteItemsView == null) return;

            RouletteStatusLabel.Text = "🎰 Kręcenie ruletki...";
            
            // Znajdź indeks wygrywającego elementu (w środku listy dla lepszego efektu)
            var totalItems = _viewModel.RouletteItems.Count;
            var winningIndex = totalItems / 2; // Środek listy
            
            // Ustaw odpowiedni element na pozycji wygrywającej
            if (winningIndex < _viewModel.RouletteItems.Count)
            {
                var winningItem = _viewModel.RouletteItems[winningIndex];
                
                if (winningResult.RewardType == "Avatar")
                {
                    var avatarId = int.Parse(winningResult.RewardValue);
                    var avatar = await _viewModel.GetAvatarByIdAsync(avatarId);
                    if (avatar != null)
                    {
                        winningItem.Name = avatar.Name;
                        winningItem.ImagePath = avatar.ImagePath;
                        winningItem.Value = "AWATAR";
                        winningItem.RewardType = "Avatar";
                        winningItem.RewardValue = winningResult.RewardValue;
                        winningItem.Rarity = avatar.Rarity;
                    }
                }
                else
                {
                    winningItem.Name = "Punkty";
                    winningItem.ImagePath = "coin_icon.png";
                    winningItem.Value = $"{winningResult.RewardValue} pkt";
                    winningItem.RewardType = "Points";
                    winningItem.RewardValue = winningResult.RewardValue;
                }
            }

            // Animacja scrollowania (symulacja - bo ScrollView nie ma łatwej animacji)
            // Robimy to przez zmiany opacity i scale elementów
            for (int i = 0; i < 50; i++) // 50 kroków animacji
            {
                // Szybkość animacji - początkowo szybko, potem wolniej
                var speed = Math.Max(20, 200 - (i * 4)); 
                await Task.Delay((int)speed);
                
                // Aktualizuj status
                if (i % 10 == 0)
                {
                    RouletteStatusLabel.Text = $"🎰 Kręcenie ruletki... {(i * 2)}%";
                }
            }

            // Finalizacja
            RouletteStatusLabel.Text = "🎉 Wynik jest gotowy!";
            await Task.Delay(1000);
        }

        private async Task HideRouletteAndShowResultAsync(LootBoxResult result, LootBox lootBox)
        {
            // Ukryj ruletę z animacją
            var popup = RoulettePopup.Children[0] as Frame;
            if (popup != null)
            {
                await Task.WhenAll(
                    popup.ScaleTo(0.8, 200, Easing.CubicIn),
                    popup.FadeTo(0, 200)
                );
            }
            
            RoulettePopup.IsVisible = false;
            
            // Pokaż wynik
            await ShowRewardAsync(result, lootBox);
        }

        private async void OnCancelRouletteClicked(object sender, EventArgs e)
        {
            // Anuluj animację (opcjonalnie)
            RoulettePopup.IsVisible = false;
        }
    }

    public partial class LootBoxViewModel : ObservableObject
    {
        private readonly LootBoxService _lootBoxService;
        private readonly PointsService _pointsService;

        [ObservableProperty]
        private ObservableCollection<LootBoxDisplayModel> lootBoxes = new();

        [ObservableProperty]
        private int currentPoints;

        [ObservableProperty]
        private ObservableCollection<RouletteItemModel> rouletteItems = new();

        // Publiczne właściwości dla dostępu z code-behind
        public LootBoxService LootBoxService => _lootBoxService;
        public PointsService PointsService => _pointsService;

        public LootBoxViewModel(LootBoxService lootBoxService, PointsService pointsService)
        {
            _lootBoxService = lootBoxService;
            _pointsService = pointsService;

            // Subskrybuj zmiany punktów - ale odśwież dane z PlayerProfile
            WeakReferenceMessenger.Default.Register<PointsChangedMessage>(this, async (r, m) =>
            {
                // Po zmianie punktów, odśwież punkty z PlayerProfile.TotalPoints
                var playerProfile = await _pointsService.GetPlayerProfileAsync();
                CurrentPoints = playerProfile.TotalPoints;
            });
        }

        public async Task LoadDataAsync()
        {
            await _lootBoxService.InitializeDefaultLootBoxesAsync();
            
            var lootBoxData = await _lootBoxService.GetAvailableLootBoxesAsync();
            var displayModels = lootBoxData.Select(lb => new LootBoxDisplayModel(lb)).ToList();
            
            // Pobierz punkty z Models.PlayerProfile.TotalPoints
            var playerProfile = await _pointsService.GetPlayerProfileAsync();
            CurrentPoints = playerProfile.TotalPoints;
            
            // Zaktualizuj status "CanAfford" dla każdego lootboxa
            foreach (var model in displayModels)
            {
                model.UpdateAffordability(CurrentPoints);
            }
            
            LootBoxes.Clear();
            foreach (var model in displayModels)
            {
                LootBoxes.Add(model);
            }
            
            // Debug: sprawdź ile punktów faktycznie mamy
            System.Diagnostics.Debug.WriteLine($"[LootBoxPage] Załadowano punkty: {CurrentPoints}");
            System.Diagnostics.Debug.WriteLine($"[LootBoxPage] Dostępne lootboxy:");
            foreach (var box in LootBoxes)
            {
                System.Diagnostics.Debug.WriteLine($"  - {box.Name}: {box.Price} pkt (można kupić: {box.CanAfford})");
            }
        }

        [RelayCommand]
        private async Task OpenLootBox(LootBoxDisplayModel lootBoxModel)
        {
            try
            {
                // Sprawdź czy mamy wystarczająco punktów
                var playerProfile = await _pointsService.GetPlayerProfileAsync();
                if (playerProfile.TotalPoints < lootBoxModel.Price)
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Nie masz wystarczająco punktów!", "OK");
                    return;
                }

                // Pokaż animację ruletki zamiast natychmiastowego wyniku
                if (Application.Current?.MainPage is AppShell shell)
                {
                    var navigation = shell.Navigation;
                    var currentPage = navigation.NavigationStack.LastOrDefault();
                    if (currentPage is LootBoxPage lootBoxPage)
                    {
                        await lootBoxPage.ShowRouletteAnimationAsync(lootBoxModel);
                    }
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("punktów"))
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Nie masz wystarczająco punktów!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Wystąpił błąd: {ex.Message}", "OK");
            }
        }

        public async Task<Avatar?> GetAvatarByIdAsync(int avatarId)
        {
            return await _lootBoxService.GetAvatarByIdAsync(avatarId);
        }
    }

    public class LootBoxDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public bool CanAfford { get; set; } = true; // Dodana właściwość sprawdzania punktów

        public string RarityColor => Rarity switch
        {
            "Common" => "#808080",
            "Rare" => "#0066cc",
            "Epic" => "#9933cc",
            "Legendary" => "#ff6600",
            _ => "#808080"
        };

        public string RarityBackgroundColor => Rarity switch
        {
            "Common" => "#40808080",
            "Rare" => "#400066cc",
            "Epic" => "#409933cc",
            "Legendary" => "#40ff6600",
            _ => "#40808080"
        };

        public string ButtonText => CanAfford ? "📦 OTWÓRZ SKRZYNKĘ" : "💎 Za mało punktów";
        public Color ButtonColor => CanAfford ? Colors.Green : Colors.Red;

        public LootBoxDisplayModel(LootBox lootBox)
        {
            Id = lootBox.Id;
            Name = lootBox.Name;
            Description = lootBox.Description;
            Price = lootBox.Price;
            ImagePath = lootBox.ImagePath;
            Rarity = lootBox.Rarity;
        }

        // Metoda do aktualizacji statusu czy można kupić
        public void UpdateAffordability(int currentPoints)
        {
            CanAfford = currentPoints >= Price;
            System.Diagnostics.Debug.WriteLine($"[LootBox] {Name}: {Price} pkt, Gracz ma: {currentPoints}, Może kupić: {CanAfford}");
        }

        public LootBox ToLootBox()
        {
            return new LootBox
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Price = Price,
                ImagePath = ImagePath,
                Rarity = Rarity
            };
        }
    }

    // Model dla elementów ruletki
    public class RouletteItemModel
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public string RewardValue { get; set; } = string.Empty;

        public string BackgroundColor => Rarity switch
        {
            "Common" => "#404040",
            "Rare" => "#0066cc",
            "Epic" => "#9933cc",
            "Legendary" => "#ff6600",
            _ => "#404040"
        };

        public string BorderColor => Rarity switch
        {
            "Common" => "#808080",
            "Rare" => "#0088ff",
            "Epic" => "#bb44ff",
            "Legendary" => "#ff8800",
            _ => "#808080"
        };

        public string ValueColor => RewardType switch
        {
            "Avatar" => "#ffdd44",
            "Points" => "#44ff44",
            _ => "#ffffff"
        };
    }
}
