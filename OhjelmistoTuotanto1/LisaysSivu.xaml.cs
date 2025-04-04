using Microsoft.Maui.Controls;
using MySqlConnector;
using Org.BouncyCastle.Asn1.Cms;
using System.Data.Common;
using Windows.Media.AppBroadcasting;

namespace OhjelmistoTuotanto1;

public partial class LisaysSivu : ContentPage
{
    public LisaysSivu()
    {
        InitializeComponent();
    }

        //Luo yhteyden tietokantaan ja tarkistaa ensin onko tiedot jo tietokannassa, jos ei niin lisää ne tietokantaan.
        //Jos tiedot ovat tietokannassa ponnahdus ikkuna aukeaa ja kertoo.
        //Tämän jälkeen lisää kaikki tiedot mökkitauluun, alue_id ja postinro tulevat posti ja alue tauluista haettuina.
        //Mökkitaulun omat tiedot lisätään suoraan tekstikentistä.
    private async void LisaysNappi(object sender, EventArgs e)
    {
        string mokkinimi = Mokkinimi.Text;
        string postinro = Postinro.Text;
        string toimipaikka = Toimipaikka.Text;
        string katuosoite = Katuosoite.Text;
        double hinta;
        string kuvaus = Kuvaus.Text;
        string varustelu = Varustelu.Text;
        int henkilomaara;
        string nimi = Alue.Text;

        if (mokkinimi.Length > 45) 
        {
            await DisplayAlert("Virhe", "Mökinnimi saa olla max 45 merkkiä", "Ok");
            return;
        }
        if (katuosoite.Length > 45) 
        {
            await DisplayAlert("Virhe", "Katuosoite saa olla max 45 merkkiä", "Ok");
            return;
        }

        if (Postinro.Text.Length != 5)
        {
            await DisplayAlert("Virhe", "Postinumerossa on oltava 5 merkkiä", "Ok");
            return;
        }
        if (!double.TryParse(Hinta.Text, out hinta)) 
        {
            await DisplayAlert("Virhe", "Hinnan täytyy olla numero", "Ok");
            return;
        }
        if (!int.TryParse(Henkilomaara.Text, out henkilomaara)) 
        {
            await DisplayAlert("Virhe", "Henkilömäärän täytyy olla kokonaisluku", "Ok");
            return;
        }


        await InsertData(nimi, mokkinimi, katuosoite, hinta, kuvaus, varustelu, postinro, toimipaikka, henkilomaara);

    }
    private async Task InsertData(string nimi, string mokkinimi, string katuosoite, double hinta, string kuvaus, string varustelu, string postinro, string toimipaikka, int henkilomaara)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();

            string aluetarkistus = "SELECT COUNT(*) FROM vn.alue WHERE nimi = @nimi;";
            using (var command = new MySqlCommand(aluetarkistus, connection))
            {
                command.Parameters.AddWithValue("@nimi", nimi);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                if (count > 0)
                {
                    await DisplayAlert("Oho", "Alue on jo olemassa", "Ok");
                }
                else if (count == 0)
                {
                    string addAlue = "INSERT INTO vn.alue (nimi) VALUES (@nimi);";
                    using (var insertCommand = new MySqlCommand(addAlue, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@nimi", nimi);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }

            }
            string postinroTarkistus = "SELECT COUNT(*) from vn.posti where postinro = @postinro;";
            using (var command = new MySqlCommand(postinroTarkistus, connection))
            {
                command.Parameters.AddWithValue("@postinro", postinro);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                if (count > 0)
                {
                    await DisplayAlert("Hups", "Postinumero on jo kannassa", "ok");
                }
                else if (count == 0)
                {
                    string addPostinro = "INSERT INTO vn.posti (postinro, toimipaikka) VALUES (@postinro, @toimipaikka);";
                    using (var insertCommand = new MySqlCommand(addPostinro, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@postinro", postinro);
                        insertCommand.Parameters.AddWithValue("@toimipaikka", toimipaikka);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            string addMokki = "INSERT INTO vn.mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) VALUES ((SELECT alue_id from vn.alue where nimi = @nimi), (SELECT postinro from vn.posti where postinro = @postinro), @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu);";
            using (var command = new MySqlCommand(addMokki, connection)) 
            {
                command.Parameters.AddWithValue("@nimi", nimi);
                command.Parameters.AddWithValue("@postinro", postinro);
                command.Parameters.AddWithValue("@mokkinimi", mokkinimi);
                command.Parameters.AddWithValue("@katuosoite", katuosoite);
                command.Parameters.AddWithValue("@hinta", hinta);
                command.Parameters.AddWithValue("@kuvaus", kuvaus);
                command.Parameters.AddWithValue("@henkilomaara", henkilomaara);
                command.Parameters.AddWithValue("@varustelu", varustelu);
                command.ExecuteNonQuery();
            }
        }
    }
}