using Microsoft.Maui.Controls;
using MySqlConnector;
using Org.BouncyCastle.Asn1.Cms;
using System.Data.Common;
using OhjelmistoTuotanto1.Data;
using System.Collections.ObjectModel;
using OhjelmistoTuotanto1.Models;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Maui.ApplicationModel.Communication;
using System.ComponentModel;
using Org.BouncyCastle.Security;
using Google.Protobuf.WellKnownTypes;

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
        LoadAlueData();

    }

    private void AluehallintaBtn(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Aluehallinta());
    }

    private async void LoadAlueData()
    {
        var alueNames = await GetAlueNamesAsync();
        aluePicker.ItemsSource = alueNames;
    }
    private async Task<List<string>> GetAlueNamesAsync()
    {
        var alueNames = new List<string>();


        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            {
                string SelectAlue = "SELECT nimi FROM vn.alue";
                var command = new MySqlCommand(SelectAlue, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            alueNames.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return alueNames;
        }
    }
    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        var selectedAlueName = picker.SelectedItem as string;
        HiddenEntry3.Text = selectedAlueName;

    }
    public void ClearFieldsButton(object sender, EventArgs e) 
    {
        mokkiID.Text = string.Empty;
        Mokkinimi.Text = string.Empty;
        Postinro.Text = string.Empty;
        Hinta.Text = string.Empty;
        Katuosoite.Text = string.Empty;
        Kuvaus.Text = string.Empty;
        Henkilomaara.Text = string.Empty;
        Varustelu.Text = string.Empty;
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
                Mokkinimi.Text = selectedMokki.Mokkinimi;
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
        if (Mokkinimi.Text == null || Mokkinimi.Text == string.Empty)
        {
            await DisplayAlert("Virhe", "Mökkinimi ei saa olla tyhjä.", "Ok");
            return;
        }

        else if (Postinro.Text == string.Empty || Postinro.Text == null)
        {
            await DisplayAlert("Virhe", "Postinro ei saa olla tyhjä.", "Ok");
            return;
        }
        else if (Toimipaikka.Text == string.Empty || Toimipaikka.Text == null)
        {
            await DisplayAlert("Virhe", "Toimipaikka ei saa olla tyhjä.", "Ok");
            return;
        }
        else if (Katuosoite.Text == string.Empty || Katuosoite.Text == null)
        {
            await DisplayAlert("Virhe", "Katuosoite ei saa olla tyhjä.", "Ok");
            return;
        }
        else if (Hinta.Text == string.Empty | Hinta.Text == null)
        {
            await DisplayAlert("Virhe", "Hinta ei saa olla tyhjä.", "Ok");
            return;
        }
        else if (Kuvaus.Text == string.Empty | Kuvaus.Text == null)
        {
            await DisplayAlert("Virhe", "Kuvaus ei saa olla tyhjä.", "Ok");
            return;
        }
        else if (HiddenEntry3.Text == string.Empty | HiddenEntry3.Text == null) 
        {
            await DisplayAlert("Virhe", "Henkilömäärä ei saa olla tyhjä.", "Ok");
            return;
        }
        else if (Henkilomaara.Text == string.Empty | Henkilomaara.Text == null)
        {
            await DisplayAlert("Virhe", "Henkilömäärä ei saa olla tyhjä.", "Ok");
            return;
        }

        else if (Varustelu.Text == string.Empty | Varustelu.Text == null)
        {
            await DisplayAlert("Virhe", "Varustelu kenttä ei saa olla tyhjä.", "Ok");
            return;
        }

        string mokkinimi = Mokkinimi.Text;
        string postinro = Postinro.Text;
        string toimipaikka = Toimipaikka.Text;
        string katuosoite = Katuosoite.Text;
        double hinta;
        string kuvaus = Kuvaus.Text;
        string varustelu = Varustelu.Text;
        int henkilomaara;
        string nimi = HiddenEntry3.Text;

        if (mokkinimi.Length > 45)
        {
            await DisplayAlert("Virhe", "Mökinnimi saa olla max 45 merkkiä", "Ok");
            return;
        }
        else if (katuosoite.Length > 45)
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

                string PaivitaMokki = "UPDATE vn.mokki SET postinro = @postinro, mokkinimi = @mokkinimi, katuosoite = @katuosoite, hinta = @hinta, kuvaus = @kuvaus, henkilomaara = @henkilomaara, alue_id = (SELECT alue_id FROM vn.alue WHERE nimi = @nimi) WHERE mokki_id = @MokkiID;";

                using (var Command = new MySqlCommand(PaivitaMokki, connection))
                {
                    Command.Parameters.AddWithValue("@MokkiID", int.Parse(mokkiID.Text));
                    Command.Parameters.AddWithValue("@mokkinimi", mokkinimi);
                    Command.Parameters.AddWithValue("@katuosoite", katuosoite);
                    Command.Parameters.AddWithValue("@postinro", postinro);
                    Command.Parameters.AddWithValue("@kuvaus", kuvaus);
                    Command.Parameters.AddWithValue("@henkilomaara", henkilomaara);
                    Command.Parameters.AddWithValue("@hinta", hinta);
                    Command.Parameters.AddWithValue("@nimi", HiddenEntry3.Text);
                    await Command.ExecuteNonQueryAsync();

                    var updatedMokki = MokkiList.FirstOrDefault(m => m.MokkiID == int.Parse(mokkiID.Text));
                    if (updatedMokki != null)
                    {
                        updatedMokki.Mokkinimi = mokkinimi;
                        updatedMokki.Katuosoite = katuosoite;
                        updatedMokki.Postinro = postinro;
                        updatedMokki.Hinta = hinta;
                        updatedMokki.Kuvaus = kuvaus;
                        updatedMokki.Henkilomaara = henkilomaara;
                    }
                }
            }
        }
    }

    //Poistaa mökin kaikkialta tietokannasta.
    public async void DeleteOnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(mokkiID.Text))
        {
            await DisplayAlert("Error", "Ei valittua mökkiä.", "OK");
            return;
        }

        int mokkiIdToDelete = int.Parse(mokkiID.Text);
        DatabaseConnection dbc = new DatabaseConnection();

        using (var connection = dbc._getConnection())
        {
            connection.Open();

            string deleteRelatedQuery = "DELETE FROM vn.varaus WHERE mokki_id = @MokkiID;";
            using (var command = new MySqlCommand(deleteRelatedQuery, connection))
            {
                command.Parameters.AddWithValue("@MokkiID", mokkiIdToDelete);
                await command.ExecuteNonQueryAsync();
            }

            string deleteQuery = "DELETE FROM vn.mokki WHERE mokki_id = @MokkiID;";
            using (var command = new MySqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@MokkiID", mokkiIdToDelete);
                await command.ExecuteNonQueryAsync();
            }
        }

        var mokkiToRemove = MokkiList.FirstOrDefault(m => m.MokkiID == mokkiIdToDelete);
        if (mokkiToRemove != null)
        {
            MokkiList.Remove(mokkiToRemove);
        }

        await DisplayAlert("Success", "Mökki onnistuneesti poistettu.", "OK");
    }
    private async void UpdatePickerButton_Clicked(object sender, EventArgs e)
    {
        var updatedAlueNames = await GetAlueNamesAsync();
        aluePicker.ItemsSource = updatedAlueNames;
    }
}

