using NeuroMate.ViewModels;
using NeuroMate.Views;
using NeuroMate.Services;
using SkiaSharp.Extended.UI.Controls;

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
            var viewModel = new MainViewModel();
            BindingContext = viewModel;
            
            // Subskrypcja do zmian w ViewModelu dla aktualizacji UI
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            // Nasłuchuj komunikatów o zmianach z innych stron
            MessagingCenter.Subscribe<AvatarShopPage>(this, "AvatarChanged", async (sender) =>
            {
                // Odśwież dane awatara po zmianie w sklepie
                if (BindingContext is MainViewModel vm)
                {
                    await vm.RefreshDataAsync();
                }
            });
            
            MessagingCenter.Subscribe<AvatarShopPage>(this, "PointsChanged", async (sender) =>
            {
                // Odśwież dane punktów po zakupie w sklepie
                if (BindingContext is MainViewModel vm)
                {
                    await vm.RefreshDataAsync();
                }
            });
            
            // Uruchom timer interwencji
            StartInterventionTimer();
            
            // Aktualizuj dane startowe
            UpdateDashboardData();
            
            // Inicjalizuj dialog avatara
            InitializeAvatarDialog();
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (BindingContext is not MainViewModel viewModel) return;

            // Aktualizuj UI na podstawie zmian w ViewModelu
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.TotalPoints):
                    TotalPointsLabel.Text = viewModel.TotalPoints.ToString();
                    break;
                case nameof(MainViewModel.PointsEarnedToday):
                    TodayPointsLabel.Text = $"Dziś: +{viewModel.PointsEarnedToday}";
                    break;
                case nameof(MainViewModel.CurrentAvatarName):
                    CurrentAvatarNameLabel.Text = viewModel.CurrentAvatarName;
                    break;
                case nameof(MainViewModel.CurrentAvatarLottie):
                    MainAvatarImage.Source = viewModel.CurrentAvatarLottie;
                    DialogAvatarImage.Source = viewModel.CurrentAvatarLottie;
                    break;
                case nameof(MainViewModel.NeuroScore):
                    NeuroScoreLabel.Text = viewModel.NeuroScore.ToString();
                    break;
                case nameof(MainViewModel.AssistantHint):
                    AvatarMessageLabel.Text = viewModel.AssistantHint;
                    break;
            }
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
            // Wybierz odpowiednią animację na podstawie wyników
            string animationFile = GetAvatarAnimationBasedOnPerformance();
            DialogAvatarVideo.Source = animationFile;
            
            // Ustaw wiadomość na podstawie wyników
            var message = GetAvatarMessageBasedOnPerformance();
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
            await Shell.Current.GoToAsync("//CognitiveGames");
        }

        private async void OnDialogShowStatsClicked(object sender, EventArgs e)
        {
            await HideAvatarDialog();
            await Shell.Current.GoToAsync("//DailySummary");
        }

        private async void OnDialogSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Ustawienia", "Funkcja ustawień będzie dostępna w przyszłej wersji!", "OK");
        }

        // Event handlers dla przycisków głównych
        private async void OnStroopTestClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//StroopGame");
        }

        private async void OnPvtTestClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PvtGame");
        }

        private async void OnTaskSwitchClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//TaskSwitchingGame");
        }

        private async void OnSummaryClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//DailySummary");
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
            await Shell.Current.GoToAsync("//StroopGame");
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Odśwież dane ViewModelu, w tym aktualnego awatara
            if (BindingContext is MainViewModel viewModel)
            {
                await viewModel.RefreshDataAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Wyczyść subskrypcje MessagingCenter aby uniknąć memory leaks
            MessagingCenter.Unsubscribe<AvatarShopPage>(this, "AvatarChanged");
            MessagingCenter.Unsubscribe<AvatarShopPage>(this, "PointsChanged");
        }

        private string GetAvatarAnimationBasedOnPerformance()
        {
            // Pobierz aktualne dane z ViewModelu
            if (BindingContext is not MainViewModel viewModel)
                return "idle.mkv"; // Domyślna animacja

            // Analiza wyników użytkownika
            int neuroScore = viewModel.NeuroScore;
            int avgReactionMs = viewModel.AvgReactionMs;
            int pointsEarnedToday = viewModel.PointsEarnedToday;
            
            // Oblicz ogólny wynik na podstawie różnych metryk
            double performanceScore = CalculateOverallPerformance(neuroScore, avgReactionMs, pointsEarnedToday);
            
            // Wybierz animację na podstawie wyniku
            return performanceScore switch
            {
                > 0.7 => "wave.mkv",     // Dobry wynik - machanie
                < 0.3 => "sad.mkv",      // Słaby wynik - smutek
                _ => "idle.mkv"          // Neutralny wynik - spokojny
            };
        }

        private string GetAvatarMessageBasedOnPerformance()
        {
            if (BindingContext is not MainViewModel viewModel)
                return "Cześć! Jak się masz?";

            int neuroScore = viewModel.NeuroScore;
            int avgReactionMs = viewModel.AvgReactionMs;
            int pointsEarnedToday = viewModel.PointsEarnedToday;
            
            double performanceScore = CalculateOverallPerformance(neuroScore, avgReactionMs, pointsEarnedToday);
            
            var goodMessages = new[]
            {
                "🎉 Świetna robota! Twoje wyniki są imponujące!",
                "💪 Jesteś w doskonałej formie! Tak trzymaj!",
                "⭐ Fantastyczne rezultaty! Twój mózg jest w topowej formie!",
                "🚀 Wow! Twoja koncentracja jest na najwyższym poziomie!",
                "👏 Brawo! Osiągasz znakomite wyniki w testach!"
            };

            var neutralMessages = new[]
            {
                "🧠 Twoje wyniki są w normie. Może czas na kolejne wyzwanie?",
                "⚖️ Stabilne rezultaty! Chcesz popracować nad koncentracją?",
                "📊 Wyniki wyglądają przeciętnie. Sprawdźmy co możemy poprawić!",
                "🎯 Jesteś na dobrej drodze. Może spróbujemy nowego ćwiczenia?",
                "💡 Twój mózg pracuje stabilnie. Czas na dodatkowy trening!"
            };

            var poorMessages = new[]
            {
                "😔 Widzę że możesz się czuć zmęczony. Może zrobimy przerwę?",
                "💤 Twój mózg potrzebuje odpoczynku. Czas na regenerację!",
                "🔋 Wygląda na to że energia ci się kończy. Zrób sobie przerwę!",
                "☕ Może czas na kawę? Twoja koncentracja wydaje się osłabiona.",
                "🌿 Nie martw się! Każdy ma gorsze dni. Jutro będzie lepiej!"
            };

            var messages = performanceScore switch
            {
                > 0.7 => goodMessages,
                < 0.3 => poorMessages,
                _ => neutralMessages
            };

            return messages[_random.Next(messages.Length)];
        }

        private double CalculateOverallPerformance(int neuroScore, int avgReactionMs, int pointsEarnedToday)
        {
            double score = 0;
            int factors = 0;

            // NeuroScore (0-100) -> 0-1
            if (neuroScore > 0)
            {
                score += neuroScore / 100.0;
                factors++;
            }

            // Czas reakcji (im mniejszy tym lepszy)
            if (avgReactionMs > 0)
            {
                // Zakładamy że 200ms to doskonały czas, 500ms+ to słaby
                double reactionScore = Math.Max(0, Math.Min(1, (500 - avgReactionMs) / 300.0));
                score += reactionScore;
                factors++;
            }

            // Punkty dzisiaj (względem oczekiwanej dziennej normy, np. 100 pkt)
            if (pointsEarnedToday >= 0)
            {
                double pointsScore = Math.Min(1, pointsEarnedToday / 100.0);
                score += pointsScore;
                factors++;
            }

            return factors > 0 ? score / factors : 0.5; // Domyślnie neutralny
        }
    }
}