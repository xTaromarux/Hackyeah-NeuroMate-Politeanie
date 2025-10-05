using Microsoft.Maui.Controls;
using NeuroMate.Services;
using NeuroMate.Database.Entities;
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
    }

    public partial class LootBoxViewModel : ObservableObject
    {
        private readonly LootBoxService _lootBoxService;
        private readonly PointsService _pointsService;

        [ObservableProperty]
        private ObservableCollection<LootBoxDisplayModel> lootBoxes = new();

        [ObservableProperty]
        private int currentPoints;

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
            
            LootBoxes.Clear();
            foreach (var model in displayModels)
            {
                LootBoxes.Add(model);
            }

            // Pobierz punkty z Models.PlayerProfile.TotalPoints
            var playerProfile = await _pointsService.GetPlayerProfileAsync();
            CurrentPoints = playerProfile.TotalPoints;
        }

        [RelayCommand]
        private async Task OpenLootBoxAsync(LootBoxDisplayModel lootBoxModel)
        {
            try
            {
                var result = await _lootBoxService.OpenLootBoxAsync(lootBoxModel.Id);
                
                // Znajdź stronę i pokaż animację
                if (Application.Current?.MainPage is AppShell shell)
                {
                    var navigation = shell.Navigation;
                    var currentPage = navigation.NavigationStack.LastOrDefault();
                    if (currentPage is LootBoxPage lootBoxPage)
                    {
                        await lootBoxPage.ShowRewardAsync(result, lootBoxModel.ToLootBox());
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

        public LootBoxDisplayModel(LootBox lootBox)
        {
            Id = lootBox.Id;
            Name = lootBox.Name;
            Description = lootBox.Description;
            Price = lootBox.Price;
            ImagePath = lootBox.ImagePath;
            Rarity = lootBox.Rarity;
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
}
