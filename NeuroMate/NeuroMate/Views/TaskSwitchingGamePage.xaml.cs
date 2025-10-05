using System.Diagnostics;

namespace NeuroMate.Views;

public partial class TaskSwitchingGamePage : ContentPage
{
    private readonly Random _random = new();
    
    // Dostępne monety w groszach (1gr, 2gr, 5gr, 10gr, 20gr, 50gr, 1zł, 2zł)
    private readonly List<int> _availableCoins = new() { 1, 2, 5, 10, 20, 50, 100, 200 };
    private readonly Dictionary<int, string> _coinNames = new()
    {
        { 1, "1gr" }, { 2, "2gr" }, { 5, "5gr" }, { 10, "10gr" },
        { 20, "20gr" }, { 50, "50gr" }, { 100, "1zł" }, { 200, "2zł" }
    };
    
    private int _targetAmount = 0; // Kwota do wydania w groszach
    private List<int> _selectedCoins = new();
    private Timer? _gameTimer;
    private Timer? _taskTimer; // Timer dla pojedynczego zadania (5 sekund)
    private Stopwatch _responseTimer = new();
    
    private int _currentTask = 0;
    private int _totalTasks = 15;
    private int _correctAnswers = 0;
    private List<int> _coinsUsedPerTask = new();
    private List<int> _responseTimes = new();
    private bool _isGameRunning = false;
    private bool _isPaused = false;
    private int _gameTimeLeft = 60;
    private int _taskTimeLeft = 5; // 5 sekund na zadanie
    private const int TASK_TIME_LIMIT = 5; // 5 sekund na zadanie

    public TaskSwitchingGamePage()
    {
        InitializeComponent();
        ResetGame();
    }

    private void ResetGame()
    {
        _currentTask = 0;
        _correctAnswers = 0;
        _coinsUsedPerTask.Clear();
        _responseTimes.Clear();
        _selectedCoins.Clear();
        _gameTimeLeft = 60;
        _taskTimeLeft = TASK_TIME_LIMIT;
        
        UpdateUI();
        GenerateNewTask();
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
        _responseTimer.Start();

        StartStopButton.Text = "⏹️ Zatrzymaj";
        PauseButton.IsVisible = true;
        
        EnableCoinButtons(true);
        StartGameTimer();
        StartTaskTimer();
        
        UpdateAvatarMood("focused");
    }

    private void StopGame()
    {
        _isGameRunning = false;
        _gameTimer?.Dispose();
        _taskTimer?.Dispose();
        _responseTimer.Stop();
        
        StartStopButton.Text = "🚀 Rozpocznij Kasjer";
        PauseButton.IsVisible = false;
        
        EnableCoinButtons(false);
        ShowResults();
    }

    private void OnPauseClicked(object sender, EventArgs e)
    {
        if (!_isGameRunning) return;

        _isPaused = !_isPaused;
        
        if (_isPaused)
        {
            _gameTimer?.Dispose();
            _taskTimer?.Dispose();
            _responseTimer.Stop();
            PauseButton.Text = "▶️ Wznów";
            EnableCoinButtons(false);
            UpdateAvatarMood("thinking");
        }
        else
        {
            _responseTimer.Start();
            StartGameTimer();
            StartTaskTimer();
            PauseButton.Text = "⏸️ Pauza";
            EnableCoinButtons(true);
            UpdateAvatarMood("focused");
        }
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Zakończ grę", "Czy na pewno chcesz wyjść?", "Tak", "Nie");
        if (result)
        {
            _gameTimer?.Dispose();
            _taskTimer?.Dispose();
            await Navigation.PopAsync();
        }
    }

    private void OnCoinSelected(object sender, EventArgs e)
    {
        if (!_isGameRunning || _isPaused) return;

        var button = sender as Button;
        var coinValue = GetCoinValueFromButton(button);
        
        if (coinValue > 0)
        {
            _selectedCoins.Add(coinValue);
            UpdateSelectedCoinsDisplay();
            
            // Sprawdź czy można sprawdzić odpowiedź
            CheckAnswerButton.IsEnabled = _selectedCoins.Count > 0;
        }
    }

    private int GetCoinValueFromButton(Button button)
    {
        return button?.Text switch
        {
            "1gr" => 1,
            "2gr" => 2,
            "5gr" => 5,
            "10gr" => 10,
            "20gr" => 20,
            "50gr" => 50,
            "1zł" => 100,
            "2zł" => 200,
            _ => 0
        };
    }

    private void OnClearSelection(object sender, EventArgs e)
    {
        _selectedCoins.Clear();
        UpdateSelectedCoinsDisplay();
        CheckAnswerButton.IsEnabled = false;
    }

