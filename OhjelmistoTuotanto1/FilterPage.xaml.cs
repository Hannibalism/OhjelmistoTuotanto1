using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using MySqlConnector;

namespace OhjelmistoTuotanto1
{
    public partial class FilterPage : ContentPage
    {
        // Lista kaikista m�keist�
        private List<Cottage> allCottages = new();
        private readonly DatabaseConnection dbConnection = new();

        public FilterPage()
        {
            InitializeComponent();
            PricePicker.ItemsSource = new List<string> { "Kaikki", "0�300 �", "301�500 �", "501�700 �", "Yli 700 �" };
            StatusPicker.ItemsSource = new List<string> { "Kaikki", "Vapaat", "Varatut" };
            PricePicker.SelectedIndex = 0;
            StatusPicker.SelectedIndex = 0;

            LoadCottagesFromDatabase();
        }

        // Lataa m�kkitiedot tietokannasta ja selvitt�� ovatko m�kit vapaita vai varattuja.
        private void LoadCottagesFromDatabase()
        {
            try
            {
                using var conn = dbConnection._getConnection();
                conn.Open();

                // SQL-kysely hakee tiedot
                string query = @"
                    SELECT 
                        m.mokkinimi AS name,
                        m.hinta AS price,
                        CASE 
                            WHEN NOT EXISTS (
                                SELECT 1 FROM varaus v
                                WHERE v.mokki_id = m.mokki_id
                                AND v.varattu_loppupvm >= NOW()
                            )
                            THEN TRUE
                            ELSE FALSE
                        END AS is_available
                    FROM mokki m;
                ";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                allCottages.Clear(); // Tyhjennet��n lista ennen uusien tietojen lataamista

                while (reader.Read())
                {
                    allCottages.Add(new Cottage
                    {
                        Name = reader.GetString("name"),
                        Price = Convert.ToInt32(reader.GetDouble("price")), // Py�ristet��n double -> int
                        IsAvailable = reader.GetBoolean("is_available")
                    });
                }

                // K�ynnistet��n suodatus heti datan lataamisen j�lkeen
                OnFilterChanged(null, null);
            }
            catch (Exception ex)
            {
                // N�ytet��n virheilmoitus k�ytt�j�lle
                DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }

        // Suodattaa m�kit k�ytt�j�n valintojen mukaan.
        private void OnFilterChanged(object sender, EventArgs e)
        {
            var priceIndex = PricePicker.SelectedIndex;
            var statusIndex = StatusPicker.SelectedIndex;

            var filtered = allCottages.Where(cottage =>
            {
                // Hintasuodatus
                bool priceMatch = priceIndex switch
                {
                    1 => cottage.Price <= 300,
                    2 => cottage.Price >= 301 && cottage.Price <= 500,
                    3 => cottage.Price >= 501 && cottage.Price <= 700,
                    4 => cottage.Price > 700,
                    _ => true
                };

                // Varaustilasuodatus
                bool statusMatch = statusIndex switch
                {
                    1 => cottage.IsAvailable,
                    2 => !cottage.IsAvailable,
                    _ => true
                };

                return priceMatch && statusMatch;
            }).ToList();

            CottagesCollection.ItemsSource = filtered;
        }
    }

    public class Cottage
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}