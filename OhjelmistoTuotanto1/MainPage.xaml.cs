using MySqlConnector;

namespace OhjelmistoTuotanto1;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void Mokkilisays(object sender, EventArgs e)
	{
        Navigation.PushAsync(new LisaysSivu());
    }
    private async void OnDatabaseClicked(object sender, EventArgs e)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        try
        {
            var conn = dbc._getConnection();
            conn.Open();
            await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK"); 
        }
        catch (MySqlException ex)
        {
            await DisplayAlert("Failure", ex.Message, "OK");
        }
    }
}

