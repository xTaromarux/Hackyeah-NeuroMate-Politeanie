using NeuroMate.Models;
using NeuroMate.Database;

namespace NeuroMate.Services
{
    public class InterventionService : IInterventionService
    {
        private readonly List<Intervention> _interventions = new()
        {
            new Intervention
            {
                Name = "Ćwiczenie oddechowe",
                Description = "Oddychanie 4-7-8 na uspokojenie",
                Type = InterventionType.BreathingExercise,
                DurationSeconds = 120,
                Instructions = "Wdech przez 4s, zatrzymaj oddech na 7s, wydech przez 8s",
                AvatarMessage = "Zrobimy krótkie ćwiczenie oddechowe",
                Priority = 5
            },
            new Intervention
            {
                Name = "Reset oczu",
                Description = "Patrzenie w dal na odpoczęcie oczu",
                Type = InterventionType.EyeReset,
                DurationSeconds = 60,
                Instructions = "Spójrz przez okno w najdalszy punkt przez 20 sekund",
                AvatarMessage = "Twoje oczy potrzebują odpoczynku",
                Priority = 4
            },
            new Intervention
            {
                Name = "Mini-aktywność",
                Description = "Krótka aktywność fizyczna",
                Type = InterventionType.PhysicalActivity,
                DurationSeconds = 180,
                Instructions = "Wstań i rozciągnij się przez 3 minuty",
                AvatarMessage = "Czas na ruch!",
                Priority = 3
            }
        };

        private readonly DatabaseService _db;

        public InterventionService(DatabaseService db)
        {
            _db = db;
        }

        public Task<Intervention?> GetRecommendedInterventionAsync(int neuroScore, int minutesNoBreak, string userGoal)
        {
            Intervention? recommendation = null;

            if (minutesNoBreak > 120)
            {
                recommendation = _interventions.FirstOrDefault(i => i.Type == InterventionType.PhysicalActivity);
            }
            else if (neuroScore < 50)
            {
                recommendation = _interventions.FirstOrDefault(i => i.Type == InterventionType.BreathingExercise);
            }
            else if (minutesNoBreak > 60)
            {
                recommendation = _interventions.FirstOrDefault(i => i.Type == InterventionType.EyeReset);
            }

            return Task.FromResult(recommendation);
        }

        public Task<InterventionResult> ExecuteInterventionAsync(Intervention intervention)
        {
            var result = new InterventionResult
            {
                InterventionId = intervention.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddSeconds(intervention.DurationSeconds),
                Completed = true,
                ScoreBeforeIntervention = 0,
                ScoreAfterIntervention = 0
            };

            return Task.FromResult(result);
        }

        public List<Intervention> GetAllInterventions()
        {
            return _interventions.ToList();
        }

        public Task SaveInterventionResultAsync(InterventionResult result)
        {
            return Task.CompletedTask;
        }

        public Task<List<InterventionResult>> GetInterventionHistoryAsync(DateTime? from = null)
        {
            return Task.FromResult(new List<InterventionResult>());
        }
    }
}
