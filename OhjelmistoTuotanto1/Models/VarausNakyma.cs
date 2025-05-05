using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhjelmistoTuotanto1.Models
{
    public class VarausNakyma
    {
        public int VarausId { get; set; }
        public string AsiakasNimi { get; set; }
        public string MokkiNimi { get; set; }

        public override string ToString()
        {
            return $"{AsiakasNimi} – {MokkiNimi}";
        }
    }
}
