using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMate.Database.Entities
{
    using SQLite;
    using System;

    public class SleepData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalSleepMinutes { get; set; }
        public double SleepEfficiency { get; set; }
        public int RemMinutes { get; set; }
        public int DeepMinutes { get; set; }
        public int AwakeningsCount { get; set; }
    }

}
