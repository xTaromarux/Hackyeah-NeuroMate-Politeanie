using SQLite;
using NeuroMate.Models;

namespace NeuroMate.Database.Entities
{
    public class Intervention
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Stored as string
        public int DurationSeconds { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string AvatarMessage { get; set; } = string.Empty;
        public int Priority { get; set; }
        
        [Ignore]
        public InterventionType TypeEnum 
        { 
            get => Enum.TryParse<InterventionType>(Type, out var result) ? result : InterventionType.CognitiveGame;
            set => Type = value.ToString();
        }
    }
}
