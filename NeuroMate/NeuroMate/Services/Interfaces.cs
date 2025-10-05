using NeuroMate.Models;

namespace NeuroMate.Services
{
    /// <summary>
    /// Serwis do kalkulacji i zarządzania Neuro-Score
    /// </summary>
    public interface INeuroScoreService
    {
        /// <summary>
        /// Oblicza aktualny Neuro-Score na podstawie różnych metryk
        /// </summary>
        Task<int> CalculateNeuroScoreAsync(
            PVTStatistics pvtStats,
            int minutesNoBreak,
            int hrv,
            int? sleepMinutes = null
        );

        /// <summary>
        /// Pobiera obecne dane użytkownika
        /// </summary>
        UserData GetCurrentUserData();

        /// <summary>
        /// Zapisuje cel użytkownika
        /// </summary>
        Task SaveUserGoalAsync(string goal);

        /// <summary>
        /// Resetuje wszystkie dane
        /// </summary>
        Task ResetAllDataAsync();

        /// <summary>
        /// Pobiera historię Neuro-Score
        /// </summary>
        Task<List<NeuroScoreHistory>> GetScoreHistoryAsync(int days = 7);

        /// <summary>
        /// Pobiera komponenty ostatniego wyliczonego score
        /// </summary>
        NeuroScoreComponents GetLastScoreComponents();
    }

    /// <summary>
    /// Serwis zarządzający interwencjami
    /// </summary>
    public interface IInterventionService
    {
        /// <summary>
        /// Pobiera rekomendowaną interwencję na podstawie obecnego stanu
        /// </summary>
        Task<Intervention?> GetRecommendedInterventionAsync(
            int neuroScore,
            int minutesNoBreak,
            string userGoal
        );

        /// <summary>
        /// Wykonuje wybraną interwencję
        /// </summary>
        Task<InterventionResult> ExecuteInterventionAsync(Intervention intervention);

        /// <summary>
        /// Pobiera wszystkie dostępne interwencje
        /// </summary>
        List<Intervention> GetAllInterventions();

        /// <summary>
        /// Zapisuje wynik interwencji
        /// </summary>
        Task SaveInterventionResultAsync(InterventionResult result);

        /// <summary>
        /// Pobiera historię interwencji
        /// </summary>
        Task<List<InterventionResult>> GetInterventionHistoryAsync(DateTime? from = null);
    }

    /// <summary>
    /// Serwis gry PVT (Psychomotor Vigilance Test)
    /// </summary>
    public interface IPVTGameService
    {
        /// <summary>
        /// Event wywoływany gdy zarejestrowano reakcję
        /// </summary>
        event EventHandler<ReactionEventArgs>? OnReactionRecorded;

        /// <summary>
        /// Event wywoływany gdy zmienia się stan gry
        /// </summary>
        event EventHandler<GameStateEventArgs>? OnGameStateChanged;

        /// <summary>
        /// Rozpoczyna grę PVT
        /// </summary>
        Task StartGameAsync();

        /// <summary>
        /// Zatrzymuje grę PVT
        /// </summary>
        Task StopGameAsync();

        /// <summary>
        /// Rejestruje reakcję gracza
        /// </summary>
        void RecordReaction(int reactionTimeMs);

        /// <summary>
        /// Pobiera statystyki bieżącej sesji
        /// </summary>
        PVTStatistics GetStatistics();

        /// <summary>
        /// Resetuje statystyki
        /// </summary>
        void ResetStatistics();

        /// <summary>
        /// Sprawdza czy gra jest aktywna
        /// </summary>
        bool IsGameActive { get; }
    }

    /// <summary>
    /// Serwis importu danych z zewnętrznych źródeł
    /// </summary>
    public interface IDataImportService
    {
        /// <summary>
        /// Importuje dane z pliku CSV
        /// </summary>
        Task<ImportResult> ImportCsvAsync(string filePath);

        /// <summary>
        /// Waliduje format pliku CSV
        /// </summary>
        Task<bool> ValidateCsvFormatAsync(string filePath);