public class Mokki : INotifyPropertyChanged
    {
        private int mokkiID;
        private string mokkinimi;
        private string katuosoite;
        private string postinro;
        private double hinta;
        private string kuvaus;
        private int henkilomaara;
        private string varustelu;

        public int MokkiID
        {
            get => mokkiID;
            set
            {
                mokkiID = value;
                OnPropertyChanged(nameof(MokkiID));
            }
        }

        public string Mokkinimi
        {
            get => mokkinimi;
            set
            {
                mokkinimi = value;
                OnPropertyChanged(nameof(Mokkinimi));
            }
        }

        public string Katuosoite
        {
            get => katuosoite;
            set
            {
                katuosoite = value;
                OnPropertyChanged(nameof(Katuosoite));
            }
        }

        public string Postinro
        {
            get => postinro;
            set
            {
                postinro = value;
                OnPropertyChanged(nameof(Postinro));
            }
        }

        public double Hinta
        {
            get => hinta;
            set
            {
                hinta = value;
                OnPropertyChanged(nameof(Hinta));
            }
        }

        public string Kuvaus
        {
            get => kuvaus;
            set
            {
                kuvaus = value;
                OnPropertyChanged(nameof(Kuvaus));
            }
        }

        public int Henkilomaara
        {
            get => henkilomaara;
            set
            {
                henkilomaara = value;
                OnPropertyChanged(nameof(Henkilomaara));
            }
        }

        public string Varustelu
        {
            get => varustelu;
            set
            {
                varustelu = value;
                OnPropertyChanged(nameof(Varustelu));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }