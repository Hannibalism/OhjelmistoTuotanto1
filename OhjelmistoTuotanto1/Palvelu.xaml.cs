using Microsoft.Maui.Controls;
using MySqlConnector;
using Org.BouncyCastle.Asn1.Cms;
using System.Data.Common;
using OhjelmistoTuotanto1.Data;
using OhjelmistoTuotanto1.Models;

namespace OhjelmistoTuotanto1;

public partial class PalveluSivu : ContentPage
{
    public PalveluSivu()
    {
        InitializeComponent();
        LoadPalvelut();
    }

    private async void Poista_Clicked(object sender, EventArgs e)
    {
        var selectedPalvelu = palveluPicker.SelectedItem as Palvelu;

        if (selectedPalvelu == null)
        {
            await DisplayAlert("Virhe", "Valitse poistettava palvelu.", "Ok");
            return;
        }

        bool confirm = await DisplayAlert("Vahvista poisto",
            $"Haluatko varmasti poistaa palvelun \"{selectedPalvelu.Nimi}\"?", "Kyllä", "Peruuta");

        if (!confirm) return;

        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            await connection.OpenAsync();

            // 1. Poista ensin viittaukset varauksen_palvelut -taulusta
            string deleteDependencies = "DELETE FROM vn.varauksen_palvelut WHERE palvelu_id = @id";
            using (var deleteCommand = new MySqlCommand(deleteDependencies, connection))
            {
                deleteCommand.Parameters.AddWithValue("@id", selectedPalvelu.PalveluId);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            // 2. Poista palvelu-taulusta
            string deleteQuery = "DELETE FROM vn.palvelu WHERE palvelu_id = @id";
            using (var command = new MySqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@id", selectedPalvelu.PalveluId);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Tyhjennä käyttöliittymä ja päivitä
        Palvelunimi.Text = "";
        Kuvaus.Text = "";
        Hinta.Text = "";
        Alv.Text = "";
        Alue.Text = "";
        palveluPicker.SelectedItem = null;

        await DisplayAlert("Poistettu", "Palvelu poistettiin onnistuneesti.", "Ok");

        LoadPalvelut();
    }
    private async void palvelulisays_Clicked(object sender, EventArgs e)
    {
        var selectedPalvelu = palveluPicker.SelectedItem as Palvelu;

        string palvelunimi = Palvelunimi.Text;
        string kuvaus = Kuvaus.Text;
        double hinta, alv;
        string aluenimi = Alue.Text;

        if (string.IsNullOrWhiteSpace(palvelunimi) ||
      string.IsNullOrWhiteSpace(kuvaus) ||
      string.IsNullOrWhiteSpace(Hinta.Text) ||
      string.IsNullOrWhiteSpace(Alv.Text) ||
      string.IsNullOrWhiteSpace(aluenimi))
        {
            await DisplayAlert("Virhe", "Kaikki kentät täytyy täyttää.", "Ok");
            return;
        }

        if (palvelunimi.Length > 45)
        {
            await DisplayAlert("Virhe", "Palvelun nimi saa olla max 45 merkkiä", "Ok");
            return;
        }
        if (!double.TryParse(Hinta.Text, out hinta))
        {
            await DisplayAlert("Virhe", "Hinnan täytyy olla numero", "Ok");
            return;
        }
        if (!double.TryParse(Alv.Text, out alv))
        {
            await DisplayAlert("Virhe", "Alv täytyy olla numero", "Ok");
            return;
        }
        if (selectedPalvelu != null)
        {
            await UpdateData(selectedPalvelu.PalveluId, aluenimi, palvelunimi, kuvaus, hinta, alv);
        }
        else
        {
            await InsertData(aluenimi, palvelunimi, kuvaus, hinta, alv);
        }

    }
    private async Task UpdateData(int palveluId, string aluenimi, string palvelunimi, string kuvaus, double hinta, double alv)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            await connection.OpenAsync();

            string aluetarkistus = "SELECT COUNT(*) FROM vn.alue WHERE nimi = @nimi;";
            using (var command = new MySqlCommand(aluetarkistus, connection))
            {
                command.Parameters.AddWithValue("@nimi", aluenimi);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                if (count == 0)
                {
                    string addAlue = "INSERT INTO vn.alue (nimi) VALUES (@nimi);";
                    using (var insertCommand = new MySqlCommand(addAlue, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@nimi", aluenimi);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            string updatePalvelu = "UPDATE vn.palvelu SET alue_id = (SELECT alue_id FROM vn.alue WHERE nimi = @nimi), nimi = @palvelunimi, kuvaus = @kuvaus, hinta = @hinta, alv = @alv WHERE palvelu_id = @id";
            using (var command = new MySqlCommand(updatePalvelu, connection))
            {
                command.Parameters.AddWithValue("@nimi", aluenimi);
                command.Parameters.AddWithValue("@palvelunimi", palvelunimi);
                command.Parameters.AddWithValue("@kuvaus", kuvaus);
                command.Parameters.AddWithValue("@hinta", hinta);
                command.Parameters.AddWithValue("@alv", alv);
                command.Parameters.AddWithValue("@id", palveluId);
                await command.ExecuteNonQueryAsync();



                Palvelunimi.Text = "";
                Kuvaus.Text = "";
                Hinta.Text = "";
                Alv.Text = "";
                Alue.Text = "";
                palveluPicker.SelectedItem = null;

                LoadPalvelut();
            }
        }
    }
    private async Task InsertData(string aluenimi, string palvelunimi, string kuvaus, double hinta, double alv)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();

            string aluetarkistus = "SELECT COUNT(*) FROM vn.alue WHERE nimi = @nimi;";
            using (var command = new MySqlCommand(aluetarkistus, connection))
            {
                command.Parameters.AddWithValue("@nimi", aluenimi);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                if (count > 0)
                {
                    await DisplayAlert("Oho", "Alue on jo olemassa", "Ok");
                }
                else if (count == 0)
                {
                    string addAlue = "INSERT INTO vn.alue (aluenimi) VALUES (@nimi);";
                    using (var insertCommand = new MySqlCommand(addAlue, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@nimi", aluenimi);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            string addPalvelu = "INSERT INTO vn.palvelu (alue_id, nimi, kuvaus, hinta, alv) " +
                "VALUES ((SELECT alue_id FROM vn.alue WHERE nimi = @nimi), @palvelunimi, @kuvaus, @hinta, @alv);";
            using (var command = new MySqlCommand(addPalvelu, connection))
            {
                command.Parameters.AddWithValue("@nimi", aluenimi);
                command.Parameters.AddWithValue("@palvelunimi", palvelunimi);
                command.Parameters.AddWithValue("@kuvaus", kuvaus);
                command.Parameters.AddWithValue("@hinta", hinta);
                command.Parameters.AddWithValue("@alv", alv);
                await command.ExecuteNonQueryAsync();

                Palvelunimi.Text = "";
                Kuvaus.Text = "";
                Hinta.Text = "";
                Alv.Text = "";
                Alue.Text = "";

                LoadPalvelut();
            }
        }

    }
    private async void LoadPalvelut()
    {

        DatabaseConnection dbc = new DatabaseConnection();
        var palvelut = new List<Palvelu>();

        using (var connection = dbc._getConnection())
        {
            await connection.OpenAsync();
            string query = "SELECT palvelu_id, alue_id, nimi, kuvaus, hinta, alv FROM vn.palvelu";
            using (var command = new MySqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    palvelut.Add(new Palvelu
                    {
                        PalveluId = reader.GetInt32(0),
                        AlueId = reader.GetInt32(1),
                        Nimi = reader.GetString(2),
                        Kuvaus = reader.GetString(3),
                        Hinta = reader.GetDouble(4),
                        Alv = reader.GetDouble(5)
                    });
                }
            }
        }
        palveluPicker.ItemsSource = palvelut;
    }
    private async void PalveluPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedPalvelu = picker.SelectedItem as Palvelu;

        if (selectedPalvelu != null)
        {
            Palvelunimi.Text = selectedPalvelu.Nimi;
            Kuvaus.Text = selectedPalvelu.Kuvaus;
            Hinta.Text = selectedPalvelu.Hinta.ToString();
            Alv.Text = selectedPalvelu.Alv.ToString();

            // Haetaan alueen nimi tietokannasta
            DatabaseConnection dbc = new DatabaseConnection();
            using (var connection = dbc._getConnection())
            {
                await connection.OpenAsync();

                string alueQuery = "SELECT nimi FROM vn.alue WHERE alue_id = @alue_id";
                using (var command = new MySqlCommand(alueQuery, connection))
                {
                    command.Parameters.AddWithValue("@alue_id", selectedPalvelu.AlueId);
                    var result = await command.ExecuteScalarAsync();
                    Alue.Text = result?.ToString() ?? "";
                }
            }
        }
    }
}