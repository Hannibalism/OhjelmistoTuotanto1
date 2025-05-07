using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using MySqlConnector;
using OhjelmistoTuotanto1.Data;

namespace OhjelmistoTuotanto1
{
    public partial class FilterPage : ContentPage
    {
        private List<Cottage> allCottages = new();
        private readonly DatabaseConnection dbConnection = new();

        public FilterPage()
        {
            InitializeComponent();
            PricePicker.ItemsSource = new List<string> { "Kaikki", "0–300 €", "301–500 €", "501–700 €", "Yli 700 €" };
            StatusPicker.ItemsSource = new List<string> { "Kaikki", "Vapaat", "Varatut" };
            PricePicker.SelectedIndex = 0;
            StatusPicker.SelectedIndex = 0;

            LoadCottagesFromDatabase();
        }

        private void LoadCottagesFromDatabase()
        {
            try
            {
                using var conn = dbConnection._getConnection();
                conn.Open();

                string query = @"
                    SELECT 
                        m.mokkinimi AS name,
                        m.hinta AS price,
                        a.nimi AS location,
                        CASE 
                            WHEN NOT EXISTS (
                                SELECT 1 FROM varaus v
                                WHERE v.mokki_id = m.mokki_id
                                AND v.varattu_loppupvm >= NOW()
                            )
                            THEN TRUE
                            ELSE FALSE
                        END AS is_available
                    FROM mokki m
                    JOIN alue a ON m.alue_id = a.alue_id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                allCottages.Clear();

                while (reader.Read())
                {
                    allCottages.Add(new Cottage
                    {
                        Name = reader.GetString("name"),
                        Price = Convert.ToInt32(reader.GetDouble("price")),
                        IsAvailable = reader.GetBoolean("is_available"),
                        Location = reader.GetString("location")
                    });
                }

                OnFilterChanged(null, null);
            }
            catch (Exception ex)
            {
                DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            var priceIndex = PricePicker.SelectedIndex;
            var statusIndex = StatusPicker.SelectedIndex;
            string searchText = LocationSearchBar?.Text?.ToLower() ?? "";

            var filtered = allCottages.Where(cottage =>
            {
                bool priceMatch = priceIndex switch
                {
                    1 => cottage.Price <= 300,
                    2 => cottage.Price >= 301 && cottage.Price <= 500,
                    3 => cottage.Price >= 501 && cottage.Price <= 700,
                    4 => cottage.Price > 700,
                    _ => true
                };

                bool statusMatch = statusIndex switch
                {
                    1 => cottage.IsAvailable,
                    2 => !cottage.IsAvailable,
                    _ => true
                };

                bool locationMatch = string.IsNullOrWhiteSpace(searchText) ||
                                     cottage.Location.ToLower().Contains(searchText);

                return priceMatch && statusMatch && locationMatch;
            }).ToList();

            CottagesCollection.ItemsSource = filtered;
        }
    }

    public class Cottage
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string Location { get; set; }

        public string StatusText
        {
            get
            {
                return IsAvailable ? "Vapaa" : "Varattu";
            }
        }
    }
}
