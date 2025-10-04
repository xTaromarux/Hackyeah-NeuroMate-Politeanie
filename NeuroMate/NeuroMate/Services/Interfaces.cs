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
    /// Serwis zarządzania awatarem
    /// </summary>
    public interface IAvatarService
    {
        /// <summary>
        /// Pobiera konfigurację awatara
        /// </summary>
        AvatarConfig GetConfig();

        /// <summary>
        /// Zapisuje konfigurację awatara
        /// </summary>
        Task SaveConfigAsync(AvatarConfig config);

        /// <summary>
        /// Generuje wiadomość awatara na podstawie kontekstu
        /// </summary>
        AvatarMessage GenerateMessage(
            string context,
            int neuroScore,
            InterventionResult? lastIntervention = null
        );

        /// <summary>
        /// Pobiera losową wiadomość motywacyjną
        /// </summary>
        string GetMotivationalMessage();

        /// <summary>
        /// Zmienia nastrój awatara
        /// </summary>
        void SetMood(AvatarMood mood);
    }

    /// <summary>
    /// Serwis gamifikacji
    /// </summary>
    public interface IGamificationService
    {
        /// <summary>
        /// Dodaje punkty użytkownikowi
        /// </summary>
        Task<int> AddPointsAsync(int points, string reason);

        /// <summary>
        /// Pobiera postęp użytkownika
        /// </summary>
        Task<UserProgress> GetUserProgressAsync();

        /// <summary>
        /// Sprawdza czy użytkownik odblokowal nową odznakę
        /// </summary>
        Task<List<Badge>> CheckForNewBadgesAsync();

        /// <summary>
        /// Odblokuje skórkę awatara
        /// </summary>
        Task<bool> UnlockSkinAsync(string skinId);

        /// <summary>
        /// Ustawia aktywną skórkę
        /// </summary>
        Task EquipSkinAsync(string skinId);

        /// <summary>
        /// Pobiera wszystkie dostępne skórki
        /// </summary>
        List<AvatarSkin> GetAllSkins();

        /// <summary>
        /// Aktualizuje streak (serię dni)
        /// </summary>
        Task UpdateStreakAsync();
    }

    /// <summary>
    /// Serwis monitorowania aktywności na komputerze
    /// </summary>
    public interface IActivityMonitorService
    {
        /// <summary>
        /// Rozpoczyna monitorowanie
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Zatrzymuje monitorowanie
        /// </summary>
        void StopMonitoring();

        /// <summary>
        /// Pobiera czas bez przerwy w minutach
        /// </summary>
        int GetMinutesWithoutBreak();

        /// <summary>
        /// Resetuje licznik czasu
        /// </summary>
        void ResetTimer();

        /// <summary>
        /// Event wywoływany gdy wykryto długi czas bez przerwy
        /// </summary>
        event EventHandler<int>? OnLongWorkSessionDetected;

        /// <summary>
        /// Wykrywa "kaskadę Alt+Tab" (częste przełączanie aplikacji)
        /// </summary>
        bool DetectAltTabCascade();
    }

    /// <summary>
    /// Serwis integracji z kalendarzem
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Pobiera dzisiejsze wydarzenia
        /// </summary>
        Task<List<CalendarEvent>> GetTodayEventsAsync();

        /// <summary>
        /// Dodaje przerwę do kalendarza
        /// </summary>
        Task<bool> AddBreakEventAsync(DateTime startTime, int durationMinutes);

        /// <summary>
        /// Synchronizuje z Google Calendar
        /// </summary>
        Task<bool> SyncWithGoogleCalendarAsync();

        /// <summary>
        /// Synchronizuje z Microsoft Calendar
        /// </summary>
        Task<bool> SyncWithMicrosoftCalendarAsync();

        /// <summary>
        /// Proponuje optymalne czasy przerw
        /// </summary>
        Task<List<DateTime>> SuggestBreakTimesAsync();
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