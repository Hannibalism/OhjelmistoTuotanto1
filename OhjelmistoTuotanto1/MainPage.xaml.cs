using MySqlConnector;

namespace OhjelmistoTuotanto1;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
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

