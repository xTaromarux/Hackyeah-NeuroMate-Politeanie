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
        private const int _nBackLevel = 1; // Stały poziom 1-back
        
        // Zmienne dla gry z kształtami
        private List<ShapeType> _shapeHistory = new();
        private List<ShapeType> _userResponses = new();
        private List<ShapeType> _correctAnswers = new();
        private readonly string[] _shapes = { "●", "■", "▲", "♦" }; // Koło, Kwadrat, Trójkąt, Romb
        private readonly ShapeType[] _shapeTypes = { ShapeType.Circle, ShapeType.Square, ShapeType.Triangle, ShapeType.Diamond };
        
        private bool _waitingForResponse = false;
        private ShapeType _currentCorrectAnswer = ShapeType.None;
        
        // Oryginalne kolory przycisków
        private Color _originalButtonColor;
        
        public NBackGamePage()
        {
            InitializeComponent();
            InitializeColors();
            ResetGameStats();
            ShowInitialShape();
        }

        private void InitializeColors()
        {
            // Pobierz oryginalny kolor z zasobów aplikacji
            if (Application.Current?.Resources.TryGetValue("CardBackground", out var cardBgResource) == true)
            {
                _originalButtonColor = (Color)cardBgResource;
            }
            else
            {
                _originalButtonColor = Colors.White; // Fallback
            }
        }

        private void ShowInitialShape()
        {
            // Pokaż kwadrat jako początkowy kształt
            CurrentShapeDisplay.Text = "■";
            CurrentShapeDisplay.IsVisible = true;
            WaitingLabel.Text = "To jest kwadrat - kliknij Start aby rozpocząć";
        }

        private void ResetGameStats()
        {
            _currentTrial = 0;
            _shapeHistory.Clear();
            _userResponses.Clear();
            _correctAnswers.Clear();
            _waitingForResponse = false;
            _currentCorrectAnswer = ShapeType.None;
            
            UpdateStats();
            DisableShapeButtons();
            ResetButtonColors(); // Resetuj kolory przycisków
        }

        private void ResetButtonColors()
        {
            CircleButton.BackgroundColor = _originalButtonColor;
            SquareButton.BackgroundColor = _originalButtonColor;
            TriangleButton.BackgroundColor = _originalButtonColor;
            DiamondButton.BackgroundColor = _originalButtonColor;
        }

        private void UpdateStats()
        {
            TrialsLabel.Text = $"{_currentTrial}/{_totalTrials}";
            
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
            InstructionLabel.Text = "Test rozpoczęty! Wybierz kształt z poprzedniego kroku.";
            WaitingLabel.IsVisible = false;
            
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
                
                if (_isGameRunning && !_isPaused)
                {
                    await Task.Delay(400); // Krótka przerwa między bodźcami
                }
            }
        }

        private async Task PresentStimulus()
        {
            if (!_isGameRunning || _isPaused) return;

            _currentTrial++;
            
            // Wylosuj kształt (0-3)
            var shapeIndex = _random.Next(0, 4);
            var currentShape = _shapeTypes[shapeIndex];
            _shapeHistory.Add(currentShape);
            
            // Sprawdź jaka jest poprawna odpowiedź (kształt 1 krok wstecz)
            ShapeType correctAnswer = ShapeType.None;
            if (_shapeHistory.Count > _nBackLevel)
            {
                correctAnswer = _shapeHistory[_shapeHistory.Count - 1 - _nBackLevel];
            }
            _correctAnswers.Add(correctAnswer);
            _currentCorrectAnswer = correctAnswer;
            
            // Pokaż bodziec
            ShowShape(currentShape);
            
            // Włącz przyciski wyboru
            EnableShapeButtons();
            _waitingForResponse = true;
            
            // Czekaj na odpowiedź użytkownika (3 sekundy)
            var responseTask = Task.Delay(3000);
            await responseTask;
            
            // Jeśli użytkownik nie odpowiedział, uznaj jako brak odpowiedzi
            if (_waitingForResponse)
            {
                _userResponses.Add(ShapeType.None);
                _waitingForResponse = false;
            }
            
            // Wyłącz przyciski i ukryj kształt
            DisableShapeButtons();
            HideCurrentShape();
            
            UpdateStats();
            
            // Krótka przerwa przed następnym bodźcem
            if (_isGameRunning && !_isPaused)
            {
                await Task.Delay(1000);
            }
        }

        private void ShowShape(ShapeType shape)
        {
            var shapeIndex = (int)shape;
            CurrentShapeDisplay.Text = _shapes[shapeIndex];
            CurrentShapeDisplay.IsVisible = true;
        }

        private void HideCurrentShape()
        {
            CurrentShapeDisplay.IsVisible = false;
        }

        private void EnableShapeButtons()
        {
            ResetButtonColors(); // Resetuj kolory przed włączeniem
            CircleButton.IsEnabled = true;
            SquareButton.IsEnabled = true;
            TriangleButton.IsEnabled = true;
            DiamondButton.IsEnabled = true;
        }

        private void DisableShapeButtons()
        {
            CircleButton.IsEnabled = false;
            SquareButton.IsEnabled = false;
            TriangleButton.IsEnabled = false;
            DiamondButton.IsEnabled = false;
        }

        private void OnShapeSelected(object sender, EventArgs e)
        {
            if (!_waitingForResponse) return;

            var button = sender as Button;
            ShapeType selectedShape = ShapeType.None;

            // Określ wybrany kształt na podstawie przycisku
            if (button == CircleButton) selectedShape = ShapeType.Circle;
            else if (button == SquareButton) selectedShape = ShapeType.Square;
            else if (button == TriangleButton) selectedShape = ShapeType.Triangle;
            else if (button == DiamondButton) selectedShape = ShapeType.Diamond;

            _userResponses.Add(selectedShape);
            _waitingForResponse = false;

            // Feedback wizualny
            ShowButtonFeedback(button, selectedShape == _currentCorrectAnswer);
        }

        private async void ShowButtonFeedback(Button button, bool isCorrect)
        {
            // Użyj zapisanego oryginalnego koloru
            button.BackgroundColor = isCorrect ? Colors.LightGreen : Colors.LightCoral;
            
            await Task.Delay(400);
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (button != null)
                {
                    button.BackgroundColor = _originalButtonColor;
                }
            });
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
            InstructionLabel.Text = "Test wstrzymany";
            HideCurrentShape();
            DisableShapeButtons();
            _waitingForResponse = false;
        }

        private async void ResumeGame()
        {
            _isPaused = false;
            PauseButton.Text = "⏸️ Pauza";
            InstructionLabel.Text = "Test wznowiony! Wybierz kształt z poprzedniego kroku.";
            
            await StartStimulusSequence();
        }

        private void StopGame()
        {
            _isGameRunning = false;
            _gameTimer?.Dispose();
            
            StartStopButton.Text = "🚀 Start";
            PauseButton.IsVisible = false;
            InstructionLabel.Text = "Kliknij Start aby rozpocząć test pamięci";
            ShowInitialShape(); // Pokaż ponownie początkowy kształt
            
            DisableShapeButtons();
            ResetButtonColors(); // Resetuj kolory przycisków
            _waitingForResponse = false;
        }

        private async void EndGame()
        {
            StopGame();
            
            // Oblicz wyniki końcowe
            var correct = 0;
            var answered = 0;
            for (int i = 0; i < Math.Min(_userResponses.Count, _correctAnswers.Count); i++)
            {
                if (_correctAnswers[i] != ShapeType.None) // Tylko próby gdzie była możliwa poprawna odpowiedź
                {
                    answered++;
                    if (_userResponses[i] == _correctAnswers[i]) correct++;
                }
            }
            
            var accuracy = answered > 0 ? (double)correct / answered * 100 : 0;
            var totalValidTrials = _correctAnswers.Count(x => x != ShapeType.None);
            
            await DisplayAlert("🧠 Test zakończony!", 
                $"Wykonałeś {_currentTrial} prób\n" +
                $"Próby wymagające odpowiedzi: {totalValidTrials}\n" +
                $"Dokładność: {accuracy:F1}%\n" +
                $"Poprawnych odpowiedzi: {correct}/{answered}\n\n" +
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

    // Enum dla typów kształtów
    public enum ShapeType
    {
        None = -1,
        Circle = 0,
        Square = 1,
        Triangle = 2,
        Diamond = 3
    }
}
