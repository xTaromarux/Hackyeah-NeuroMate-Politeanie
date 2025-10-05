namespace NeuroMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

            // Gry kognitywne
            Routing.RegisterRoute(nameof(Views.CognitiveGamesPage), typeof(Views.CognitiveGamesPage));
            Routing.RegisterRoute(nameof(Views.StroopGamePage), typeof(Views.StroopGamePage));
            Routing.RegisterRoute(nameof(Views.PvtGamePage), typeof(Views.PvtGamePage));
            Routing.RegisterRoute(nameof(Views.TaskSwitchingGamePage), typeof(Views.TaskSwitchingGamePage));

            // Interwencje
            Routing.RegisterRoute(nameof(Views.InterventionPage), typeof(Views.InterventionPage));

            // Sen - usuwam jeśli nie istnieje
            // Routing.RegisterRoute(nameof(SleepPage), typeof(SleepPage));

            // Podsumowanie
            Routing.RegisterRoute(nameof(Views.DailySummaryPage), typeof(Views.DailySummaryPage));

            // Ustawienia / Onboarding
            Routing.RegisterRoute(nameof(Views.OnboardingPage), typeof(Views.OnboardingPage));
            
            // Rejestracja routingu dla nowych stron
            Routing.RegisterRoute(nameof(Views.LootBoxPage), typeof(Views.LootBoxPage));
        }
    }
}
