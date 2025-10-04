using Microsoft.Maui.Controls;

namespace NeuroMate.Views
{
    public partial class OnboardingPage : ContentPage
    {
        private string _selectedGoal = "Koncentracja";
        private string _selectedSessionLength = "Krótkie";

        public OnboardingPage()
        {
            InitializeComponent();
            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            // Załaduj zapisane ustawienia użytkownika
            // W przyszłości to będzie z bazy danych/preferencji

            // Domyślne ustawienia - sprawdź czy Resources nie są null
            if (Application.Current?.Resources != null)
            {
                ConcentrationGoalBtn.Style = (Style)Application.Current.Resources["PrimaryButton"];
                ShortSessionBtn.Style = (Style)Application.Current.Resources["PrimaryButton"];
            }
        }

        private void OnGoalSelected(object sender, EventArgs e)
        {
            if (Application.Current?.Resources == null) return;

            // Reset wszystkich przycisków celów
            ConcentrationGoalBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            StressGoalBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            EnergyGoalBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            MemoryGoalBtn.Style = (Style)Application.Current.Resources["OutlineButton"];

            // Ustaw aktywny przycisk
            var button = sender as Button;
            button.Style = (Style)Application.Current.Resources["PrimaryButton"];

            _selectedGoal = button.Text.Split(' ')[1]; // Pobierz nazwę bez emoji
        }

        private void OnSessionLengthSelected(object sender, EventArgs e)
        {
            // Reset przycisków długości sesji
            ShortSessionBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            LongSessionBtn.Style = (Style)Application.Current.Resources["OutlineButton"];

            // Ustaw aktywny przycisk
            var button = sender as Button;
            button.Style = (Style)Application.Current.Resources["PrimaryButton"];

            _selectedSessionLength = button.Text.Split(' ')[0]; // "Krótkie" lub "Dłuższe"
        }

        private async void OnSaveSettings(object sender, EventArgs e)
        {
            try
            {
                // Zbierz wszystkie ustawienia
                var settings = new UserSettings
                {
                    MainGoal = _selectedGoal,
                    SessionLength = _selectedSessionLength,
                    WorkStartTime = WorkStartTime.Time,
                    WorkEndTime = WorkEndTime.Time,
                    SmartbandEnabled = SmartbandSwitch.IsToggled,
                    CalendarEnabled = CalendarSwitch.IsToggled,
                    ActivityMonitoringEnabled = ActivitySwitch.IsToggled
                };

                // Zapisz ustawienia (symulacja)
                await SaveUserSettings(settings);

                // Pokaż potwierdzenie
                await DisplayAlert("✅ Zapisano!",
                    $"Twoje ustawienia zostały zapisane.\nCel: {_selectedGoal}\nSesje: {_selectedSessionLength}\nPraca: {WorkStartTime.Time:hh\\:mm} - {WorkEndTime.Time:hh\\:mm}",
                    "OK");

                // Wróć do Dashboard
                await Shell.Current.GoToAsync("///MainPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Błąd", $"Nie udało się zapisać ustawień: {ex.Message}", "OK");
            }
        }

        private async Task SaveUserSettings(UserSettings settings)
        {
            // Symulacja zapisu - w przyszłości będzie to prawdziwa baza danych
            await Task.Delay(500);

            // Zapisz do Preferences
            Preferences.Set("MainGoal", settings.MainGoal);
            Preferences.Set("SessionLength", settings.SessionLength);
            Preferences.Set("WorkStartTime", settings.WorkStartTime.ToString());
            Preferences.Set("WorkEndTime", settings.WorkEndTime.ToString());
            Preferences.Set("SmartbandEnabled", settings.SmartbandEnabled);
            Preferences.Set("CalendarEnabled", settings.CalendarEnabled);
            Preferences.Set("ActivityMonitoringEnabled", settings.ActivityMonitoringEnabled);
        }

        public class UserSettings
        {
            public string MainGoal { get; set; }
            public string SessionLength { get; set; }
            public TimeSpan WorkStartTime { get; set; }
            public TimeSpan WorkEndTime { get; set; }
            public bool SmartbandEnabled { get; set; }
            public bool CalendarEnabled { get; set; }
            public bool ActivityMonitoringEnabled { get; set; }
        }
    }
}
