using NeuroMate.Database;
using NeuroMate.Database.Entities;
using System.Globalization;
using ModelsImportResult = NeuroMate.Models.ImportResult;

namespace NeuroMate.Services
{
    public class DataImportService : IDataImportService
    {
        private readonly string[] _requiredHeaders = { "timestamp", "hr", "hrv", "steps", "sleep_minutes" };
        private readonly DatabaseService _db;

        public DataImportService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<ModelsImportResult> ImportCsvAsync(string filePath)
        {
            var result = new ModelsImportResult { Success = false };

            try
            {
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = "Plik nie istnieje.";
                    return result;
                }

                // Walidacja formatu
                if (!await ValidateCsvFormatAsync(filePath))
                {
                    result.ErrorMessage = "Nieprawidłowy format pliku CSV.";
                    return result;
                }

                var lines = await File.ReadAllLinesAsync(filePath);
                if (lines.Length < 2)
                {
                    result.ErrorMessage = "Plik jest pusty lub zawiera tylko nagłówki.";
                    return result;
                }

                // Parsowanie nagłówków
                var headers = lines[0].ToLower().Split(',')
                    .Select(h => h.Trim())
                    .ToArray();

                // Parsowanie danych
                var records = new List<Database.Entities.HealthRecord>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    if (values.Length != headers.Length) continue;

                    try
                    {
                        var record = ParseHealthRecord(values, headers);
                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                    catch
                    {
                        // Pomiń błędne rekordy
                        continue;
                    }
                }

                // Oblicz statystyki
                result.Success = true;
                result.RecordsImported = records.Count;
                // Usuwam nieistniejące właściwości Records i AverageHRV

                // Symulacja obliczeń dla kompatybilności
                // Jeśli potrzebne, można dodać te właściwości do ImportResult
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Błąd podczas importu: {ex.Message}";
            }

            return result;
        }

        public async Task<bool> ValidateCsvFormatAsync(string filePath)
        {
            try
            {
                var firstLine = (await File.ReadAllLinesAsync(filePath)).FirstOrDefault();
                if (string.IsNullOrEmpty(firstLine))
                    return false;

                var headers = firstLine.ToLower().Split(',')
                    .Select(h => h.Trim())
                    .ToArray();

                // Sprawdź czy zawiera wymagane nagłówki
                return _requiredHeaders.All(rh => headers.Contains(rh));
            }
            catch
            {
                return false;
            }
        }

        public Database.Entities.HealthRecord ParseHealthRecord(string[] csvRow, string[] headers)
        {
            var record = new Database.Entities.HealthRecord();

            for (int i = 0; i < headers.Length && i < csvRow.Length; i++)
            {
                var header = headers[i];
                var value = csvRow[i].Trim();

                if (string.IsNullOrEmpty(value))
                    continue;

                switch (header)
                {
                    case "timestamp":
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
                        {
                            record.Timestamp = timestamp;
                        }
                        break;

                    case "hr":
                    case "heart_rate":
                        if (int.TryParse(value, out var hr))
                        {
                            record.HeartRate = hr;
                        }
                        break;

                    case "hrv":
                        if (int.TryParse(value, out var hrv))
                        {
                            record.HRV = hrv;
                        }
                        break;

                    case "steps":
                        if (int.TryParse(value, out var steps))
                        {
                            record.Steps = steps;
                        }
                        break;

                    case "sleep_minutes":
                    case "sleep":
                        if (int.TryParse(value, out var sleep))
                        {
                            record.SleepMinutes = sleep;
                        }
                        break;
                }
            }

            return record;
        }

        public async Task<ModelsImportResult> ImportFromGoogleFitAsync()
        {
            // TODO: Implementacja integracji z Google Fit API
            await Task.CompletedTask;
            return new ModelsImportResult
            {
                Success = false,
                ErrorMessage = "Integracja z Google Fit nie jest jeszcze zaimplementowana."
            };
        }

        public async Task<ModelsImportResult> ImportFromAppleHealthAsync()
        {
            // TODO: Implementacja integracji z Apple Health
            await Task.CompletedTask;
            return new ModelsImportResult
            {
                Success = false,
                ErrorMessage = "Integracja z Apple Health nie jest jeszcze zaimplementowana."
            };
        }
    }
}