using OhjelmistoTuotanto1.Models;
using MySqlConnector;
using OhjelmistoTuotanto1.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace OhjelmistoTuotanto1;

public partial class LaskutusSivu : ContentPage
{
    private Lasku valittuLasku;
    public LaskutusSivu()
    {
        InitializeComponent();
        LoadLaskut();
        //LoadVarausData();
    }

    private async void LoadLaskut()
    {
        try
        {
            DatabaseConnection dbc = new DatabaseConnection();
            var laskut = new List<Lasku>();

            using (var connection = dbc._getConnection())
            {
                await connection.OpenAsync();
                string query = @"
                SELECT l.lasku_id, l.varaus_id, l.summa, l.alv, l.maksettu,
                       CONCAT(a.etunimi, ' ', a.sukunimi) AS asiakasnimi, a.lahiosoite
                FROM vn.lasku l
                JOIN vn.varaus v ON l.varaus_id = v.varaus_id
                JOIN vn.asiakas a ON v.asiakas_id = a.asiakas_id
                ORDER BY l.lasku_id ASC";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    laskut.Add(new Lasku
                    {
                        LaskuId = reader.GetInt32(0),
                        VarausId = reader.GetInt32(1),
                        Summa = reader.GetDouble(2),
                        Alv = reader.GetDouble(3),
                        Maksettu = reader.GetBoolean(4),
                        asiakasnimi = reader.GetString(5) + (reader.GetBoolean(4) ? " (Maksettu)" : " (Ei maksettu)"),
                        Katuosoite = reader.GetString(6)
                    });
                }
            }

            laskutView.ItemsSource = laskut;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe", $"Laskujen lataaminen epäonnistui:\n{ex.Message}", "OK");
        }
    }

    /*private async void LoadVarausData()
    {
        DatabaseConnection dbc = new DatabaseConnection();
        var varaukset = new List<VarausNakyma>();

        using (var connection = dbc._getConnection())
        {
            await connection.OpenAsync();
            string query = @"
                SELECT v.varaus_id, CONCAT(a.etunimi, ' ', a.sukunimi) AS asiakas, m.mokkinimi
                FROM vn.varaus v
                JOIN vn.asiakas a ON v.asiakas_id = a.asiakas_id
                JOIN vn.mokki m ON v.mokki_id = m.mokki_id";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                varaukset.Add(new VarausNakyma
                {
                    VarausId = reader.GetInt32(0),
                    AsiakasNimi = reader.GetString(1),
                    MokkiNimi = reader.GetString(2)
                });
            }
        }

        varausPicker.ItemsSource = varaukset;
    }*/

    /*private async void VarausPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedVaraus = varausPicker.SelectedItem as VarausNakyma;
        if (selectedVaraus == null)
            return;

        DatabaseConnection dbc = new DatabaseConnection();
        using var connection = dbc._getConnection();
        await connection.OpenAsync();

        string query = "SELECT summa, alv FROM vn.lasku WHERE varaus_id = @varaus_id LIMIT 1";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@varaus_id", selectedVaraus.VarausId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            Summa.Text = reader.GetDouble(0).ToString("F2");
            Alv.Text = reader.GetDouble(1).ToString("F2");
        }
        else
        {
            Summa.Text = "";
            Alv.Text = "";
        }
    } */

    private async void Lisaalasku_Clicked(object sender, EventArgs e)
    {
        if (valittuLasku == null)
        {
            await DisplayAlert("Virhe", "Valitse ensin lasku listasta", "OK");
            return;
        }

        if (!double.TryParse(Summa.Text, out double summa))
        {
            await DisplayAlert("Virhe", "Summa ei ole kelvollinen numero", "OK");
            return;
        }

        if (!double.TryParse(Alv.Text, out double alv))
        {
            await DisplayAlert("Virhe", "ALV ei ole kelvollinen numero", "OK");
            return;
        }

        if (summa > 99999999.99)
        {
            await DisplayAlert("Virhe", "Summa on liian suuri.", "OK");
            return;
        }

        if (alv > 99999999.99)
        {
            await DisplayAlert("Virhe", "Summa on liian suuri.", "OK");
            return;
        }

        bool maksettu = MaksettuSwitch.IsToggled;

        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            await connection.OpenAsync();

            string update = @"UPDATE vn.lasku 
                          SET summa = @summa, alv = @alv, maksettu = @maksettu 
                          WHERE lasku_id = @lasku_id";

            using var command = new MySqlCommand(update, connection);
            command.Parameters.AddWithValue("@summa", summa);
            command.Parameters.AddWithValue("@alv", alv);
            command.Parameters.AddWithValue("@maksettu", maksettu);
            command.Parameters.AddWithValue("@lasku_id", valittuLasku.LaskuId);

            await command.ExecuteNonQueryAsync();
            await DisplayAlert("Tallennettu", "Laskun tiedot päivitetty!", "OK");

            LoadLaskut();
        }
    }

    private void LaskuValittu(object sender, SelectionChangedEventArgs e)
    {
        valittuLasku = e.CurrentSelection.FirstOrDefault() as Lasku;
        if (valittuLasku == null)
            return;

        Summa.Text = valittuLasku.Summa.ToString("F2");
        Alv.Text = valittuLasku.Alv.ToString("F2");
        MaksettuSwitch.IsToggled = valittuLasku.Maksettu;

        Varaaja.Text = valittuLasku.asiakasnimi;
        Osoite.Text = valittuLasku.Katuosoite;
    }

    private async void TulostaClicked(object sender, EventArgs e)
    {
        if (valittuLasku == null)
        {
            await DisplayAlert("Virhe", "Valitse ensin lasku listasta", "OK");
            return;
        }

        try
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pdfFilePath = Path.Combine(documentsPath, $"Lasku_{valittuLasku.LaskuId}.pdf");

            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Header().Text("Lasku").SemiBold().FontSize(35);
                    page.Content().Column(x =>
                    {
                        x.Spacing(10);
                        x.Item().Text("Teille on lasku Village Newbies Oy:lta");
                        x.Item().Text("Asiakasnimi: " + valittuLasku.asiakasnimi);
                        x.Item().Text("Osoite: " + valittuLasku.Katuosoite);
                        x.Item().Text("Laskun numero: " + valittuLasku.LaskuId);
                        x.Item().Text("Varauksen numero: " + valittuLasku.VarausId);
                        x.Item().Text("Laskun summa: " + valittuLasku.Summa.ToString("F2") + " €");
                        x.Item().Text("Veron osuus: " + valittuLasku.Alv.ToString("F2") + " €");
                        x.Item().Text("Viite: XXXXXXXXXXXX");
                        x.Item().Text("Tilinumero: FIXXXXXXXXXXX");
                        x.Item().Text("Päiväys: " + DateTime.Now.ToString("dd.MM.yyyy"));
                    });
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Sivu ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(pdfFilePath);

            await DisplayAlert("Tulostus", $"Lasku on tallennettu: {pdfFilePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe", $"PDF:n luominen epäonnistui:\n{ex.Message}", "OK");
        }
    }
}
