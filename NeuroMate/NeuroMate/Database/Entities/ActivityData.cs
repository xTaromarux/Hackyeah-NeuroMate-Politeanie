using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMate.Database.Entities
{
    public class ActivityData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; } // dzień, którego dane dotyczą

        public int StepsTotal { get; set; }
        public int ActiveMinutes { get; set; }
    }
}
