using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMate.Database.Entities
{
    public class HeartData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; } // dzień lub noc, którego dane dotyczą

        public int? HrRest { get; set; } // uderzenia na minutę, nullable jeśli brak danych
        public double? HrvRmssd { get; set; } // ms, nullable jeśli brak danych
    }
}
