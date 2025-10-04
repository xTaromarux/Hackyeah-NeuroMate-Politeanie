using System.Diagnostics;

namespace NeuroMate.Views;

public partial class TaskSwitchingGamePage : ContentPage
{
    private readonly Random _random = new();
    private readonly List<string> _shapes = new() { "●", "■", "▲", "♦" };
    private readonly List<Color> _colors = new() 
    { 
        Color.FromArgb("#F44336"), // Czerwony
        Color.FromArgb("#2196F3"), // Niebieski
        Color.FromArgb("#4CAF50"), // Zielony
        Color.FromArgb("#FFC107")  // Żółty
    };
    
    private string _currentRule = "KOLOR"; // KOLOR lub KSZTAŁT
    private string _currentShape = "";
    private Color _currentColor = Colors.Black;
    private Stopwatch _reactionTimer = new();
    private Timer? _gameTimer;
    private Timer? _stimulusTimer;
    private int _currentTask = 0;
    private int _totalTasks = 20;
    private int _correctAnswers = 0;
    private List<int> _reactionTimes = new();
    private List<int> _switchCosts = new();
    private bool _isGameRunning = false;
    private bool _isPaused = false;
    private int _timeLeft = 45;
    private bool _wasSwitch = false;
    private int _lastNonSwitchRT = 0;

    public TaskSwitchingGamePage()
    {
        InitializeComponent();
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
        _currentTask = 0;
        _correctAnswers = 0;
        _reactionTimes.Clear();
        _switchCosts.Clear();
        _timeLeft = 45;

        StartStopButton.Text = "⏹️ Zatrzymaj";
        
        // Bezpieczne ustawienie stylu
        if (Application.Current?.Resources?.TryGetValue("OutlineButton", out var outlineStyle) == true)
        {
            StartStopButton.Style = (Style)outlineStyle;
        }
        
        EnableResponseButtons(true);
        StartTimer();
        StartTaskSequence();
        
        UpdateAvatarMood("focused");
    }

    private void StopGame()
    {
        _isGameRunning = false;
        _gameTimer?.Dispose();
        _stimulusTimer?.Dispose();
        
        StartStopButton.Text = "🔄 Rozpocznij Switching";
        
        // Bezpieczne ustawienie stylu
        if (Application.Current?.Resources?.TryGetValue("PrimaryButton", out var primaryStyle) == true)
        {
            StartStopButton.Style = (Style)primaryStyle;
        }
        
        EnableResponseButtons(false);
        ShowResults();
    }

    private void OnPauseClicked(object sender, EventArgs e)
    {
        if (!_isGameRunning) return;

        _isPaused = !_isPaused;
        
        if (_isPaused)
        {
            _gameTimer?.Dispose();
            _stimulusTimer?.Dispose();
            PauseButton.Text = "▶️ Wznów";
            EnableResponseButtons(false);
            UpdateAvatarMood("thinking");
        }
        else
        {
            StartTimer();
            StartTaskSequence();
            PauseButton.Text = "⏸️ Pauza";
            EnableResponseButtons(true);
            UpdateAvatarMood("focused");
        }
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Zakończ grę", "Czy na pewno chcesz wyjść?", "Tak", "Nie");
        if (result)
        {
            _gameTimer?.Dispose();
            _stimulusTimer?.Dispose();
            await Navigation.PopAsync();
        }
    }

