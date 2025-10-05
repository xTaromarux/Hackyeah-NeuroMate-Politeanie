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
        HealthRecord ParseHealthRecord(string[] csvRow, string[] headers);

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
    /// Serwis zarządzający systemem punktów i nagród
    /// </summary>
    public interface IPointsService
    {
        event Action OnProfileChanged;
        /// <summary>
        /// Dodaje punkty za wykonanie gry
        /// </summary>
        Task<int> AddPointsForGameAsync(string gameType, int gameScore, int reactionTimeMs = 0);

        /// <summary>
        /// Pobiera aktualny profil gracza
        /// </summary>
        Task<PlayerProfile> GetPlayerProfileAsync();

        /// <summary>
        /// Pobiera historię punktów
        /// </summary>
        Task<List<PointsHistory>> GetPointsHistoryAsync(int days = 30);

        /// <summary>
        /// Zapisuje profil gracza
        /// </summary>
        Task SavePlayerProfileAsync(PlayerProfile profile);

        /// <summary>
        /// Wydaje punkty
        /// </summary>
        Task<bool> SpendPointsAsync(int amount);
    }

    /// <summary>
    /// Serwis zarządzający awatarami
    /// </summary>
    public interface IAvatarService
    {
        /// <summary>
        /// Pobiera wszystkie dostępne awatary
        /// </summary>
        Task<List<Avatar>> GetAllAvatarsAsync();

        /// <summary>
        /// Pobiera odblokowane awatary gracza
        /// </summary>
        Task<List<Avatar>> GetUnlockedAvatarsAsync();

        /// <summary>
        /// Kupuje awatara za punkty
        /// </summary>
        Task<bool> PurchaseAvatarAsync(string avatarId);

        /// <summary>
        /// Zmienia aktualnego awatara
        /// </summary>
        Task<bool> ChangeAvatarAsync(string avatarId);

        /// <summary>
        /// Pobiera aktualnego awatara
        /// </summary>
        Task<Avatar> GetCurrentAvatarAsync();

        /// <summary>
        /// Sprawdza czy awatar jest odblokowany
        /// </summary>
        Task<bool> IsAvatarUnlockedAsync(string avatarId);
    }

    /// <summary>
    /// Serwis zarządzający lootboxami
    /// </summary>
    public interface ILootBoxService
    {
        /// <summary>
        /// Pobiera dostępne lootboxy
        /// </summary>
        Task<List<LootBox>> GetAvailableLootBoxesAsync();

        /// <summary>
        /// Otwiera lootbox
        /// </summary>
        Task<LootBoxResult> OpenLootBoxAsync(string lootBoxId);

        /// <summary>
        /// Sprawdza czy gracz może kupić lootbox
        /// </summary>
        Task<bool> CanAffordLootBoxAsync(string lootBoxId);

        /// <summary>
        /// Generuje nagrodę z lootboxa
        /// </summary>
        Task<Avatar> GenerateRewardAsync(LootBox lootBox);
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