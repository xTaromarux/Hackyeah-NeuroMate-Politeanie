using NeuroMate.Database;
using NeuroMate.Database.Entities;

namespace NeuroMate
{
    public partial class SleepPage : ContentPage
    {
        private DatabaseService _db;

        public SleepPage()
        {
            InitializeComponent();
            _db = new DatabaseService();
            LoadData();
        }

        private async void LoadData()
        {
            var records = await _db.GetAllSleepDataAsync();
            SleepCollectionView.ItemsSource = records;
        }

        private async void AddRecord_Clicked(object sender, EventArgs e)
        {
            var record = new SleepData
            {
                Date = DateTime.Now,
                TotalSleepMinutes = int.Parse(TotalMinutesEntry.Text),
                SleepEfficiency = double.Parse(EfficiencyEntry.Text),
                RemMinutes = int.Parse(RemEntry.Text),
                DeepMinutes = int.Parse(DeepEntry.Text),
                AwakeningsCount = int.Parse(AwakeningsEntry.Text)
            };

            await _db.SaveSleepDataAsync(record);
            LoadData();
        }

        private async void DeleteRecord_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is SleepData record)
            {
                await _db.DeleteSleepDataAsync(record);
                LoadData();
            }
        }
    }
}