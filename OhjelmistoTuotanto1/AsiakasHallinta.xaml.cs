namespace OhjelmistoTuotanto1;
using MySqlConnector;
using System.Collections.ObjectModel;

public partial class AsiakasHallinta : ContentPage
{
    public ObservableCollection<Asiakas> AsiakasList { get; set; }


    /// Toistaiseksi n‰ytt‰‰ asiakastiedot tietokannasta
    ///Lis‰ys ei viel‰ implementoitu
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
                    Lahiosoite = reader.GetString("lahiosoite"),
                    Email = reader.GetString("email"),
                    Puhelinnro = reader.GetString("puhelinnro")
                });
            }
        }
    }
}

public class Asiakas
{
    public int AsiakasId { get; set; }
    public string Etunimi { get; set; }
    public string Sukunimi { get; set; }
    public string Lahiosoite { get; set; }
    public string Email { get; set; }
    public string Puhelinnro { get; set; }
}