    private void OnCheckAnswer(object sender, EventArgs e)
    {
        if (!_isGameRunning || _isPaused) return;

        ProcessAnswer();
    }

    private void ProcessAnswer()
    {
        _responseTimer.Stop();
        _taskTimer?.Dispose(); // Zatrzymaj timer zadania
        
        var responseTime = (int)_responseTimer.ElapsedMilliseconds;
        _responseTimes.Add(responseTime);

        var currentSum = _selectedCoins.Sum();
        var isCorrect = currentSum == _targetAmount;
        var optimalCoins = CalculateOptimalSolution(_targetAmount);
        var coinsUsed = _selectedCoins.Count;
        
        _coinsUsedPerTask.Add(coinsUsed);

        if (isCorrect)
        {
            _correctAnswers++;
            
            if (coinsUsed == optimalCoins)
            {
                ShowFeedback("🎉 Doskonale! Optymalna liczba monet!", Colors.Green);
                UpdateAvatarMood("celebrating");
            }
            else
            {
                ShowFeedback($"✅ Poprawnie! (Optimum: {optimalCoins} monet)", Colors.Orange);
                UpdateAvatarMood("happy");
            }
        }
        else if (_selectedCoins.Count == 0)
        {
            // Timeout - brak odpowiedzi
            ShowFeedback("⏰ Czas minął! Brak odpowiedzi.", Colors.Red);
            UpdateAvatarMood("concerned");
        }
        else
        {
            ShowFeedback($"❌ Błąd! Potrzebujesz dokładnie {FormatAmount(_targetAmount)}", Colors.Red);
            UpdateAvatarMood("concerned");
        }

        _currentTask++;
        UpdateStats();

        if (_currentTask >= _totalTasks || _gameTimeLeft <= 0)
        {
            StopGame();
            return;
        }

        // Następne zadanie po krótkiej przerwie
        Task.Delay(1500).ContinueWith(_ => 
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                if (_isGameRunning && !_isPaused)
                {
                    StartNextTask();
                }
            });
        });
    }

    private void StartNextTask()
    {
        GenerateNewTask();
        OnClearSelection(null, null);
        _taskTimeLeft = TASK_TIME_LIMIT;
        _responseTimer.Restart();
        StartTaskTimer();
        UpdateAvatarMood("focused");
    }

    private int CalculateOptimalSolution(int amount)
    {
        // Algorytm zachłanny - zawsze optymalny dla systemu monetarnego EUR/PLN
        var coins = 0;
        var remaining = amount;
        
        foreach (var coin in _availableCoins.OrderByDescending(x => x))
        {
            coins += remaining / coin;
            remaining %= coin;
        }
        
        return coins;
    }

    private void GenerateNewTask()
    {
        // Generuj kwoty od 1gr do 5zł
        _targetAmount = _random.Next(1, 501); // 1gr do 5zł
        TargetAmountLabel.Text = FormatAmount(_targetAmount);
    }

    private string FormatAmount(int grosz)
    {
        if (grosz >= 100)
        {
            var zloty = grosz / 100;
            var grosze = grosz % 100;
            
            if (grosze == 0)
                return $"{zloty} zł";
            else
                return $"{zloty},{grosze:D2} zł";
        }
        else
        {
            return $"{grosz} gr";
        }
    }

    private void UpdateSelectedCoinsDisplay()
    {
        if (_selectedCoins.Count == 0)
        {
            SelectedCoinsLabel.Text = "Wybierz monety...";
        }
        else
        {
            var coinGroups = _selectedCoins.GroupBy(x => x)
                                          .OrderByDescending(g => g.Key)
                                          .Select(g => $"{g.Count()}x{_coinNames[g.Key]}")
                                          .ToList();
            
            SelectedCoinsLabel.Text = string.Join(", ", coinGroups);
        }
        
        // Usunięto wyświetlanie sumy - tylko liczba monet
        CoinsCountLabel.Text = _selectedCoins.Count.ToString();
    }

    private void UpdateStats()
    {
        ProgressLabel.Text = $"{_currentTask}/{_totalTasks}";
        ScoreLabel.Text = _correctAnswers.ToString();
        
        if (_coinsUsedPerTask.Count > 0)
        {
            var avgCoins = _coinsUsedPerTask.Average();
            AvgCoinsLabel.Text = $"{avgCoins:F1}";
        }
    }

    private void UpdateUI()
    {
        UpdateStats();
        UpdateSelectedCoinsDisplay();
    }

    private void ShowFeedback(string message, Color color)
    {
        InstructionLabel.Text = message;
        InstructionLabel.TextColor = color;
        
        Task.Delay(1200).ContinueWith(_ => 
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                InstructionLabel.Text = "Wydaj resztę używając najmniejszej liczby monet";
                InstructionLabel.TextColor = Color.FromArgb("#757575");
            });
        });
    }

    private void StartGameTimer()
    {
        _gameTimer = new Timer(_ => 
        {
            _gameTimeLeft--;
            MainThread.BeginInvokeOnMainThread(() => 
            {
                TimerLabel.Text = $"{_gameTimeLeft}s";
                
                if (_gameTimeLeft <= 0)
                {
                    StopGame();
                }
                else if (_gameTimeLeft <= 10)
                {
                    TimerLabel.TextColor = Colors.Red;
                    UpdateAvatarMood("concerned");
                }
            });
        }, null, 1000, 1000);
    }

    private void StartTaskTimer()
    {
        _taskTimeLeft = TASK_TIME_LIMIT;
        
        _taskTimer = new Timer(_ => 
        {
            _taskTimeLeft--;
            MainThread.BeginInvokeOnMainThread(() => 
            {
                // Zaktualizuj timer zadania w interfejsie (opcjonalnie)
                if (_taskTimeLeft <= 0)
                {
                    // Timeout - automatycznie przejdź do następnego zadania
                    if (_isGameRunning && !_isPaused)
                    {
                        ProcessAnswer(); // Przetwórz jako brak odpowiedzi
                    }
                }
                else if (_taskTimeLeft <= 2)
                {
                    // Ostrzeżenie - zmień kolor timera na czerwony
                    TimerLabel.TextColor = Colors.Orange;
                }
            });
        }, null, 1000, 1000);
    }

    private void EnableCoinButtons(bool enabled)
    {
        Coin1Button.IsEnabled = enabled;
        Coin2Button.IsEnabled = enabled;
        Coin5Button.IsEnabled = enabled;
        Coin10Button.IsEnabled = enabled;
        Coin20Button.IsEnabled = enabled;
        Coin50Button.IsEnabled = enabled;
        Coin100Button.IsEnabled = enabled;
        Coin200Button.IsEnabled = enabled;
        ClearButton.IsEnabled = enabled;
        CheckAnswerButton.IsEnabled = enabled && _selectedCoins.Count > 0;
        
        var opacity = enabled ? 1.0 : 0.5;
        Coin1Button.Opacity = opacity;
        Coin2Button.Opacity = opacity;
        Coin5Button.Opacity = opacity;
        Coin10Button.Opacity = opacity;
        Coin20Button.Opacity = opacity;
        Coin50Button.Opacity = opacity;
        Coin100Button.Opacity = opacity;
        Coin200Button.Opacity = opacity;
    }

    private async void ShowResults()
    {
        UpdateAvatarMood("celebrating");
        
        var accuracy = _currentTask > 0 ? (double)_correctAnswers / _currentTask * 100 : 0;
        var avgCoins = _coinsUsedPerTask.Count > 0 ? _coinsUsedPerTask.Average() : 0;
        var avgTime = _responseTimes.Count > 0 ? (int)_responseTimes.Average() : 0;
        
        // Oblicz efektywność (im mniej monet, tym lepiej)
        var totalOptimalCoins = 0;
        var totalUsedCoins = _coinsUsedPerTask.Sum();
        
        // Estymacja optymalnych monet dla wykonanych zadań
        for (int i = 0; i < _currentTask; i++)
        {
            // Przybliżona wartość optymalna (trudno odtworzyć dokładne kwoty)
            totalOptimalCoins += 2; // Średnio 2-3 monety na zadanie
        }
        
        var efficiency = totalOptimalCoins > 0 ? (double)totalOptimalCoins / totalUsedCoins * 100 : 0;
        
        var message = $"💰 Wyniki Gry Kasjer!\n\n" +
                     $"Poprawne odpowiedzi: {_correctAnswers}/{_currentTask}\n" +
                     $"Dokładność: {accuracy:F1}%\n" +
                     $"Średnia liczba monet: {avgCoins:F1}\n" +
                     $"Średni czas odpowiedzi: {avgTime}ms\n" +
                     $"Efektywność: {efficiency:F1}%\n\n";

        if (efficiency >= 90)
        {
            message += "🏆 Ekspert kasjera! Doskonała optymalizacja!";
        }
        else if (efficiency >= 75)
        {
            message += "💪 Bardzo dobry kasjer!";
        }
        else if (efficiency >= 60)
        {
            message += "👍 Dobry kasjer!";
        }
        else
        {
            message += "💡 Trenuj dalej! Pamiętaj o największych monetach!";
        }

        await DisplayAlert("Wyniki Gry Kasjer", message, "OK");
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

            CashierAvatarLottie.Source = new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource
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
        _taskTimer?.Dispose();
        _responseTimer?.Stop();
    }
}
