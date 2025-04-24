using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhjelmistoTuotanto1.Models
{
    public class Asiakas
    {
        public int AsiakasId { get; set; }
        public string Postinro { get; set; }
        public string Etunimi { get; set; }
        public string Sukunimi { get; set; }
        public string Lahiosoite { get; set; }
        public string Email { get; set; }
        public string Puhelinnro { get; set; }

        public string Kokonimi => $"{Etunimi} {Sukunimi}";
        public override string ToString()
        {
            return Kokonimi;
        }
    }
}
