namespace OhjelmistoTuotanto1;

using MySqlConnector;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.PortableExecutable;
using OhjelmistoTuotanto1.Data;
using System.ComponentModel;
using Org.BouncyCastle.Tls;

public partial class AsiakasHallinta : ContentPage
{
    public ObservableCollection<Asiakas> AsiakasList { get; set; }


    //Lis�� tietokantaan asiakkaan tietoja ja p�ivitys onnistuu valitsemalla listasta asiakas.
    //Tiedot kopioituvat kenttiin ja asiakasID kentt� t�yttyy jonka j�lkeen kyseisen asiakasID:n
    //Tiedot p�ivittyv�t samasta napista. Jos uutta tietoa haluaa asiakkaan valittua lis�t�
    //Pit�� kent�t tyhjent�� tyhjennys napilla.
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
        if (EtunimiEntry.Text == string.Empty | EtunimiEntry.Text == null)
        {
            await DisplayAlert("Virhe", "Etunimi ei saa olla tyhj�.", "Ok");
            return;
        }
        else if (SukunimiEntry.Text == string.Empty | SukunimiEntry.Text == null)
        {
            await DisplayAlert("Virhe", "Sukunimi ei saa olla tyhj�.", "Ok");
            return;
        }
        else if (PostinumeroEntry.Text == string.Empty | PostinumeroEntry.Text == null)
        {
            await DisplayAlert("Virhe", "Postinumero ei saa olla tyhj�.", "Ok");
            return;
        }
        else if (LahiosoiteEntry.Text == string.Empty | LahiosoiteEntry.Text == null)
        {
            await DisplayAlert("Virhe", "Lahiosoite ei saa olla tyhj�.", "Ok");
            return;
        }
        else if (emailEntry.Text == string.Empty | emailEntry.Text == null)
        {
            await DisplayAlert("Virhe", "s�hk�posti ei saa olla tyhj�.", "Ok");
            return;
        }
        else if (PuhelinnumeroEntry.Text == string.Empty | PuhelinnumeroEntry.Text == null)
        {
            await DisplayAlert("Virhe", "Puhelinnumero ei saa olla tyhj�.", "Ok");
            return;
        }
        else if (ToimipaikkaEntry.Text == string.Empty | ToimipaikkaEntry.Text == null)
        {
            await DisplayAlert("Virhe", "Toimipaikka ei saa olla tyhj�.", "Ok");
            return;
        }



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
                        await DisplayAlert("Oho", "Postinumero on jo kannassa, k�ytet��n olevaa tietoa", "ok");
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
                    await DisplayAlert("Onnistui", "Listaan lis�ys onnistui", "Ok");
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
                        await DisplayAlert("Oho", "Postinumero on jo kannassa, k�ytet��n olevaa tietoa", "ok");
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
                    await Command.ExecuteNonQueryAsync();

                    var existingAsiakas = AsiakasList.FirstOrDefault(a => a.AsiakasId == int.Parse(asiakasid));
                    if (existingAsiakas != null)
                    {
                        existingAsiakas.Etunimi = etunimi;
                        existingAsiakas.Sukunimi = sukunimi;
                        existingAsiakas.Postinumero = postinumero;
                        existingAsiakas.Lahiosoite = lahiosoite;
                        existingAsiakas.Email = email;
                        existingAsiakas.Puhelinnro = puhelinnumero;
                    }
                }
            }
        }
    }
    public async void DeleteOnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(IDEntry.Text))
        {
            await DisplayAlert("Error", "Ei valittua asiakasta.", "OK");
            return;
        }

        int asiakasIdToDelete = int.Parse(IDEntry.Text);
        DatabaseConnection dbc = new DatabaseConnection();

        using (var connection = dbc._getConnection())
        {
            connection.Open();

            // Poistaa asiakkaan varaus palveluista.
            // Poistaa asiakkaan varaus taulusta.
            // Poistaa asiakkaan t�st� taulusta.
            string deleteRelatedQuery = "DELETE FROM vn.varauksen_palvelut WHERE varaus_id IN (SELECT varaus_id FROM vn.varaus WHERE asiakas_id = @asiakasID);";
            using (var command = new MySqlCommand(deleteRelatedQuery, connection))
            {
                command.Parameters.AddWithValue("@asiakasID", asiakasIdToDelete);
                await command.ExecuteNonQueryAsync();
            }

            
            deleteRelatedQuery = "DELETE FROM vn.varaus WHERE asiakas_id = @asiakasID;";
            using (var command = new MySqlCommand(deleteRelatedQuery, connection))
            {
                command.Parameters.AddWithValue("@asiakasID", asiakasIdToDelete);
                await command.ExecuteNonQueryAsync();
            }

            string deleteQuery = "DELETE FROM vn.asiakas WHERE asiakas_id = @asiakasID;";
            using (var command = new MySqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@asiakasID", asiakasIdToDelete);
                await command.ExecuteNonQueryAsync();
            }
        }

        var asiakasToRemove = AsiakasList.FirstOrDefault(m => m.AsiakasId == asiakasIdToDelete);
        if (asiakasToRemove != null)
        {
            AsiakasList.Remove(asiakasToRemove);
        }

        await DisplayAlert("Success", "Asiakas poistettu onnistuneest.", "OK");
    }

    public class Asiakas : INotifyPropertyChanged
    {
        private int asiakasId;
        private string etunimi;
        private string sukunimi;
        private string postinumero;
        private string lahiosoite;
        private string email;
        private string puhelinnumero;

        public int AsiakasId
        {
            get => asiakasId;
            set { asiakasId = value; OnPropertyChanged(nameof(AsiakasId)); }
        }

        public string Etunimi
        {
            get => etunimi;
            set { etunimi = value; OnPropertyChanged(nameof(Etunimi)); }
        }

        public string Sukunimi
        {
            get => sukunimi;
            set { sukunimi = value; OnPropertyChanged(nameof(Sukunimi)); }
        }

        public string Postinumero
        {
            get => postinumero;
            set { postinumero = value; OnPropertyChanged(nameof(Postinumero)); }
        }

        public string Lahiosoite
        {
            get => lahiosoite;
            set { lahiosoite = value; OnPropertyChanged(nameof(Lahiosoite)); }
        }

        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string Puhelinnro
        {
            get => puhelinnumero;
            set { puhelinnumero = value; OnPropertyChanged(nameof(Puhelinnro)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
