using MySqlConnector;
using System.Data.Common;

namespace OhjelmistoTuotanto1;

public partial class LisaysSivu : ContentPage
{
    public LisaysSivu()
    {
        InitializeComponent();
    }

    //Luo yhteyden ja lisää tietoa tietokantaan.
    //ei toimia vielä oikein alue_id:n takia.
    private async void LisaysNappi(object sender, EventArgs e)
    {
        string mokkinimi = Mökkinimi.Text;
        string postinro = Postinro.Text;
        string katuosoite = Katuosoite.Text;
        double hinta = double.Parse(Hinta.Text);
        string kuvaus = Kuvaus.Text;
        string varustelu = Varustelu.Text;

        await InsertData(mokkinimi, postinro, katuosoite, hinta, kuvaus, varustelu);

    }
    private async Task InsertData(string mokkinimi, string postinro, string katuosoite, double hinta, string kuvaus, string varustelu)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            string query = "INSERT INTO vn.mokki (mokkinimi, postinro, katuosoite, hinta, kuvaus, varustelu) VALUES (@mokkinimi, @postinro, @katuosoite, @hinta, @kuvaus, @varustelu)";
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@mokkinimi", mokkinimi);
                command.Parameters.AddWithValue("@postinro", postinro);
                command.Parameters.AddWithValue("@katuosoite", katuosoite);
                command.Parameters.AddWithValue("@hinta", hinta);
                command.Parameters.AddWithValue("@kuvaus", kuvaus);
                command.Parameters.AddWithValue("@varustelu", varustelu);

                await command.ExecuteNonQueryAsync();
                await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK");
            }
        }
    }
}