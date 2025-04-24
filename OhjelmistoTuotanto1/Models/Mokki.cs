using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhjelmistoTuotanto1.Models
{
    public class MokkiModel
    {
        public int MokkiId { get; set; }
        public int AlueId { get; set; }
        public string Postinro { get; set; }
        public string Toimipaikka { get; set; }
        public string Mokkinimi { get; set; }
        public string Katuosoite { get; set; }
        public double Hinta { get; set; }
        public string Kuvaus { get; set; }
        public int Henkilomaara { get; set; }
        public string Varustelu { get; set; }


        public override string ToString()
        {
            return $"{Mokkinimi} ({Katuosoite}, {Postinro} {Toimipaikka}) - {Hinta:C}";
        }
    }
}
