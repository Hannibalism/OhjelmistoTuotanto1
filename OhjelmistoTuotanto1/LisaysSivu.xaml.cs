using Microsoft.Maui.Controls;
using MySqlConnector;
using Org.BouncyCastle.Asn1.Cms;
using System.Data.Common;
using OhjelmistoTuotanto1.Data;
using System.Collections.ObjectModel;
using OhjelmistoTuotanto1.Models;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Maui.ApplicationModel.Communication;

namespace OhjelmistoTuotanto1;

public partial class LisaysSivu : ContentPage
{
    public ObservableCollection<Mokki> MokkiList { get; set; }

    public LisaysSivu()
    {
        InitializeComponent();
        MokkiList = new ObservableCollection<Mokki>();
        LoadData();
        BindingContext = this;
    }

    private void LoadData()
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand("SELECT * FROM vn.mokki", connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                MokkiList.Add(new Mokki
                {
                    MokkiID = reader.GetInt32("mokki_id"),
                    Mokkinimi = reader.GetString("mokkinimi"),
                    Katuosoite = reader.GetString("katuosoite"),
                    Postinro = reader.GetString("postinro"),
                    Hinta = reader.GetDouble("hinta"),
                    Kuvaus = reader.GetString("kuvaus"),
                    Henkilomaara = reader.GetInt32("henkilomaara"),
                    Varustelu = reader.GetString("varustelu")
                });
            }
        }

    }
    public void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedMokki = e.CurrentSelection[0] as Mokki;
            if (selectedMokki != null)
            {
                mokkiID.Text = selectedMokki.MokkiID.ToString();
                Mokkinimi.Text= selectedMokki.Mokkinimi;
                Postinro.Text = selectedMokki.Postinro;
                Hinta.Text = selectedMokki.Hinta.ToString();
                Katuosoite.Text = selectedMokki.Katuosoite;
                Kuvaus.Text = selectedMokki.Kuvaus;
                Henkilomaara.Text = selectedMokki.Henkilomaara.ToString();
                Varustelu.Text = selectedMokki.Varustelu;
            }
        }
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
        if (mokkiID.Text == null || mokkiID.Text == string.Empty)
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
                        await DisplayAlert("Oho", "Alue on jo olemassa, käytetään olemassa olevaa tietoa.", "Ok");
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
                        await DisplayAlert("Oho", "Postinumero on jo kannassa, käytetään olevaa tietoa", "ok");
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
                string addMokki = "INSERT INTO vn.mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) VALUES ((SELECT alue_id from vn.alue where nimi = @nimi), (SELECT postinro from vn.posti where postinro = @postinro), @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu); SELECT LAST_INSERT_ID();";
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
                    var newId = await command.ExecuteScalarAsync();
                    MokkiList.Add(new Mokki
                    {
                        MokkiID = Convert.ToInt32(newId),
                        Mokkinimi = mokkinimi,
                        Katuosoite = katuosoite,
                        Postinro = postinro,
                        Hinta = hinta,
                        Kuvaus = kuvaus,
                        Henkilomaara = henkilomaara,
                        Varustelu = varustelu
                    });
                }
            }
        }
        else if (mokkiID.Text != null || mokkiID.Text != string.Empty)
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
                        await DisplayAlert("Oho", "Alue on jo olemassa, käytetään olemassa olevaa tietoa.", "Ok");
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
                string PaivitaMokki = "UPDATE vn.mokki SET postinro = @postinro, mokkinimi = @mokkinimi, katuosoite = @katuosoite, hinta = @hinta, kuvaus = @kuvaus, henkilomaara = @henkilomaara WHERE mokki_id = @MokkiID;";
                using (var Command = new MySqlCommand(PaivitaMokki, connection))
                {
                    Command.Parameters.AddWithValue("@MokkiID", int.Parse(mokkiID.Text));
                    Command.Parameters.AddWithValue("@mokkinimi", mokkinimi);
                    Command.Parameters.AddWithValue("@katuosoite", katuosoite);
                    Command.Parameters.AddWithValue("@postinro", postinro);
                    Command.Parameters.AddWithValue("@kuvaus", kuvaus);
                    Command.Parameters.AddWithValue("@henkilomaara", henkilomaara);
                    Command.Parameters.AddWithValue("@hinta", hinta);
                    Command.ExecuteScalar();

                }
            }
        }
    }
    public class Mokki
    {
        public int MokkiID { get; set; }
        public string Mokkinimi { get; set; }
        public string Katuosoite { get; set; }
        public string Postinro { get; set; }
        public double Hinta { get; set; }
        public string Kuvaus { get; set; }
        public int Henkilomaara { get; set; }
        public string Varustelu { get; set; }


    }
}