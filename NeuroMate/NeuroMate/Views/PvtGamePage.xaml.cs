using NeuroMate.Database;
using System.Diagnostics;

namespace NeuroMate.Views
{
    public partial class PvtGamePage : ContentPage
    {
        private DatabaseService _dbService => App.Services.GetService<DatabaseService>()!;
        private readonly Random _random = new();
        private Timer? _gameTimer;
        private Stopwatch _reactionStopwatch = new();
        private List<int> _reactionTimes = new();
        
        private bool _isGameRunning = false;
        private bool _isPaused = false;
        private int _currentTrial = 0;
        private int _totalTrials = 20;
        private int _gameTimeLeft = 180; // 3 minuty

        public PvtGamePage()
        {
            InitializeComponent();
            ResetGameStats();
        }

        private void ResetGameStats()
        {
            _currentTrial = 0;
            _reactionTimes.Clear();
            UpdateStats();
        }

        private void UpdateStats()
        {
            TrialsLabel.Text = $"{_currentTrial}/{_totalTrials}";
            
            if (_reactionTimes.Count > 0)
            {
                var avgRT = _reactionTimes.Average();
                var fastestRT = _reactionTimes.Min();
                
                AvgRTLabel.Text = $"{avgRT:F0}ms";
                FastestRTLabel.Text = $"{fastestRT}ms";
            }
            else
            {
                AvgRTLabel.Text = "0ms";
                FastestRTLabel.Text = "0ms";
            }
        }

        private async void OnStartStopClicked(object sender, EventArgs e)
        {
            if (!_isGameRunning)
            {
                await StartGame();
            }
            else
            {
                StopGame();
            }
        }

        private async Task StartGame()
        {
            _isGameRunning = true;
            _gameTimeLeft = 180;
            ResetGameStats();
            
            StartStopButton.Text = "⏹️ Stop";
            PauseButton.IsVisible = true;
            InstructionLabel.Text = "Czekaj na czerwone światło...";
            
            // Timer gry (odliczanie)
            _gameTimer = new Timer(GameTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            
            // Zacznij pierwszy test
            await ScheduleNextStimulus();
        }

        private void GameTimerTick(object? state)
        {
            if (_isPaused) return;
            
            _gameTimeLeft--;
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = $"{_gameTimeLeft}s";
                
                if (_gameTimeLeft <= 0 || _currentTrial >= _totalTrials)
                {
                    EndGame();
                }
            });
        }

        private async Task ScheduleNextStimulus()
        {
            if (!_isGameRunning || _isPaused) return;
            
            // Pokaż stan oczekiwania
            ShowWaitingState();
            
            // Losowy czas oczekiwania (2-10 sekund)
            var waitTime = _random.Next(2000, 10000);
            
            await Task.Delay(waitTime);
            
            if (_isGameRunning && !_isPaused)
            {
                ShowRedLight();
            }
        }

        private void ShowWaitingState()
        {
            WaitingState.IsVisible = true;
            RedLightState.IsVisible = false;
            ConfirmationState.IsVisible = false;
            TestArea.BackgroundColor = Color.FromArgb("#f5f5f5");
        }

        private void ShowRedLight()
        {
            WaitingState.IsVisible = false;
            RedLightState.IsVisible = true;
            ConfirmationState.IsVisible = false;
            TestArea.BackgroundColor = Color.FromArgb("#fee");
            
            _reactionStopwatch.Restart();
        }

        private async void OnReactionButtonClicked(object sender, EventArgs e)
        {
            if (!RedLightState.IsVisible) 
            {
                // Kliknięto za wcześnie
                await DisplayAlert("⚠️ Za wcześnie!", "Poczekaj na czerwone światło!", "OK");
                return;
            }

            // Zatrzymaj pomiar reakcji
            _reactionStopwatch.Stop();
            var reactionTime = (int)_reactionStopwatch.ElapsedMilliseconds;
            _reactionTimes.Add(reactionTime);
            _currentTrial++;

            // Pokaż wynik
            ShowConfirmation(reactionTime);
            
            // Aktualizuj statystyki
            UpdateStats();
            
            // Zaplanuj następny test po 1.5 sekundy
            await Task.Delay(1500);
            
            if (_currentTrial < _totalTrials && _isGameRunning)
            {
                await ScheduleNextStimulus();
            }
            else if (_currentTrial >= _totalTrials)
            {
                EndGame();
            }
        }

        private void ShowConfirmation(int reactionTime)
        {
            WaitingState.IsVisible = false;
            RedLightState.IsVisible = false;
            ConfirmationState.IsVisible = true;
            TestArea.BackgroundColor = Color.FromArgb("#f0fff0");
            
            ReactionTimeLabel.Text = $"{reactionTime}ms";
            
            // Ocena reakcji
            if (reactionTime < 250)
            {
                ReactionTimeLabel.TextColor = Color.FromArgb("#48bb78"); // Excellent
            }
            else if (reactionTime < 400)
            {
                ReactionTimeLabel.TextColor = Color.FromArgb("#4facfe"); // Good
            }
            else
            {
                ReactionTimeLabel.TextColor = Color.FromArgb("#ed8936"); // Slow
            }
        }

        private void OnPauseClicked(object sender, EventArgs e)
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void PauseGame()
        {
            _isPaused = true;
            PauseButton.Text = "▶️ Wznów";
            InstructionLabel.Text = "Gra wstrzymana";
            ShowWaitingState();
        }

        private async void ResumeGame()
        {
            _isPaused = false;
            PauseButton.Text = "⏸️ Pauza";
            InstructionLabel.Text = "Czekaj na czerwone światło...";
            
            await ScheduleNextStimulus();
        }

        private void StopGame()
        {
            _isGameRunning = false;
            _gameTimer?.Dispose();
            
            StartStopButton.Text = "🚀 Start";
            PauseButton.IsVisible = false;
            InstructionLabel.Text = "Kliknij Start aby rozpocząć test";
            
            ShowWaitingState();
        }

        private async void EndGame()
        {
            StopGame();
            
            // Pokaż wyniki końcowe
            var avgRT = _reactionTimes.Count > 0 ? _reactionTimes.Average() : 0;
            var fastestRT = _reactionTimes.Count > 0 ? _reactionTimes.Min() : 0;

            await AddDataToDb((int)avgRT);

            await DisplayAlert("🎉 Test zakończony!", 
                $"Wykonałeś {_currentTrial} prób\n" +
                $"Średni czas reakcji: {avgRT:F0}ms\n" +
                $"Najszybszy czas: {fastestRT}ms\n\n" +
                $"Wynik zostanie zapisany w Twoim profilu!", 
                "OK");
        }

        private async Task AddDataToDb(int avgRT)
        {
            var GameReactionRecord = new Database.Entities.GameReactionRecord
            {
                ReactionTimeMs = avgRT,
                IsValid = false,
            };

           await _dbService.SaveGameReactionRecordAsync(GameReactionRecord);
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            if (_isGameRunning)
            {
                var result = await DisplayAlert("⚠️ Przerwać test?", 
                    "Test jest w toku. Czy na pewno chcesz wyjść?", 
                    "Tak", "Nie");
                
                if (!result) return;
            }
            
            StopGame();
            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopGame();
        }
    }
}
