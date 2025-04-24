using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhjelmistoTuotanto1.Models
{
    public class Lasku
    {
        public int LaskuId { get; set; }
        public int VarausId { get; set; }
        public double Summa { get; set; }
        public double Alv { get; set; }
        public bool Maksettu { get; set; }
    }
}
