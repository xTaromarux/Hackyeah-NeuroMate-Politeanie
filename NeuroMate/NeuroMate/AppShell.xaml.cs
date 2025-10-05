namespace NeuroMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

            // Gry kognitywne
            Routing.RegisterRoute(nameof(CognitiveGamesPage), typeof(CognitiveGamesPage));
            Routing.RegisterRoute(nameof(StroopGamePage), typeof(StroopGamePage));
            Routing.RegisterRoute(nameof(PvtGamePage), typeof(PvtGamePage));
            Routing.RegisterRoute(nameof(TaskSwitchingGamePage), typeof(TaskSwitchingGamePage));

            // Interwencje
            Routing.RegisterRoute(nameof(InterventionPage), typeof(InterventionPage));

            // Sen
            Routing.RegisterRoute(nameof(SleepPage), typeof(SleepPage));

            // Podsumowanie
            Routing.RegisterRoute(nameof(DailySummaryPage), typeof(DailySummaryPage));

            // Ustawienia / Onboarding
            Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
            
            // Rejestracja routingu dla nowych stron
            Routing.RegisterRoute(nameof(LootBoxPage), typeof(Views.LootBoxPage));
        }
    }
}
