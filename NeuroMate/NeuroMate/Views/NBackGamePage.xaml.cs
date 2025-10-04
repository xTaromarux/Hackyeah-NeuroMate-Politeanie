namespace NeuroMate.Views
{
    public partial class NBackGamePage : ContentPage
    {
        private readonly Random _random = new();
        private Timer? _gameTimer;
        
        private bool _isGameRunning = false;
        private bool _isPaused = false;
        private int _currentTrial = 0;
        private int _totalTrials = 30;
        private int _gameTimeLeft = 120; // 2 minuty
        private int _nBackLevel = 1; // 1-back lub 2-back
        
        private List<int> _stimulusHistory = new();
        private List<bool> _userResponses = new();
        private List<bool> _correctAnswers = new();
        private Frame[] _gamePositions = new Frame[9];
        
        public NBackGamePage()
        {
            InitializeComponent();
            InitializeGameBoard();
            ResetGameStats();
        }

        private void InitializeGameBoard()
        {
            // Mapuj pozycje na planszy do tablicy
            _gamePositions[0] = Pos00;
            _gamePositions[1] = Pos01;
            _gamePositions[2] = Pos02;
            _gamePositions[3] = Pos10;
            _gamePositions[4] = Pos11;
            _gamePositions[5] = Pos12;
            _gamePositions[6] = Pos20;
            _gamePositions[7] = Pos21;
            _gamePositions[8] = Pos22;
        }

        private void ResetGameStats()
        {
            _currentTrial = 0;
            _stimulusHistory.Clear();
            _userResponses.Clear();
            _correctAnswers.Clear();
            UpdateStats();
            HideAllStimuli();
        }

        private void UpdateStats()
        {
            TrialsLabel.Text = $"{_currentTrial}/{_totalTrials}";
            LevelLabel.Text = $"{_nBackLevel}-back";
            
            if (_userResponses.Count > 0)
            {
                var correct = 0;
                for (int i = 0; i < Math.Min(_userResponses.Count, _correctAnswers.Count); i++)
                {
                    if (_userResponses[i] == _correctAnswers[i]) correct++;
                }
                var accuracy = (double)correct / _userResponses.Count * 100;
                AccuracyLabel.Text = $"{accuracy:F0}%";
            }
            else
            {
                AccuracyLabel.Text = "0%";
            }
        }

        private void OnLevelSelected(object sender, EventArgs e)
        {
            // Reset stylów przycisków
            OneBackBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            TwoBackBtn.Style = (Style)Application.Current.Resources["OutlineButton"];

            var button = sender as Button;
            button.Style = (Style)Application.Current.Resources["PrimaryButton"];

            _nBackLevel = button.Text.Contains("1") ? 1 : 2;
            
            // Resetuj grę przy zmianie poziomu
            if (_isGameRunning)
            {
                StopGame();
            }
            ResetGameStats();
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
            _gameTimeLeft = 120;
            ResetGameStats();
            
            StartStopButton.Text = "⏹️ Stop";
            PauseButton.IsVisible = true;
            InstructionLabel.Text = $"Gra {_nBackLevel}-back rozpoczęta!";
            
            // Wyłącz przyciski poziomu podczas gry
            OneBackBtn.IsEnabled = false;
            TwoBackBtn.IsEnabled = false;
            
            // Timer gry (odliczanie)
            _gameTimer = new Timer(GameTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            
            // Zacznij pierwszą sekwencję
            await StartStimulusSequence();
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

        private async Task StartStimulusSequence()
        {
            while (_isGameRunning && !_isPaused && _currentTrial < _totalTrials)
            {
                await PresentStimulus();
                await Task.Delay(500); // Czas na odpowiedź
                
                if (_isGameRunning && !_isPaused)
                {
                    await Task.Delay(1500); // Przerwa między bodźcami
                }
            }
        }

        private async Task PresentStimulus()
        {
            if (!_isGameRunning || _isPaused) return;

            _currentTrial++;
            
            // Wylosuj pozycję (0-8)
            var position = _random.Next(0, 9);
            _stimulusHistory.Add(position);
            
            // Sprawdź czy to match (pozycja n kroków wstecz)
            bool isMatch = false;
            if (_stimulusHistory.Count > _nBackLevel)
            {
                var nBackPosition = _stimulusHistory[_stimulusHistory.Count - 1 - _nBackLevel];
                isMatch = position == nBackPosition;
            }
            _correctAnswers.Add(isMatch);
            
            // Pokaż bodziec
            ShowStimulusAt(position);
            
            // Czekaj na odpowiedź użytkownika (2 sekundy)
            var responseReceived = false;
            var responseTask = Task.Delay(2000);
            
            // Reset user response flag
            _waitingForResponse = true;
            _lastResponse = false;
            
            await responseTask;
            
            _waitingForResponse = false;
            
            // Jeśli użytkownik nie odpowiedział, uznaj jako "No Match"
            if (!responseReceived)
            {
                _userResponses.Add(false);
            }
            
            // Ukryj bodziec
            HideAllStimuli();
            
            UpdateStats();
        }

        private bool _waitingForResponse = false;
        private bool _lastResponse = false;

        private void ShowStimulusAt(int position)
        {
            HideAllStimuli();
            
            // Ustaw bodziec na wybranej pozycji
            CurrentStimulus.IsVisible = true;
            Grid.SetRow(CurrentStimulus, position / 3);
            Grid.SetColumn(CurrentStimulus, position % 3);
        }

        private void HideAllStimuli()
        {
            CurrentStimulus.IsVisible = false;
        }

        private void OnMatchClicked(object sender, EventArgs e)
        {
            if (_waitingForResponse)
            {
                _userResponses.Add(true);
                _waitingForResponse = false;
                
                // Feedback wizualny
                MatchButton.BackgroundColor = Color.FromArgb("#48bb78");
                Task.Delay(200).ContinueWith(_ => 
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MatchButton.BackgroundColor = Color.FromArgb("#667eea");
                    });
                });
            }
        }

        private void OnNoMatchClicked(object sender, EventArgs e)
        {
            if (_waitingForResponse)
            {
                _userResponses.Add(false);
                _waitingForResponse = false;
                
                // Feedback wizualny
                NoMatchButton.BackgroundColor = Color.FromArgb("#48bb78");
                Task.Delay(200).ContinueWith(_ => 
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        NoMatchButton.BackgroundColor = Color.FromArgb("#f093fb");
                    });
                });
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
            HideAllStimuli();
        }

        private async void ResumeGame()
        {
            _isPaused = false;
            PauseButton.Text = "⏸️ Pauza";
            InstructionLabel.Text = $"Gra {_nBackLevel}-back wznowiona";
            
            await StartStimulusSequence();
        }

        private void StopGame()
        {
            _isGameRunning = false;
            _gameTimer?.Dispose();
            
            StartStopButton.Text = "🚀 Start";
            PauseButton.IsVisible = false;
            InstructionLabel.Text = "Kliknij Start aby rozpocząć test pamięci";
            
            // Włącz z powrotem przyciski poziomu
            OneBackBtn.IsEnabled = true;
            TwoBackBtn.IsEnabled = true;
            
            HideAllStimuli();
        }

        private async void EndGame()
        {
            StopGame();
            
            // Oblicz wyniki końcowe
            var correct = 0;
            for (int i = 0; i < Math.Min(_userResponses.Count, _correctAnswers.Count); i++)
            {
                if (_userResponses[i] == _correctAnswers[i]) correct++;
            }
            var accuracy = _userResponses.Count > 0 ? (double)correct / _userResponses.Count * 100 : 0;
            
            await DisplayAlert("🧠 Test zakończony!", 
                $"Poziom: {_nBackLevel}-back\n" +
                $"Wykonałeś {_currentTrial} prób\n" +
                $"Dokładność: {accuracy:F1}%\n" +
                $"Poprawnych odpowiedzi: {correct}/{_userResponses.Count}\n\n" +
                $"Wynik zostanie zapisany w Twoim profilu!", 
                "OK");
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
