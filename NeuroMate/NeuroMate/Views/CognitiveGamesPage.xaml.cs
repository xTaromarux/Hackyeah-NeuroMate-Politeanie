using NeuroMate.Views;

namespace NeuroMate.Views
{
    public partial class CognitiveGamesPage : ContentPage
    {
        public CognitiveGamesPage()
        {
            InitializeComponent();
            LoadGameStats();
        }

        private void LoadGameStats()
        {
            // Symulacja statystyk gier - w przyszłości z bazy danych
            // Dane są już ustawione w XAML jako przykład
        }

        private async void OnStroopGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StroopGamePage());
        }

        private async void OnPvtGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PvtGamePage());
        }

        private async void OnNBackGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NBackGamePage());
        }

        private async void OnTaskSwitchingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TaskSwitchingGamePage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadGameStats();
        }
    }
}
