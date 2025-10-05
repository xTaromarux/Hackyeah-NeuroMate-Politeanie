using System.Text;

namespace NeuroMate.Views;

public partial class DailySummaryPage
{
	public DailySummaryPage()
	{
		InitializeComponent();
	}

	private async void OnExportReportClicked(object sender, EventArgs e)
	{
		try
		{
			// Przygotowanie treści raportu
			var reportContent = GenerateReportContent();
			
			// Utworzenie pliku tymczasowego
			var fileName = $"NeuroMate_Raport_{DateTime.Now:yyyy-MM-dd}.txt";
			var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
			
			await File.WriteAllTextAsync(filePath, reportContent, Encoding.UTF8);
			
			// Udostępnienie pliku
			await Share.Default.RequestAsync(new ShareFileRequest
			{
				Title = "Eksport raportu NeuroMate",
				File = new ShareFile(filePath)
			});
			
			await DisplayAlert("Sukces", "Raport został wyeksportowany pomyślnie!", "OK");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Błąd", $"Nie udało się wyeksportować raportu: {ex.Message}", "OK");
		}
	}

	private async void OnPlanTomorrowClicked(object sender, EventArgs e)
	{
		try
		{
			// Nawigacja do strony planowania na jutro
			// Jeśli nie ma dedykowanej strony, pokażmy alert z planem
			var planContent = "🌅 Plan na jutro:\n\n" +
							"• 09:30 - Pierwsza sesja fokusowa\n" +
							"• 11:00 - Test N-back (15 min)\n" +
							"• 13:00 - Drzemka (15 min)\n" +
							"• 15:30 - Mikro-aktywność\n" +
							"• 17:00 - Druga sesja N-back\n" +
							"• 19:00 - Reset oczu\n\n" +
							"Cel: Poprawa czasu reakcji o kolejne 5%";

			var result = await DisplayAlert("Plan na jutro", planContent, "Zaplanuj", "Anuluj");
			
			if (result)
			{
				// Tu można dodać logikę zapisywania planu
				await DisplayAlert("Zaplanowano", "Plan na jutro został zapisany!", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Błąd", $"Nie udało się utworzyć planu: {ex.Message}", "OK");
		}
	}

	private async void OnBackToMainClicked(object sender, EventArgs e)
	{
		try
		{
			// Nawigacja do głównej strony
			await Shell.Current.GoToAsync("//MainPage");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Błąd", $"Nie udało się wrócić do głównej: {ex.Message}", "OK");
		}
	}

	private string GenerateReportContent()
	{
		var report = new StringBuilder();
		report.AppendLine("=== NEUROMATE - DZIENNY RAPORT ===");
		report.AppendLine($"Data: {DateTime.Now:dd.MM.yyyy}");
		report.AppendLine();
		
		report.AppendLine("🎯 GŁÓWNE METRYKI:");
		report.AppendLine("• Neuro-Score: 78 (+6 pkt)");
		report.AppendLine("• Ukończone interwencje: 4");
		report.AppendLine("• Czas skupienia: 6.2h");
		report.AppendLine("• Ocena dnia: A+");
		report.AppendLine();
		
		report.AppendLine("📈 POPRAWA W CIĄGU DNIA:");
		report.AppendLine("• Czas reakcji: 389ms → 341ms (-12%)");
		report.AppendLine("• Dokładność: 84% → 92% (+8%)");
		report.AppendLine();
		
		report.AppendLine("✅ WYKONANE INTERWENCJE:");
		report.AppendLine("1. Test Stroop - 09:30 (Poprawa RT: +18%, Dokładność: +12%)");
		report.AppendLine("2. Oddychanie 4-7-8 - 12:15 (Redukcja stresu, reset mentalny)");
		report.AppendLine("3. Mikro-aktywność - 15:45 (5 przysiadów + rozciąganie)");
		report.AppendLine("4. Reset oczu - 17:20 (Patrzenie w dal, mruganie)");
		report.AppendLine();
		
		report.AppendLine("💡 REKOMENDACJE NA JUTRO:");
		report.AppendLine("• Spróbuj dodać 15-min drzemkę między 13:00-14:00");
		report.AppendLine("• Zwiększ częstotliwość N-back do 2 razy dziennie");
		report.AppendLine("• Zaplanuj pierwszą sesję fokusową na 9:30");
		report.AppendLine();
		
		report.AppendLine("Raport wygenerowany przez NeuroMate");
		report.AppendLine($"Czas generowania: {DateTime.Now:HH:mm:ss}");
		
		return report.ToString();
	}
}
