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

    #region Import Models

    /// <summary>
    /// Wynik importu CSV
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public int RecordsCount { get; set; }
        public int AverageHRV { get; set; }
        public int TotalSteps { get; set; }
        public int AverageSleepMinutes { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<HealthRecord> Records { get; set; } = new();
    }

    /// <summary>
    /// Pojedynczy rekord zdrowotny z CSV
    /// </summary>
    public class HealthRecord
    {
        public DateTime Timestamp { get; set; }
        public int? HeartRate { get; set; }
        public int? HRV { get; set; }
        public int? Steps { get; set; }
        public int? SleepMinutes { get; set; }
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

    #region Neuro-Score Models

    /// <summary>
    /// Komponenty składające się na Neuro-Score
    /// </summary>
    public class NeuroScoreComponents
    {
        public double ReactionTimeScore { get; set; }  // 0-1
        public double AccuracyScore { get; set; }       // 0-1
        public double BreakTimeScore { get; set; }      // 0-1
        public double HRVScore { get; set; }            // 0-1
        public double SleepScore { get; set; }          // 0-1
        
        public Dictionary<string, double> Weights { get; set; } = new()
        {
            { "ReactionTime", 0.4 },
            { "Accuracy", 0.3 },
            { "BreakTime", 0.2 },
            { "HRV", 0.1 }
        };
    }

    /// <summary>
    /// Historia Neuro-Score
    /// </summary>
    public class NeuroScoreHistory
    {
        public DateTime Timestamp { get; set; }
        public int Score { get; set; }
        public NeuroScoreComponents Components { get; set; } = new();
        public string Trigger { get; set; } = string.Empty; // Co spowodowało wyliczenie
    }

    #endregion

    #region Avatar Models

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