        /// <summary>
        /// Parsuje pojedynczy rekord z CSV
        /// </summary>
        Database.Entities.HealthRecord ParseHealthRecord(string[] csvRow, string[] headers);

        /// <summary>
        /// Importuje dane z Google Fit (opcjonalnie)
        /// </summary>
        Task<ImportResult> ImportFromGoogleFitAsync();

        /// <summary>
        /// Importuje dane z Apple Health (opcjonalnie)
        /// </summary>
        Task<ImportResult> ImportFromAppleHealthAsync();
    }

    /// <summary>
    /// Serwis zarządzający punktami gracza
    /// </summary>
    public interface IPointsService
    {
        Task<PlayerProfileData> GetCurrentPlayerAsync();
        Task<bool> SpendPointsAsync(int amount);
        Task AddPointsAsync(int amount);
        Task<int> GetCurrentPointsAsync();
        
        // Dodatkowe metody dla AvatarShopPage
        Task<NeuroMate.Models.PlayerProfile> GetPlayerProfileAsync();
        event Action OnProfileChanged;
        
        // Dodatkowe metody dla MainViewModel
        Task<List<PointsHistory>> GetPointsHistoryAsync(int days);
        Task<int> AddPointsForGameAsync(string gameType, int gameScore, int avgReactionMs);
    }

    /// <summary>
    /// Serwis zarządzający awatarami
    /// </summary>
    public interface IAvatarService
    {
        Task<List<NeuroMate.Database.Entities.Avatar>> GetAvailableAvatarsAsync();
        Task<NeuroMate.Database.Entities.Avatar?> GetAvatarByIdAsync(int avatarId);
        Task UnlockAvatarAsync(int avatarId);
        Task SelectAvatarAsync(int avatarId);
        Task<NeuroMate.Database.Entities.Avatar?> GetSelectedAvatarAsync();
        
        // Dodatkowe metody dla AvatarShopPage
        Task<List<NeuroMate.Database.Entities.Avatar>> GetAllAvatarsAsync();
        Task<bool> ChangeAvatarAsync(string avatarId);
        Task<bool> PurchaseAvatarAsync(string avatarId);
    }

    /// <summary>
    /// Serwis zarządzający lootboxami
    /// </summary>
    public interface ILootBoxService
    {
        Task InitializeDefaultLootBoxesAsync();
        Task<List<NeuroMate.Database.Entities.LootBox>> GetAvailableLootBoxesAsync();
        Task<NeuroMate.Database.Entities.LootBoxResult> OpenLootBoxAsync(int lootBoxId);
        Task<NeuroMate.Database.Entities.Avatar?> GetAvatarByIdAsync(int avatarId);
    }

    /// <summary>
    /// Model wydarzenia kalendarzowego
    /// </summary>
    public class CalendarEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsBreak { get; set; }
    }

    /// <summary>
    /// Serwis lokalnej bazy danych
    /// </summary>
    public interface ILocalDatabaseService
    {
        /// <summary>
        /// Inicjalizuje bazę danych
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Zapisuje dane użytkownika
        /// </summary>
        Task SaveUserDataAsync(UserData data);

        /// <summary>
        /// Pobiera dane użytkownika
        /// </summary>
        Task<UserData?> GetUserDataAsync();

        /// <summary>
        /// Zapisuje rekord Neuro-Score
        /// </summary>
        Task SaveNeuroScoreRecordAsync(NeuroScoreHistory record);

        /// <summary>
        /// Pobiera historię Neuro-Score
        /// </summary>
        Task<List<NeuroScoreHistory>> GetNeuroScoreHistoryAsync(int days);

        /// <summary>
        /// Eksportuje wszystkie dane do JSON
        /// </summary>
        Task<string> ExportAllDataAsync();

        /// <summary>
        /// Importuje dane z JSON
        /// </summary>
        Task<bool> ImportDataAsync(string jsonData);

        /// <summary>
        /// Usuwa wszystkie dane
        /// </summary>
        Task EraseAllDataAsync();
    }
}