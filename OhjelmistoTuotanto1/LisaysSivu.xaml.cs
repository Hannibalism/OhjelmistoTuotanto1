using MySqlConnector;
using System.Data.Common;
using Windows.Media.AppBroadcasting;

namespace OhjelmistoTuotanto1;

public partial class LisaysSivu : ContentPage
{
    public LisaysSivu()
    {
        InitializeComponent();
    }

    //Luo yhteyden ja lisää tietoa tietokantaan, lisää alueen alue tauluun, tämän jälkeen postinumeron postitauluun ja ottaa toimipaikan alueesta.
    //sitten lisää loput tiedot mökki tauluun ja ottaa alue_id:n ja postinumeron alueen perusteella ja lisää loput tiedot.
    //Toistaiseksi kaatuu jos duplicate tietoa koitetaan lisätä.
    private async void LisaysNappi(object sender, EventArgs e)
    {
        string mokkinimi = Mökkinimi.Text;
        string postinro = Postinro.Text;
        string katuosoite = Katuosoite.Text;
        double hinta = double.Parse(Hinta.Text);
        string kuvaus = Kuvaus.Text;
        string varustelu = Varustelu.Text;
        string nimi = Alue.Text;
        await InsertData(nimi, mokkinimi, postinro, katuosoite, hinta, kuvaus, varustelu);

    }
        private async Task InsertData(string nimi, string mokkinimi, string postinro, string katuosoite, double hinta, string kuvaus, string varustelu)
        {
            DatabaseConnection dbc = new DatabaseConnection();
            using (var connection = dbc._getConnection())
            {
                connection.Open();

            string query = "INSERT INTO vn.alue (nimi) VALUES (@nimi); " +
               "INSERT INTO vn.posti (postinro, toimipaikka) VALUES (@postinro, @nimi); " +
               "INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, varustelu) " +
               "VALUES ((SELECT alue_id FROM vn.alue WHERE nimi = @nimi), " +
               "(SELECT postinro FROM vn.posti WHERE toimipaikka = " +
               "(SELECT toimipaikka FROM vn.alue WHERE nimi = @nimi)), @mokkinimi, @katuosoite, @hinta, @kuvaus, @varustelu);";
            using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@postinro", postinro);
                    command.Parameters.AddWithValue("@nimi", nimi);
                    command.Parameters.AddWithValue("@mokkinimi", mokkinimi);
                    command.Parameters.AddWithValue("@katuosoite", katuosoite);
                    command.Parameters.AddWithValue("@hinta", hinta);
                    command.Parameters.AddWithValue("@kuvaus", kuvaus);
                    command.Parameters.AddWithValue("@varustelu", varustelu);
                    await command.ExecuteNonQueryAsync();
                }

                await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK");
            }
        }

    }