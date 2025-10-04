using System.Diagnostics;

namespace NeuroMate.Views;

public partial class StroopGamePage : ContentPage
{
    private readonly Random _random = new();
    private readonly List<string> _colorNames = new() { "CZERWONY", "NIEBIESKI", "ZIELONY", "ŻÓŁTY" };
    private readonly List<Color> _colors = new() 
    { 
        Color.FromArgb("#F44336"), // Czerwony
        Color.FromArgb("#2196F3"), // Niebieski
        Color.FromArgb("#4CAF50"), // Zielony
        Color.FromArgb("#FFC107")  // Żółty
    };
    
    private string _currentWord = "";
    private Color _currentColor = Colors.Black;
    private Stopwatch _reactionTimer = new();
    private Timer? _gameTimer;
    private int _currentTrial = 0;
    private int _totalTrials = 30;
    private int _correctAnswers = 0;
    private List<int> _reactionTimes = new();
    private bool _isGameRunning = false;
    private bool _isPaused = false;
    private int _timeLeft = 60;

    public StroopGamePage()
    {
        InitializeComponent();
        
        // Inicjalizuj pierwszy stimulus
        ShowNextStimulus();
    }

    private void OnStartStopClicked(object sender, EventArgs e)
    {
        if (!_isGameRunning)
        {
            StartGame();
        }
        else
        {
            StopGame();
        }
    }

    private void StartGame()
    {
        _isGameRunning = true;
        _isPaused = false;
        _currentTrial = 0;
        _correctAnswers = 0;
        _reactionTimes.Clear();
        _timeLeft = 60;

        StartStopButton.Text = "⏹️ Stop";
        
        // Bezpieczne ustawienie stylu
        if (Application.Current?.Resources?.TryGetValue("SecondaryButton", out var secondaryStyle) == true)
        {
            StartStopButton.Style = (Style)secondaryStyle;
        }

        StartTimer();
        ShowNextStimulus();
        UpdateAvatarMood("focused");
        UpdateProgress();
    }

    private void StopGame()
    {
        _isGameRunning = false;
        _gameTimer?.Dispose();
        
        StartStopButton.Text = "🚀 Start";
        
        // Bezpieczne ustawienie stylu
        if (Application.Current?.Resources?.TryGetValue("PrimaryButton", out var primaryStyle) == true)
        {
            StartStopButton.Style = (Style)primaryStyle;
        }
        
        ShowResults();
    }

    private void OnPauseClicked(object sender, EventArgs e)
    {
        if (!_isGameRunning) return;

        _isPaused = !_isPaused;
        
        if (_isPaused)
        {
            _gameTimer?.Dispose();
            PauseButton.Text = "▶️ Wznów";
            UpdateAvatarMood("thinking");
        }
        else
        {
            StartTimer();
            PauseButton.Text = "⏸️ Pauza";
            UpdateAvatarMood("focused");
        }
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Zakończ grę", "Czy na pewno chcesz wyjść?", "Tak", "Nie");
        if (result)
        {
            _gameTimer?.Dispose();
            await Navigation.PopAsync();
        }
    }

