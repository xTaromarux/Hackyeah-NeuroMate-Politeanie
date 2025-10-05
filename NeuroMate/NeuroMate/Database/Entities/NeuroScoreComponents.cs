using SQLite;
using System;

namespace NeuroMate.Database.Entities
{
    public class NeuroScoreComponents
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double ReactionTimeScore { get; set; }
        public double AccuracyScore { get; set; }
        public double BreakTimeScore { get; set; }
        public double HRVScore { get; set; }
        public double SleepScore { get; set; }
        public string WeightsJson { get; set; } = string.Empty;
    }
}
