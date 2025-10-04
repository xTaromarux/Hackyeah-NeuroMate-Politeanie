using NeuroMate.ViewModels;
using NeuroMate.Views;
using NeuroMate.Services;

namespace NeuroMate
{
    public partial class MainPage : ContentPage
    {
        private readonly Random _random = new();
        private IFloatingAvatarService? _floatingAvatarService;
        private readonly List<string> _avatarMessages = new()
        {
            "Czas na przerwę! Wykonajmy razem szybki test Stroop 🎨",
            "Zauważyłem, że pracujesz już długo. Może krótka gra na reakcję? ⚡",
            "Twoja koncentracja może potrzebować odświeżenia. Spróbujmy Task Switching! 🔄",
            "Pora na mikro-przerwę! Wykonaj ze mną ćwiczenia oczu 👁️",
            "Twój mózg potrzebuje wyzwania. Zagrajmy w N-back! 🧠",
            "Czas na ruch! Wstań i rozciągnij się przez minutę 🤸‍♂️",
            "Może coś do picia? Nawodnienie jest ważne dla mózgu! 💧"
        };

        private Timer? _interventionTimer;
        private DateTime _lastInterventionTime = DateTime.Now;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();

            // Uruchom timer interwencji
            StartInterventionTimer();

            // Aktualizuj dane startowe
            UpdateDashboardData();

            // Inicjalizuj dialog avatara
            InitializeAvatarDialog();
        }

        private void InitializeAvatarDialog()
        {
            // Ustaw początkowy stan okna dialogowego
            AvatarDialogOverlay.IsVisible = false;
            AvatarDialogOverlay.Opacity = 0;
            AvatarDialogCard.Scale = 0.8;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler != null)
            {
                // Opóźnij inicjalizację avatara, żeby strona była w pełni załadowana
                Dispatcher.Dispatch(async () =>
                {
                    await Task.Delay(1000);

                    _floatingAvatarService = Handler.MauiContext?.Services?.GetService<IFloatingAvatarService>();

                    if (_floatingAvatarService != null)
                    {
                        UpdateDashboardData();
                    }
                });
            }
        }

        private async void OnAvatarTapped(object sender, EventArgs e)
        {
            await ShowAvatarDialog();
        }

        private async Task ShowAvatarDialog()
        {
            // Ustaw losową wiadomość
            var message = _avatarMessages[_random.Next(_avatarMessages.Count)];
            AvatarMessageLabel.Text = message;

            // Pokaż dialog z animacją
            AvatarDialogOverlay.IsVisible = true;

            await Task.WhenAll(
                AvatarDialogOverlay.FadeTo(1, 300, Easing.CubicOut),
                AvatarDialogCard.ScaleTo(1, 300, Easing.SpringOut)
            );

            // Rozpocznij animację wskaźnika mowy
            StartSpeechIndicatorAnimation();
        }

        private async Task HideAvatarDialog()
        {
            await Task.WhenAll(
                AvatarDialogOverlay.FadeTo(0, 250, Easing.CubicIn),
                AvatarDialogCard.ScaleTo(0.9, 250, Easing.CubicIn)
            );

            AvatarDialogOverlay.IsVisible = false;
        }

        private async void StartSpeechIndicatorAnimation()
        {
            while (AvatarDialogOverlay.IsVisible && AvatarDialogOverlay.Opacity > 0)
            {
                await SpeechIndicator.ScaleTo(1.2, 600, Easing.SinInOut);
                await SpeechIndicator.ScaleTo(1.0, 600, Easing.SinInOut);
                await Task.Delay(100);
            }
        }

        // Event handlers dla dialogu avatara
        private async void OnDialogOverlayTapped(object sender, EventArgs e)
        {
            await HideAvatarDialog();
        }

        private async void OnCloseDialogClicked(object sender, EventArgs e)
        {
            await HideAvatarDialog();
        }

        private async void OnDialogStartTrainingClicked(object sender, EventArgs e)
        {
            await HideAvatarDialog();
            await Shell.Current.GoToAsync("///CognitiveGames");
        }

        private async void OnDialogShowStatsClicked(object sender, EventArgs e)
        {
            await HideAvatarDialog();
            await Shell.Current.GoToAsync("///DailySummary");
        }

        private async void OnDialogSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Ustawienia", "Funkcja ustawień będzie dostępna w przyszłej wersji!", "OK");
        }

        // Event handlers dla przycisków głównych
        private async void OnStroopTestClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///StroopGame");
        }

        private async void OnPvtTestClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///PvtGame");
        }

        private async void OnTaskSwitchClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///TaskSwitchingGame");
        }

        private async void OnSummaryClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///DailySummary");
        }

        private async void OnToggleAvatarClicked(object sender, EventArgs e)
        {
            if (_floatingAvatarService != null)
            {
                await DisplayAlert("Avatar", "Funkcja przełączania avatara będzie dostępna wkrótce!", "OK");
            }
        }

        private async void OnStartRecommendationClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///StroopGame");
        }

        private async void OnMoreRecommendationsClicked(object sender, EventArgs e)
        {
            await DisplayAlert(
                "Rekomendacje AI",
                "• Test Stroop - popraw koncentrację\n• Gra PVT - zwiększ szybkość reakcji\n• Task Switching - wzmocnij elastyczność poznawczą\n• N-back - trenuj pamięć roboczą",
                "OK"
            );
        }

        private void UpdateDashboardData()
        {
            // Symulacja danych dla demonstracji
            var focusLevel = _random.Next(60, 100);
            var workHours = _random.Next(1, 8);
            var workMinutes = _random.Next(0, 60);
            var sleepScore = _random.Next(50, 100);
            var stressLevel = _random.Next(20, 80);

            // Aktualizuj UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FocusLevelLabel.Text = focusLevel > 75 ? "Wysoka" : focusLevel > 50 ? "Średnia" : "Niska";
                FocusLevelLabel.TextColor = focusLevel > 75 ? Colors.Green : focusLevel > 50 ? Colors.Orange : Colors.Red;
                FocusProgressBar.Progress = focusLevel / 100.0;

                WorkTimeLabel.Text = $"{workHours}h {workMinutes}m";
                WorkStatusLabel.Text = workHours < 6 ? "W porządku" : "Czas na przerwę";
                WorkStatusLabel.TextColor = workHours < 6 ? Colors.Green : Colors.Orange;

                SleepScoreLabel.Text = $"{sleepScore}/100";
                SleepScoreLabel.TextColor = sleepScore > 70 ? Colors.Blue : Colors.Orange;

                StressLevelLabel.Text = stressLevel < 40 ? "Niski" : stressLevel < 70 ? "Średni" : "Wysoki";
                StressLevelLabel.TextColor = stressLevel < 40 ? Colors.Green : stressLevel < 70 ? Colors.Orange : Colors.Red;

                NeuroScoreLabel.Text = ((focusLevel + sleepScore + (100 - stressLevel)) / 3).ToString();

                StatusLabel.Text = focusLevel > 75 ? "Gotowy do treningu!" : "Może czas na odpoczynek?";
            });
        }

        private async void StartInterventionTimer()
        {
            _interventionTimer = new Timer(async _ =>
            {
                if (DateTime.Now - _lastInterventionTime > TimeSpan.FromMinutes(30))
                {
                    _lastInterventionTime = DateTime.Now;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await ShowAvatarDialog();
                    });
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateDashboardData();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _interventionTimer?.Dispose();
        }
    }
}