namespace OhjelmistoTuotanto1;

using MySqlConnector;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.PortableExecutable;
using OhjelmistoTuotanto1.Data;

public partial class AsiakasHallinta : ContentPage
{
    public ObservableCollection<Asiakas> AsiakasList { get; set; }


    //Lis‰‰ tietokantaan asiakkaan tietoja ja p‰ivitys onnistuu valitsemalla listasta asiakas.
    //Tiedot kopioituvat kenttiin ja asiakasID kentt‰ t‰yttyy jonka j‰lkeen kyseisen asiakasID:n
    //Tiedot p‰ivittyv‰t samasta napista. Jos uutta tietoa haluaa asiakkaan valittua lis‰t‰
    //Pit‰‰ kent‰t tyhjent‰‰ tyhjennys napilla.
    public AsiakasHallinta()
    {
        InitializeComponent();
        AsiakasList = new ObservableCollection<Asiakas>();
        LoadData();
        BindingContext = this;
    }

    private void LoadData()
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand("SELECT * FROM asiakas", connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                AsiakasList.Add(new Asiakas
                {
                    AsiakasId = reader.GetInt32("asiakas_id"),
                    Etunimi = reader.GetString("etunimi"),
                    Sukunimi = reader.GetString("sukunimi"),
                    Postinumero = reader.GetString("postinro"),
                    Lahiosoite = reader.GetString("lahiosoite"),
                    Email = reader.GetString("email"),
                    Puhelinnro = reader.GetString("puhelinnro")
                });
            }
        }

    }
    public void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedAsiakas = e.CurrentSelection[0] as Asiakas;
            if (selectedAsiakas != null)
            {
                IDEntry.Text = selectedAsiakas.AsiakasId.ToString();
                EtunimiEntry.Text = selectedAsiakas.Etunimi;
                SukunimiEntry.Text = selectedAsiakas.Sukunimi;
                PostinumeroEntry.Text = selectedAsiakas.Postinumero;
                LahiosoiteEntry.Text = selectedAsiakas.Lahiosoite;
                emailEntry.Text = selectedAsiakas.Email;
                PuhelinnumeroEntry.Text = selectedAsiakas.Puhelinnro;
            }
        }
    }
    private void Tyhjenna(object sender, EventArgs e) 
    { 
        IDEntry.Text = string.Empty;
        EtunimiEntry.Text = string.Empty;
        SukunimiEntry.Text = string.Empty;
        PostinumeroEntry.Text= string.Empty;
        LahiosoiteEntry.Text= string.Empty;
        emailEntry.Text= string.Empty;
        PuhelinnumeroEntry.Text = string.Empty;
        ToimipaikkaEntry.Text= string.Empty;
    }
    private async void Lisaa(object sender, EventArgs e)
    {
        string asiakasid = IDEntry.Text;
        string etunimi = EtunimiEntry.Text;
        string sukunimi = SukunimiEntry.Text;
        string postinumero = PostinumeroEntry.Text;
        string lahiosoite = LahiosoiteEntry.Text;
        string email = emailEntry.Text;
        string puhelinnumero = PuhelinnumeroEntry.Text;
        string toimipaikka = ToimipaikkaEntry.Text;
        await InsertData(asiakasid, etunimi, sukunimi, postinumero, lahiosoite, email, puhelinnumero, toimipaikka);
    }
    private async Task InsertData(string asiakasid, string etunimi, string sukunimi, string postinumero, string lahiosoite, string email, string puhelinnumero, string toimipaikka)
    {
        if (asiakasid == null || asiakasid == string.Empty)
        {
            DatabaseConnection dbc = new DatabaseConnection();
            using (var connection = dbc._getConnection())
            {
                connection.Open();
                string postinroTarkistus = "SELECT COUNT(*) from vn.posti where postinro = @postinro;";
                using (var command = new MySqlCommand(postinroTarkistus, connection))
                {
                    command.Parameters.AddWithValue("@postinro", postinumero);
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        await DisplayAlert("Oho", "Postinumero on jo kannassa, k‰ytet‰‰n olevaa tietoa", "ok");
                    }
                    else if (count == 0)
                    {
                        string addPostinro = "INSERT INTO vn.posti (postinro, toimipaikka) VALUES (@postinro, @toimipaikka);";
                        using (var insertCommand = new MySqlCommand(addPostinro, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@postinro", postinumero);
                            insertCommand.Parameters.AddWithValue("@toimipaikka", toimipaikka);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
                string addAsiakas = "INSERT INTO vn.asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES ((SELECT postinro from vn.posti where postinro = @postinumero), @etunimi, @sukunimi, @lahiosoite, @email, @puhelinnumero); SELECT LAST_INSERT_ID();";
                using (var Command = new MySqlCommand(addAsiakas, connection))
                {
                    Command.Parameters.AddWithValue("@etunimi", etunimi);
                    Command.Parameters.AddWithValue("@sukunimi", sukunimi);
                    Command.Parameters.AddWithValue("@postinumero", postinumero);
                    Command.Parameters.AddWithValue("@lahiosoite", lahiosoite);
                    Command.Parameters.AddWithValue("@email", email);
                    Command.Parameters.AddWithValue("@puhelinnumero", puhelinnumero);
                    var newId = await Command.ExecuteScalarAsync();
                    await DisplayAlert("Onnistui", "Listaan lis‰ys onnistui", "Ok");
                    AsiakasList.Add(new Asiakas
                    {
                        AsiakasId = Convert.ToInt32(newId),
                        Etunimi = etunimi,
                        Sukunimi = sukunimi,
                        Postinumero = postinumero,
                        Lahiosoite = lahiosoite,
                        Email = email,
                        Puhelinnro = puhelinnumero
                    });
                }
            }
        }
        else if (IDEntry.Text != null || IDEntry.Text != string.Empty)
        {
            DatabaseConnection dbc = new DatabaseConnection();
            using (var connection = dbc._getConnection())
            {
                connection.Open();

                string postinroTarkistus = "SELECT COUNT(*) from vn.posti where postinro = @postinro;";
                using (var command = new MySqlCommand(postinroTarkistus, connection))
                {
                    command.Parameters.AddWithValue("@postinro", postinumero);
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        await DisplayAlert("Oho", "Postinumero on jo kannassa, k‰ytet‰‰n olevaa tietoa", "ok");
                    }
                    else if (count == 0)
                    {
                        string addPostinro = "INSERT INTO vn.posti (postinro, toimipaikka) VALUES (@postinro, @toimipaikka);";
                        using (var insertCommand = new MySqlCommand(addPostinro, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@postinro", postinumero);
                            insertCommand.Parameters.AddWithValue("@toimipaikka", toimipaikka);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }

                string PaivitaAsiakas = "UPDATE vn.asiakas SET postinro = @postinumero, etunimi = @etunimi, sukunimi = @sukunimi, lahiosoite = @lahiosoite, email = @email, puhelinnro = @puhelinnumero WHERE asiakas_id = @asiakasID;";
                using (var Command = new MySqlCommand(PaivitaAsiakas, connection))
                {
                    Command.Parameters.AddWithValue("@asiakasID", int.Parse(IDEntry.Text));
                    Command.Parameters.AddWithValue("@etunimi", etunimi);
                    Command.Parameters.AddWithValue("@sukunimi", sukunimi);
                    Command.Parameters.AddWithValue("@postinumero", postinumero);
                    Command.Parameters.AddWithValue("@lahiosoite", lahiosoite);
                    Command.Parameters.AddWithValue("@email", email);
                    Command.Parameters.AddWithValue("@puhelinnumero", puhelinnumero);
                    Command.ExecuteScalar();
                }
            }
        }
    }

    public class Asiakas
    {
        public int AsiakasId { get; set; }
        public string Etunimi { get; set; }
        public string Sukunimi { get; set; }
        public string Postinumero {  get; set; }
        public string Lahiosoite { get; set; }
        public string Email { get; set; }
        public string Puhelinnro { get; set; }


    }
}