    private void OnResponseButtonClicked(object sender, EventArgs e)
    {
        if (!_isGameRunning || _isPaused) return;

        var button = sender as Button;
        var response = button?.Text?.Contains("LEWO") == true ? "LEWO" :
                      button?.Text?.Contains("PRAWO") == true ? "PRAWO" :
                      button?.Text?.Contains("GÓRA") == true ? "GÓRA" : "DÓŁ";
        
        _reactionTimer.Stop();
        var reactionTime = (int)_reactionTimer.ElapsedMilliseconds;
        _reactionTimes.Add(reactionTime);

        bool isCorrect = CheckAnswer(response);
        
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

        // Oblicz switch cost
        if (_wasSwitch && _lastNonSwitchRT > 0)
        {
            var switchCost = reactionTime - _lastNonSwitchRT;
            _switchCosts.Add(Math.Max(0, switchCost));
        }
        else if (!_wasSwitch)
        {
            _lastNonSwitchRT = reactionTime;
        }

        _currentTask++;
        UpdateProgress();
        UpdateStats();

        if (_currentTask >= _totalTasks || _timeLeft <= 0)
        {
            StopGame();
            return;
        }

        // Następne zadanie po krótkiej przerwie
        Task.Delay(800).ContinueWith(_ => 
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                if (_isGameRunning && !_isPaused)
                {
                    PrepareNextTask();
                    UpdateAvatarMood("focused");
                }
            });
        });
    }

    private void OnDirectionButtonClicked(object sender, EventArgs e)
    {
        OnResponseButtonClicked(sender, e);
    }

    private void StartTaskSequence()
    {
        _stimulusTimer = new Timer(_ => 
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                if (!_isGameRunning || _isPaused) return;
                PrepareNextTask();
            });
        }, null, 1000, 0); // Pierwsze zadanie po 1s
    }

    private void PrepareNextTask()
    {
        // Losowo zmień regułę (30% szans na switch)
        var previousRule = _currentRule;
        if (_random.NextDouble() < 0.3)
        {
            _currentRule = _currentRule == "KOLOR" ? "KSZTAŁT" : "KOLOR";
        }
        _wasSwitch = _currentRule != previousRule;

        // Wygeneruj nowy bodziec
        _currentShape = _shapes[_random.Next(_shapes.Count)];
        _currentColor = _colors[_random.Next(_colors.Count)];

        // Aktualizuj UI
        CurrentRuleLabel.Text = _currentRule;
        CurrentRuleLabel.TextColor = _currentRule == "KOLOR" ? 
            Color.FromArgb("#2196F3") : Color.FromArgb("#FF9800");
        
        RuleFrame.BackgroundColor = _currentRule == "KOLOR" ? 
            Color.FromArgb("#BBDEFB") : Color.FromArgb("#FFE0B2");

        StimulusLabel.Text = _currentShape;
        StimulusLabel.TextColor = _currentColor;
        
        _reactionTimer.Restart();
    }

    private bool CheckAnswer(string response)
    {
        if (_currentRule == "KOLOR")
        {
            // Odpowiadaj na podstawie koloru
            return (_currentColor.ToArgbHex() == "#FFF44336" && response == "LEWO") ||  // Czerwony -> Lewo
                   (_currentColor.ToArgbHex() == "#FF2196F3" && response == "PRAWO") || // Niebieski -> Prawo
                   (_currentColor.ToArgbHex() == "#FF4CAF50" && response == "GÓRA") ||  // Zielony -> Góra
                   (_currentColor.ToArgbHex() == "#FFFFC107" && response == "DÓŁ");    // Żółty -> Dół
        }
        else
        {
            // Odpowiadaj na podstawie kształtu
            return (_currentShape == "●" && response == "LEWO") ||   // Koło -> Lewo
                   (_currentShape == "■" && response == "PRAWO") ||  // Kwadrat -> Prawo
                   (_currentShape == "▲" && response == "GÓRA") ||   // Trójkąt -> Góra
                   (_currentShape == "♦" && response == "DÓŁ");     // Romb -> Dół
        }
    }

    private void UpdateProgress()
    {
        ProgressLabel.Text = $"Zadanie {_currentTask + 1}/{_totalTasks}";
        GameProgressBar.Progress = (double)(_currentTask + 1) / _totalTasks;
    }

    private void UpdateStats()
    {
        ScoreLabel.Text = _correctAnswers.ToString();
        
        if (_reactionTimes.Count > 0)
        {
            var avgRT = (int)_reactionTimes.Average();
            AvgRTLabel.Text = $"{avgRT}ms";
        }

        if (_switchCosts.Count > 0)
        {
            var avgSwitchCost = (int)_switchCosts.Average();
            SwitchCostLabel.Text = $"{avgSwitchCost}ms";
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
                InstructionLabel.Text = "Przełączaj między regułami: kolor vs kształt";
                InstructionLabel.TextColor = Color.FromArgb("#757575");
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

    private void EnableResponseButtons(bool enabled)
    {
        LeftButton.IsEnabled = enabled;
        RightButton.IsEnabled = enabled;
        UpButton.IsEnabled = enabled;
        DownButton.IsEnabled = enabled;
        
        var opacity = enabled ? 1.0 : 0.5;
        LeftButton.Opacity = opacity;
        RightButton.Opacity = opacity;
        UpButton.Opacity = opacity;
        DownButton.Opacity = opacity;
    }

    private async void ShowResults()
    {
        UpdateAvatarMood("celebrating");
        
        var accuracy = _currentTask > 0 ? (double)_correctAnswers / _currentTask * 100 : 0;
        var avgRT = _reactionTimes.Count > 0 ? (int)_reactionTimes.Average() : 0;
        var avgSwitchCost = _switchCosts.Count > 0 ? (int)_switchCosts.Average() : 0;
        
        var message = $"🎉 Świetnie!\n\n" +
                     $"Poprawne odpowiedzi: {_correctAnswers}/{_currentTask}\n" +
                     $"Dokładność: {accuracy:F1}%\n" +
                     $"Średni czas reakcji: {avgRT}ms\n" +
                     $"Switch Cost: {avgSwitchCost}ms\n\n";

        if (avgSwitchCost < 100)
        {
            message += "🏆 Doskonała elastyczność poznawcza!";
        }
        else if (avgSwitchCost < 200)
        {
            message += "💪 Bardzo dobra elastyczność!";
        }
        else if (avgSwitchCost < 300)
        {
            message += "👍 Dobry wynik!";
        }
        else
        {
            message += "💡 Trenuj częściej!";
        }

        await DisplayAlert("Wyniki Task Switching", message, "OK");
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

            TaskAvatarLottie.Source = new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource
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
        _stimulusTimer?.Dispose();
        _reactionTimer?.Stop();
    }
}