    private void OnColorButtonClicked(object sender, EventArgs e)
    {
        if (!_isGameRunning || _isPaused) return;

        var button = sender as Button;
        if (button == null) return;

        _reactionTimer.Stop();
        var reactionTime = (int)_reactionTimer.ElapsedMilliseconds;
        _reactionTimes.Add(reactionTime);

        // Sprawdź odpowiedź na podstawie BackgroundColor przycisku
        bool isCorrect = IsCorrectAnswer(button.BackgroundColor);
        
        if (isCorrect)
        {
            _correctAnswers++;
            ShowFeedback("✅ Poprawnie!", Colors.Green);
            UpdateAvatarMood("celebrating");
        }
        else
        {
            ShowFeedback("❌ Błędnie!", Colors.Red);
            UpdateAvatarMood("concerned");
        }

        _currentTrial++;
        UpdateProgress();
        UpdateStats();

        if (_currentTrial >= _totalTrials || _timeLeft <= 0)
        {
            StopGame();
            return;
        }

        // Następny stimulus po krótkiej przerwie
        Task.Delay(800).ContinueWith(_ => 
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                if (_isGameRunning && !_isPaused)
                {
                    ShowNextStimulus();
                    UpdateAvatarMood("focused");
                }
            });
        });
    }

    private void ShowNextStimulus()
    {
        // Losuj słowo i kolor (często niezgodne dla efektu Stroop)
        var wordIndex = _random.Next(_colorNames.Count);
        var colorIndex = _random.Next(_colors.Count);
        
        // 70% szans na niezgodność słowa i koloru (efekt Stroop)
        if (_random.NextDouble() < 0.7)
        {
            while (colorIndex == wordIndex)
            {
                colorIndex = _random.Next(_colors.Count);
            }
        }

        _currentWord = _colorNames[wordIndex];
        _currentColor = _colors[colorIndex];

        // Aktualizuj UI
        WordLabel.Text = _currentWord;
        WordLabel.TextColor = _currentColor;
        
        if (_isGameRunning)
        {
            _reactionTimer.Restart();
        }
    }

    private bool IsCorrectAnswer(Color buttonColor)
    {
        // Poprawna odpowiedź to kolor tekstu, nie znaczenie słowa
        return ColorsAreEqual(buttonColor, _currentColor);
    }

    private bool ColorsAreEqual(Color color1, Color color2)
    {
        return Math.Abs(color1.Red - color2.Red) < 0.01 &&
               Math.Abs(color1.Green - color2.Green) < 0.01 &&
               Math.Abs(color1.Blue - color2.Blue) < 0.01;
    }

    private void UpdateProgress()
    {
        ProgressLabel.Text = $"{_currentTrial}/{_totalTrials}";
        GameProgressBar.Progress = (double)_currentTrial / _totalTrials;
    }

    private void UpdateStats()
    {
        ScoreLabel.Text = _correctAnswers.ToString();
        
        if (_reactionTimes.Count > 0)
        {
            var avgRT = (int)_reactionTimes.Average();
            AvgRTLabel.Text = $"{avgRT}ms";
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        InstructionLabel.Text = message;
        InstructionLabel.TextColor = color;
        
        Task.Delay(600).ContinueWith(_ => 
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                InstructionLabel.Text = "Kliknij kolor słowa, nie jego znaczenie!";
                InstructionLabel.TextColor = Color.FromArgb("#718096");
            });
        });
    }

    private void StartTimer()
    {
        _gameTimer = new Timer(_ => 
        {
            _timeLeft--;
            MainThread.BeginInvokeOnMainThread(() => 
            {
                TimerLabel.Text = $"{_timeLeft}s";
                
                if (_timeLeft <= 0)
                {
                    StopGame();
                }
                else if (_timeLeft <= 10)
                {
                    TimerLabel.TextColor = Colors.Red;
                    UpdateAvatarMood("concerned");
                }
            });
        }, null, 1000, 1000);
    }

    private async void ShowResults()
    {
        UpdateAvatarMood("celebrating");
        
        var accuracy = _currentTrial > 0 ? (double)_correctAnswers / _currentTrial * 100 : 0;
        var avgRT = _reactionTimes.Count > 0 ? (int)_reactionTimes.Average() : 0;
        
        var message = $"🎉 Świetnie!\n\n" +
                     $"Poprawne odpowiedzi: {_correctAnswers}/{_currentTrial}\n" +
                     $"Dokładność: {accuracy:F1}%\n" +
                     $"Średni czas reakcji: {avgRT}ms\n\n";

        if (accuracy >= 90)
        {
            message += "🏆 Doskonała koncentracja!";
        }
        else if (accuracy >= 75)
        {
            message += "💪 Bardzo dobry wynik!";
        }
        else if (accuracy >= 60)
        {
            message += "👍 Dobry wynik!";
        }
        else
        {
            message += "💡 Trenuj częściej!";
        }

        await DisplayAlert("Wyniki Test Stroop", message, "OK");
        await Navigation.PopAsync();
    }

    private void UpdateAvatarMood(string mood)
    {
        try
        {
            string animationSource = mood switch
            {
                "happy" => "avatar_happy.json",
                "excited" => "avatar_excited.json", 
                "focused" => "avatar_focused.json",
                "thinking" => "avatar_thinking.json",
                "celebrating" => "avatar_celebrating.json",
                "concerned" => "avatar_concerned.json",
                _ => "avatar_focused.json"
            };

            GameAvatarLottie.Source = new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource
            {
                File = animationSource
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating avatar: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _gameTimer?.Dispose();
        _reactionTimer?.Stop();
    }
}
