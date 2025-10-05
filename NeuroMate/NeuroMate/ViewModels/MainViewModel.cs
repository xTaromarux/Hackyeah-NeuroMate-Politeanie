using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeuroMate.Models;
using NeuroMate.Services;
using NeuroMate.Helpers;

namespace NeuroMate.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Services
        
        private readonly INeuroScoreService _neuroScoreService;
        private readonly IInterventionService _interventionService;
        private readonly IPVTGameService _pvtGameService;
        private readonly IDataImportService _dataImportService;
        private readonly IPointsService _pointsService;
        private readonly IAvatarService _avatarService;
        
        #endregion

        #region Observable Properties - Assistant View

        [ObservableProperty]
        private string _assistantHint = "Wybierz swój cel, aby rozpocząć personalizowaną sesję.";

        [ObservableProperty]
        private string _selectedGoal = string.Empty;

        #endregion

        #region Observable Properties - Dashboard View

        [ObservableProperty]
        private int _neuroScore = 72;

        [ObservableProperty]
        private int _minutesNoBreak = 85;

        [ObservableProperty]
        private int _hrv = 65;

        [ObservableProperty]
        private ObservableCollection<TrendPoint> _trendPoints = new();

        #endregion

        #region Observable Properties - Points & Avatar System

        [ObservableProperty]
        private int _totalPoints = 0;

        [ObservableProperty]
        private string _currentAvatarName = "Żółw Mistrz";

        [ObservableProperty]
        private string _currentAvatarLottie = "hackyeah_default.png"; // Zmieniam domyślną wartość na nowy PNG

        [ObservableProperty]
        private int _pointsEarnedToday = 0;

        #endregion

        #region Observable Properties - Game View

        [ObservableProperty]
        private string _pvtStateText = "Gotowy?";

        [ObservableProperty]
        private string _pvtButtonText = "Rozpocznij test";

        [ObservableProperty]
        private int _lastReactionMs;

        [ObservableProperty]
        private bool _hasLastReaction;

        [ObservableProperty]
        private int _bestReactionMs;

        [ObservableProperty]
        private int _avgReactionMs;

        [ObservableProperty]
        private int _trialsCount;

        [ObservableProperty]
        private bool _isPvtRunning;

        #endregion

        #region Constructor

        public MainViewModel()
        {
            // Inicjalizacja serwisów - tymczasowo bez DI
            _neuroScoreService = new NeuroScoreService();
            _interventionService = new InterventionService();
            _pvtGameService = new PvtGameService();
            _dataImportService = new DataImportService();
            _pointsService = new PointsService();
            _avatarService = new AvatarService(_pointsService);

            // Inicjalizacja danych
            InitializeTrendData();
            LoadUserData();
            _ = LoadPointsDataAsync(); // Async load without blocking

            // Subskrybuj zdarzenia z serwisów
            _pvtGameService.OnReactionRecorded += HandleReactionRecorded;
            _pvtGameService.OnGameStateChanged += HandleGameStateChanged;
        }

        #endregion

        #region Initialization

        private void InitializeTrendData()
        {
            TrendPoints = new ObservableCollection<TrendPoint>
            {
                new TrendPoint { Day = "Pon", Score = 68 },
                new TrendPoint { Day = "Wt", Score = 72 },
                new TrendPoint { Day = "Śr", Score = 75 },
                new TrendPoint { Day = "Czw", Score = 71 },
                new TrendPoint { Day = "Pt", Score = 78 },
                new TrendPoint { Day = "Sob", Score = 74 },
                new TrendPoint { Day = "Ndz", Score = 72 }
            };
        }

        private void LoadUserData()
        {
            // Załaduj zapisane dane użytkownika
            var userData = _neuroScoreService.GetCurrentUserData();
            
            NeuroScore = userData.NeuroScore;
            MinutesNoBreak = userData.MinutesNoBreak;
            Hrv = userData.HRV;
        }

        #endregion

        #region Commands - Assistant View

        [RelayCommand]
        private async Task SelectGoal(string goal)
        {
            SelectedGoal = goal;

            string message = goal switch
            {
                "focus" => "Świetny wybór! Będę monitorować Twoją koncentrację i proponować krótkie gry kognitywne.",
                "stress" => "Rozumiem. Skupimy się na technikach relaksacyjnych i monitorowaniu HRV.",
                "memory" => "Doskonale! Przygotowałem serie ćwiczeń na pamięć roboczą, w tym N-back.",
                _ => "Cel wybrany. Rozpoczynamy!"
            };

            AssistantHint = message;

            // Zapisz wybór użytkownika
            await _neuroScoreService.SaveUserGoalAsync(goal);

            // Pokaż komunikat
            await ShowToast($"Cel ustawiony: {GetGoalDisplayName(goal)}");
        }

        [RelayCommand]
        private async Task StartMicroIntervention()
        {
            AssistantHint = "Przygotowuję interwencję...";

            // Pobierz odpowiednią interwencję na podstawie obecnego stanu
            var intervention = await _interventionService.GetRecommendedInterventionAsync(
                NeuroScore, 
                MinutesNoBreak, 
                SelectedGoal
            );

            if (intervention != null)
            {
                AssistantHint = $"Rozpoczynam: {intervention.Name} - {intervention.Description}";
                
                // Uruchom interwencję
                await _interventionService.ExecuteInterventionAsync(intervention);
                
                // Aktualizuj statystyki
                await RefreshNeuroScore();
            }
            else
            {
                AssistantHint = "Jesteś w świetnej formie! Nie potrzebujesz teraz interwencji.";
            }
        }

        #endregion

        #region Commands - Dashboard View

        [RelayCommand]
        private async Task ImportCsv()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Wybierz plik CSV z danymi",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".csv" } },
                        { DevicePlatform.macOS, new[] { "csv" } },
                        { DevicePlatform.Android, new[] { "text/csv" } }
                    })
                });

                if (result != null)
                {
                    await ShowToast("Importuję dane...");
                    
                    var importedData = await _dataImportService.ImportCsvAsync(result.FullPath);
                    
                    if (importedData.Success)
                    {
                        // Aktualizuj dane na podstawie importu
                        Hrv = importedData.AverageHRV;
                        
                        await ShowToast($"Zaimportowano {importedData.RecordsCount} rekordów!");
                        await RefreshNeuroScore();
                    }
                    else
                    {
                        await ShowAlert("Błąd", importedData.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Błąd", $"Nie udało się zaimportować pliku: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddCalendarBreak()
        {
            // TODO: Integracja z kalendarzem
            await ShowToast("Dodano 3-minutową przerwę do kalendarza");
            
            AssistantHint = "Przerwa zaplanowana na 11:55-11:58";
        }

        [RelayCommand]
        private async Task ForceIntervention()
        {
            await StartMicroIntervention();
        }

        [RelayCommand]
        private async Task ResetData()
        {
            bool confirm = await ShowConfirmation(
                "Reset danych", 
                "Czy na pewno chcesz zresetować wszystkie dane? Tej operacji nie można cofnąć."
            );

            if (confirm)
            {
                await _neuroScoreService.ResetAllDataAsync();
                
                // Zresetuj wartości
                NeuroScore = 50;
                MinutesNoBreak = 0;
                Hrv = 0;
                TrialsCount = 0;
                BestReactionMs = 0;
                AvgReactionMs = 0;
                
                InitializeTrendData();
                
                await ShowToast("Dane zostały zresetowane");
            }
        }

        #endregion

        #region Commands - Game View

        [RelayCommand]
        private async Task TogglePvt()
        {
            if (!IsPvtRunning)
            {
                // Rozpocznij test
                IsPvtRunning = true;
                PvtButtonText = "Zatrzymaj test";
                PvtStateText = "Przygotuj się...";
                HasLastReaction = false;

                await _pvtGameService.StartGameAsync();
            }
            else
            {
                // Zatrzymaj test
                IsPvtRunning = false;
                PvtButtonText = "Rozpocznij test";
                PvtStateText = "Gotowy?";

                await _pvtGameService.StopGameAsync();
                
                // Przyznaj punkty za grę
                await AwardPointsForGameAsync("PVT");
                
                // Zapisz wyniki do Neuro-Score
                await RefreshNeuroScore();
            }
        }

        [RelayCommand]
        private async Task OpenAvatarShop()
        {
            await Shell.Current.GoToAsync("//AvatarShopPage");
        }

        #endregion

        #region Event Handlers

        private void HandleReactionRecorded(object? sender, ReactionEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LastReactionMs = e.ReactionTimeMs;
                HasLastReaction = true;
                
                // Aktualizuj statystyki
                var stats = _pvtGameService.GetStatistics();
                BestReactionMs = stats.BestReactionMs;
                AvgReactionMs = stats.AverageReactionMs;
                TrialsCount = stats.TrialsCount;

                // Feedback od awatara
                if (e.ReactionTimeMs < 250)
                {
                    PvtStateText = "Błyskawiczny! ⚡";
                }
                else if (e.ReactionTimeMs < 350)
                {
                    PvtStateText = "Świetnie! 👍";
                }
                else if (e.ReactionTimeMs < 500)
                {
                    PvtStateText = "Dobry czas ✓";
                }
                else
                {
                    PvtStateText = "Spróbuj szybciej";
                }
            });
        }

        private void HandleGameStateChanged(object? sender, GameStateEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PvtStateText = e.State switch
                {
                    GameState.Waiting => "Czekaj...",
                    GameState.Ready => "KLIKNIJ TERAZ!",
                    GameState.TooEarly => "Za wcześnie!",
                    GameState.Completed => "Koniec rundy",
                    _ => "Gotowy?"
                };
            });
        }

        #endregion

        #region Public Methods

        public async Task RefreshDataAsync()
        {
            LoadUserData();
            await RefreshNeuroScore();
            await LoadPointsDataAsync();
        }

        public async void RefreshData()
        {
            LoadUserData();
            await RefreshNeuroScore();
            await LoadPointsDataAsync();
        }

        public void Cleanup()
        {
            // Zatrzymaj timery i cleanup
            _pvtGameService.OnReactionRecorded -= HandleReactionRecorded;
            _pvtGameService.OnGameStateChanged -= HandleGameStateChanged;
        }

        #endregion

        #region Points & Avatar System Methods

        private async Task LoadPointsDataAsync()
        {
            try
            {
                var profile = await _pointsService.GetPlayerProfileAsync();
                var currentAvatar = await _avatarService.GetCurrentAvatarAsync();
                var todaysHistory = await _pointsService.GetPointsHistoryAsync(1);
                
                // Wymusz aktualizację WSZYSTKICH właściwości w MainThread jednocześnie
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Updating points: {TotalPoints} -> {profile.TotalPoints}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Updating avatar: {CurrentAvatarName} -> {currentAvatar.Name}");
                    
                    TotalPoints = profile.TotalPoints;
                    CurrentAvatarName = currentAvatar.Name;
                    CurrentAvatarLottie = currentAvatar.LottieFileName;
                    PointsEarnedToday = todaysHistory.Sum(h => h.PointsEarned);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading points data: {ex.Message}");
            }
        }

        private async Task AwardPointsForGameAsync(string gameType)
        {
            try
            {
                var stats = _pvtGameService.GetStatistics();
                int gameScore = CalculateGameScore(stats);
                
                int pointsEarned = await _pointsService.AddPointsForGameAsync(
                    gameType, 
                    gameScore, 
                    stats.AverageReactionMs
                );

                if (pointsEarned > 0)
                {
                    TotalPoints += pointsEarned;
                    PointsEarnedToday += pointsEarned;
                    
                    await ShowToast($"🎉 Zdobyłeś {pointsEarned} punktów!");
                    
                    // Dodaj efekt wizualny lub dźwiękowy
                    AssistantHint = $"Świetnie! +{pointsEarned} pkt za grę {gameType}. Łącznie: {TotalPoints} pkt";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error awarding points: {ex.Message}");
            }
        }

        private int CalculateGameScore(PVTStatistics stats)
        {
            if (stats.TrialsCount == 0) return 0;

            // Podstawowy wynik na podstawie czasu reakcji i liczby prób
            int accuracyScore = Math.Min(100, stats.TrialsCount * 10);
            
            // Bonus za szybką reakcję
            int reactionBonus = stats.AverageReactionMs switch
            {
                < 250 => 30,
                < 300 => 20,
                < 400 => 10,
                _ => 0
            };

            return Math.Min(100, accuracyScore + reactionBonus);
        }

        #endregion

        #region Private Helper Methods

        private async Task RefreshNeuroScore()
        {
            var score = await _neuroScoreService.CalculateNeuroScoreAsync(
                _pvtGameService.GetStatistics(),
                MinutesNoBreak,
                Hrv
            );

            NeuroScore = score;
        }

        private string GetGoalDisplayName(string goal)
        {
            return goal switch
            {
                "focus" => "Koncentracja",
                "stress" => "Redukcja stresu",
                "memory" => "Pamięć",
                _ => goal
            };
        }

        private async Task ShowToast(string message)
        {
            try
            {
                // W MAUI możesz użyć Toast lub własnego mechanizmu
                await Shell.Current.DisplaySnackbar(message);
            }
            catch (Exception ex)
            {
                // Fallback jeśli DisplaySnackbar nie działa
                System.Diagnostics.Debug.WriteLine($"Toast error: {ex.Message}");
                await ShowAlert("Info", message);
            }
        }

        private async Task ShowAlert(string title, string message)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Alert error: {ex.Message}");
            }
        }

        private async Task<bool> ShowConfirmation(string title, string message)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    return await Application.Current.MainPage.DisplayAlert(title, message, "Tak", "Nie");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Confirmation error: {ex.Message}");
            }
            return false;
        }

        #endregion
    }
}
