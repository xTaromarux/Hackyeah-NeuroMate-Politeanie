namespace NeuroMate.Views
{
    public partial class InterventionPage : ContentPage
    {
        private readonly Random _random = new();
        private Timer? _interventionTimer;
        private int _countdownTime = 0;
        private bool _isInterventionRunning = false;
        private string _selectedCategory = "Kognitywne";
        
        private readonly Dictionary<string, List<Intervention>> _interventions = new()
        {
            ["Kognitywne"] = new List<Intervention>
            {
                new("🧠 Mini-Medytacja", "Zamknij oczy i skup się na oddechu przez 60 sekund", 60),
                new("🎯 Test Stroop", "Wykonaj szybki test Stroop na koncentrację", 45),
                new("🔢 Mental Math", "Policz w pamięci: 17 x 23, potem 156 + 89", 30),
                new("📝 Mindfulness", "Nazwij 5 rzeczy, które widzisz, 4 które słyszysz, 3 które czujesz", 90)
            },
            ["Fizyczne"] = new List<Intervention>
            {
                new("🤸‍♂️ Rozciąganie Karku", "Powoli obróć głowę w lewo, prawo, do przodu i tyłu", 45),
                new("💪 Ćwiczenia Ramion", "10 obrotów ramionami do przodu i tyłu", 30),
                new("🦵 Przysiady", "Wykonaj 10 przysiadów przy biurku", 60),
                new("🚶‍♂️ Spacer", "Przejdź się po biurze lub na zewnątrz", 120),
                new("🧘‍♀️ Rozciąganie Pleców", "Wyciągnij ręce w górę i delikatnie pochyl się w boki", 45)
            },
            ["Oczy"] = new List<Intervention>
            {
                new("👁️ Patrzenie w Dal", "Popatrz przez okno na najdalszy punkt przez 20 sekund", 20),
                new("👀 Mruganie", "20 razy powoli mrugaj oczami", 30),
                new("🔄 Obroty Oczami", "Obróć oczami 5 razy w każdą stronę", 25),
                new("👁️ Zasada 20-20-20", "Co 20 minut patrz 20 sekund na obiekt 20 stóp dalej", 20),
                new("😑 Zamknięte Oczy", "Zamknij oczy i zrelaksuj mięśnie twarzy", 30)
            },
            ["Odżywianie"] = new List<Intervention>
            {
                new("💧 Nawodnienie", "Wypij szklankę wody powoli", 60),
                new("🥜 Zdrowa Przekąska", "Zjedz garść orzechów lub owoców", 120),
                new("☕ Coffee Nap", "Wypij kawę i zdrzemnij się na 15-20 minut", 900),
                new("🫖 Herbata", "Zaparz i wypij herbatę ziołową", 300),
                new("🍎 Owoc", "Zjedz świeży owoc bogaty w witaminy", 180)
            }
        };

        private readonly List<string> _dailyTips = new()
        {
            "Spróbuj techniki Pomodoro: 25 minut pracy + 5 minut przerwy. To pomaga utrzymać wysoką koncentrację przez cały dzień.",
            "Pij wodę regularnie - nawet 2% odwodnienia może zmniejszyć koncentrację o 23%.",
            "Rób krótkie przerwy co 90 minut - to naturalny rytm twojego mózgu.",
            "Jedz omega-3 (ryby, orzechy) - to paliwo dla neuronów.",
            "Śpij 7-9 godzin - podczas snu mózg czyści toksyny i konsoliduje pamięć.",
            "Ćwicz regularnie - aktywność fizyczna zwiększa BDNF (nawóz dla mózgu).",
            "Medytuj 10 minut dziennie - zwiększa to grubość kory przedczołowej.",
            "Unikaj multitaskingu - mózg może skupić się tylko na jednej rzeczy na raz."
        };

        public InterventionPage()
        {
            InitializeComponent();
            LoadTodayData();
            LoadQuickInterventions();
            LoadTodayInterventions();
            ShowRandomTip();
            LoadRandomIntervention();
        }

        private void LoadTodayData()
        {
            // Symulacja danych dzisiejszych interwencji
            TodayCountLabel.Text = "5/8";
        }

        private void LoadQuickInterventions()
        {
            QuickInterventionsList.Children.Clear();

            var quickInterventions = _interventions[_selectedCategory].Take(3);
            
            foreach (var intervention in quickInterventions)
            {
                var button = new Button
                {
                    Text = $"{intervention.Title} ({intervention.DurationSeconds}s)",
                    Style = (Style?)Application.Current?.Resources["OutlineButton"],
                    HorizontalOptions = LayoutOptions.Fill,
                    FontSize = 12
                };
                
                button.Clicked += (s, e) => StartQuickIntervention(intervention);
                QuickInterventionsList.Children.Add(button);
            }
        }

        private void LoadTodayInterventions()
        {
            TodayInterventionsList.Children.Clear();

            // Symulacja dzisiejszych interwencji
            var todayInterventions = new[]
            {
                ("✅", "Reset Oczu - 20s", "10:30", Color.FromArgb("#48bb78")),
                ("✅", "Rozciąganie Karku - 45s", "12:15", Color.FromArgb("#48bb78")),
                ("✅", "Mini-Medytacja - 60s", "14:20", Color.FromArgb("#48bb78")),
                ("⏳", "Nawodnienie - 60s", "16:00", Color.FromArgb("#ed8936")),
                ("⏳", "Spacer - 2min", "17:30", Color.FromArgb("#ed8936"))
            };

            foreach (var (status, title, time, color) in todayInterventions)
            {
                var grid = new Grid
                {
                    ColumnSpacing = 10,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
                    }
                };

                grid.Add(new Label { Text = status, FontSize = 14, VerticalOptions = LayoutOptions.Center }, 0, 0);
                grid.Add(new Label { Text = title, FontSize = 12, VerticalOptions = LayoutOptions.Center, TextColor = color }, 1, 0);
                grid.Add(new Label { Text = time, FontSize = 10, VerticalOptions = LayoutOptions.Center, TextColor = Color.FromArgb("#718096") }, 2, 0);

                TodayInterventionsList.Children.Add(grid);
            }
        }

        private void ShowRandomTip()
        {
            var randomTip = _dailyTips[_random.Next(_dailyTips.Count)];
            TipOfTheDayLabel.Text = randomTip;
        }

        private void LoadRandomIntervention()
        {
            var interventions = _interventions[_selectedCategory];
            var randomIntervention = interventions[_random.Next(interventions.Count)];
            
            InterventionTitleLabel.Text = randomIntervention.Title;
            InterventionDescriptionLabel.Text = randomIntervention.Description;
            InterventionTimeLabel.Text = $"{randomIntervention.DurationSeconds}s";
            CountdownLabel.Text = randomIntervention.DurationSeconds.ToString();
            _countdownTime = randomIntervention.DurationSeconds;
        }

        private void OnCategorySelected(object sender, EventArgs e)
        {
            // Reset stylów przycisków
            CognitiveBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            PhysicalBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            EyesBtn.Style = (Style)Application.Current.Resources["OutlineButton"];
            NutritionBtn.Style = (Style)Application.Current.Resources["OutlineButton"];

            var button = sender as Button;
            button.Style = (Style)Application.Current.Resources["PrimaryButton"];

            // Określ wybraną kategorię
            _selectedCategory = button.Text.Split(' ')[1]; // Pobierz nazwę bez emoji

            // Odśwież interwencje dla nowej kategorii
            LoadQuickInterventions();
            LoadRandomIntervention();
        }

        private async void OnStartIntervention(object sender, EventArgs e)
        {
            if (_isInterventionRunning) return;

            _isInterventionRunning = true;
            StartInterventionBtn.Text = "⏸️ Stop";
            StartInterventionBtn.Style = (Style)Application.Current.Resources["SecondaryButton"];
            SkipInterventionBtn.IsEnabled = false;

            // Uruchom timer odliczania
            _interventionTimer = new Timer(CountdownTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void CountdownTick(object? state)
        {
            _countdownTime--;
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CountdownLabel.Text = _countdownTime.ToString();
                
                if (_countdownTime <= 0)
                {
                    CompleteIntervention();
                }
            });
        }

        private async void CompleteIntervention()
        {
            StopIntervention();
            
            await DisplayAlert("🎉 Interwencja ukończona!", 
                "Świetnie! Ukończyłeś interwencję. Twój mózg właśnie otrzymał potrzebny reset.", 
                "OK");

            // Załaduj nową losową interwencję
            LoadRandomIntervention();
            
            // Aktualizuj licznik dzisiejszych interwencji
            UpdateTodayCount();
        }

        private void OnSkipIntervention(object sender, EventArgs e)
        {
            LoadRandomIntervention();
        }

        private void StopIntervention()
        {
            _isInterventionRunning = false;
            _interventionTimer?.Dispose();
            
            StartInterventionBtn.Text = "▶️ Start";
            StartInterventionBtn.Style = (Style)Application.Current.Resources["PrimaryButton"];
            SkipInterventionBtn.IsEnabled = true;
        }

        private async void StartQuickIntervention(Intervention intervention)
        {
            var result = await DisplayAlert($"🚀 {intervention.Title}", 
                $"{intervention.Description}\n\nCzas: {intervention.DurationSeconds} sekund", 
                "Start", "Anuluj");

            if (result)
            {
                // Ustaw wybraną interwencję jako aktualną
                InterventionTitleLabel.Text = intervention.Title;
                InterventionDescriptionLabel.Text = intervention.Description;
                InterventionTimeLabel.Text = $"{intervention.DurationSeconds}s";
                CountdownLabel.Text = intervention.DurationSeconds.ToString();
                _countdownTime = intervention.DurationSeconds;
                
                // Uruchom automatycznie
                OnStartIntervention(this, EventArgs.Empty);
            }
        }

        private void UpdateTodayCount()
        {
            // Symulacja - zwiększ licznik
            var parts = TodayCountLabel.Text.Split('/');
            if (int.TryParse(parts[0], out int current) && int.TryParse(parts[1], out int total))
            {
                if (current < total)
                {
                    TodayCountLabel.Text = $"{current + 1}/{total}";
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _interventionTimer?.Dispose();
        }

        public class Intervention
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int DurationSeconds { get; set; }

            public Intervention(string title, string description, int durationSeconds)
            {
                Title = title;
                Description = description;
                DurationSeconds = durationSeconds;
            }
        }
    }
}
