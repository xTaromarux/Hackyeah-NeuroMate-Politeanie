using SQLite;

namespace NeuroMate.Models
{
    #region Dashboard Models

    /// <summary>
    /// Reprezentuje punkt na wykresie trendu
    /// </summary>
    public class TrendPoint
    {
        public string Day { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    /// <summary>
    /// Dane użytkownika do dashboardu
    /// </summary>
    public class UserData
    {
        public int NeuroScore { get; set; }
        public int MinutesNoBreak { get; set; }
        public int HRV { get; set; }
        public DateTime LastUpdate { get; set; }
        public string SelectedGoal { get; set; } = string.Empty;
    }

    #endregion

    #region Game Models

    /// <summary>
    /// Statystyki z gry PVT
    /// </summary>
    public class PVTStatistics
    {
        public int BestReactionMs { get; set; }
        public int AverageReactionMs { get; set; }
        public int TrialsCount { get; set; }
        public List<int> AllReactions { get; set; } = new();
        public DateTime SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
    }

    /// <summary>
    /// Pojedyncza reakcja w grze
    /// </summary>
    public class ReactionRecord
    {
        public int ReactionTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsValid { get; set; }
    }

    #endregion

    #region Intervention Models

    /// <summary>
    /// Typ interwencji
    /// </summary>
    public enum InterventionType
    {
        CognitiveGame,      // Mini-gra kognitywna
        PhysicalActivity,   // Aktywność fizyczna
        EyeReset,          // Reset oczu
        BreathingExercise, // Ćwiczenie oddechowe
        NutritionTip,      // Wskazówka żywieniowa
        CoffeeNap          // Coffee-nap
    }

    /// <summary>
    /// Model interwencji
    /// </summary>
    public class Intervention
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InterventionType Type { get; set; }
        public int DurationSeconds { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string AvatarMessage { get; set; } = string.Empty;
        public int Priority { get; set; } // Wyższy = ważniejszy
    }

    /// <summary>
    /// Wynik wykonanej interwencji
    /// </summary>
    public class InterventionResult
    {
        public string InterventionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Completed { get; set; }
        public int ScoreBeforeIntervention { get; set; }
        public int ScoreAfterIntervention { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    #endregion

    #region Player Profile Models

    /// <summary>
    /// Dane profilu gracza do bazy danych
    /// </summary>
    public class PlayerProfileData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Points { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int SelectedAvatarId { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Historia punktów gracza
    /// </summary>
    public class PointsHistoryData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int PointsChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Profil gracza z punktami (dla UI)
    /// </summary>
    public class PlayerProfile
    {
        public int TotalPoints { get; set; }
        public int PointsSpent { get; set; }
        public string CurrentAvatarId { get; set; } = "1";
        public List<string> UnlockedAvatarIds { get; set; } = new();
        public int TotalGamesPlayed { get; set; }
        public int TotalLootBoxesOpened { get; set; }
        public DateTime LastPointsEarned { get; set; }
    }

    /// <summary>
    /// Historia zdobywania punktów
    /// </summary>
    public class PointsHistory
    {
        public DateTime Timestamp { get; set; }
        public int PointsEarned { get; set; }
        public string Source { get; set; } = string.Empty; // "PVT_Game", "Stroop_Game", etc.
        public int GameScore { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    #endregion

    #region Health Data Models

    /// <summary>
    /// Dane snu
    /// </summary>
    public class SleepData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SleepDurationMinutes { get; set; }
        public int DeepSleepMinutes { get; set; }
        public int RemSleepMinutes { get; set; }
        public int WakeUpCount { get; set; }
        public int SleepEfficiency { get; set; }
    }

    /// <summary>
    /// Dane tętna
    /// </summary>
    public class HeartData
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int HeartRate { get; set; }
        public int HRV { get; set; }
        public string Context { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dane aktywności
    /// </summary>
    public class ActivityData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Steps { get; set; }
        public int CaloriesBurned { get; set; }
        public int ActiveMinutes { get; set; }
        public double DistanceKm { get; set; }
    }

    /// <summary>
    /// Historia NeuroScore
    /// </summary>
    public class NeuroScoreHistory
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int OverallScore { get; set; }
        public string ComponentsJson { get; set; } = string.Empty;
        
        // Dodatkowe właściwości używane w NeuroScoreService
        public int Score { get; set; }
        public NeuroScoreComponents Components { get; set; } = new();
        public string Trigger { get; set; } = string.Empty; // Co spowodowało wyliczenie
    }

    /// <summary>
    /// Komponenty NeuroScore - wersja z dodatkowymi właściwościami dla obliczeń
    /// </summary>
    public class NeuroScoreComponents
    {
        // Właściwości używane w obliczeniach (0-1)
        public double ReactionTimeScore { get; set; }  
        public double AccuracyScore { get; set; }       
        public double BreakTimeScore { get; set; }      
        public double HRVScore { get; set; }            
        public double SleepScoreNormalized { get; set; } // Zmieniam nazwę dla obliczeń          
        
        public Dictionary<string, double> Weights { get; set; } = new()
        {
            { "ReactionTime", 0.4 },
            { "Accuracy", 0.3 },
            { "BreakTime", 0.2 },
            { "HRV", 0.1 }
        };

        // Właściwości z bazodanowej wersji dla kompatybilności
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SleepScore { get; set; } // To zostaje jako int dla bazy danych
        public int ActivityScore { get; set; }
        public int HeartScore { get; set; }
        public int CognitiveScore { get; set; }
        public int StressScore { get; set; }
    }

    /// <summary>
    /// Rekord zdrowia
    /// </summary>
    public class HealthRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// Wynik importu danych
    /// </summary>
    public class ImportResult
    {
        public int Id { get; set; }
        public DateTime ImportDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public int RecordsImported { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    #endregion

    #region Event Args

    /// <summary>
    /// Event args dla reakcji w grze
    /// </summary>
    public class ReactionEventArgs : EventArgs
    {
        public int ReactionTimeMs { get; set; }
        public bool IsValid { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Stany gry
    /// </summary>
    public enum GameState
    {
        Idle,
        Waiting,
        Ready,
        TooEarly,
        Completed
    }

    /// <summary>
    /// Event args dla zmiany stanu gry
    /// </summary>
    public class GameStateEventArgs : EventArgs
    {
        public GameState State { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion

    #region Avatar Models

    /// <summary>
    /// Rzadkość awatara
    /// </summary>
    public enum AvatarRarity
    {
        Common,     // Zwykły - 100-200 pkt
        Rare,       // Rzadki - 300-500 pkt
        Epic,       // Epicki - 600-800 pkt
        Legendary   // Legendarny - 1000+ pkt
    }

    /// <summary>
    /// Stan awatara
    /// </summary>
    public enum AvatarMood
    {
        Happy,
        Neutral,
        Concerned,
        Proud,
        Encouraging
    }

    /// <summary>
    /// Konfiguracja awatara
    /// </summary>
    public class AvatarConfig
    {
        public string Style { get; set; } = "Robot"; // Robot, Professor, Trainer
        public string Tone { get; set; } = "Neutral"; // Serious, Neutral, Humorous
        public bool SoundEnabled { get; set; } = true;
        public bool DiscreetMode { get; set; } = false; // Tylko tekst, bez dźwięku
    }

    /// <summary>
    /// Wiadomość od awatara
    /// </summary>
    public class AvatarMessage
    {
        public string Text { get; set; } = string.Empty;
        public AvatarMood Mood { get; set; }
        public string AnimationName { get; set; } = string.Empty;
        public int DisplayDurationMs { get; set; } = 3000;
    }

    #endregion

    #region Gamification Models

    /// <summary>
    /// Odznaka do odblokowania
    /// </summary>
    public class Badge
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int RequiredPoints { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }

    /// <summary>
    /// Skórka awatara
    /// </summary>
    public class AvatarSkin
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string PreviewImageUrl { get; set; } = string.Empty;
        public int Cost { get; set; } // Koszt w punktach
        public bool IsUnlocked { get; set; }
        public bool IsEquipped { get; set; }
    }

    /// <summary>
    /// Postęp użytkownika w gamifikacji
    /// </summary>
    public class UserProgress
    {
        public int TotalPoints { get; set; }
        public int Level { get; set; }
        public int CurrentStreakDays { get; set; }
        public int LongestStreakDays { get; set; }
        public List<Badge> Badges { get; set; } = new();
        public List<AvatarSkin> UnlockedSkins { get; set; } = new();
        public DateTime LastActivityDate { get; set; }
    }


    #endregion
}