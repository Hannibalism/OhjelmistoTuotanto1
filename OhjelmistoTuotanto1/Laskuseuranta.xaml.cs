using MySqlConnector;
using OhjelmistoTuotanto1.Data;
using OhjelmistoTuotanto1.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Reflection.Metadata;
using System.ComponentModel;
using QuestPDF.Helpers;

namespace OhjelmistoTuotanto1;

public partial class Laskuseuranta : ContentPage
{
    public ObservableCollection<Lasku> Laskut { get; set; }

    public Laskuseuranta()
    {
        InitializeComponent();
        Laskut = new ObservableCollection<Lasku>();
        LoadLaskut();
        BindingContext = this;
    }
    private void UpdateStatusClicked(object sender, EventArgs e)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            var command = new MySqlCommand(@"
                UPDATE lasku 
                SET maksettu = @maksettu 
                WHERE lasku_id = @laskuId", connection);
            command.Parameters.AddWithValue("@maksettu", MaksuStatus.Text);
            command.Parameters.AddWithValue("@laskuId", LaskuID.Text);
            command.ExecuteNonQuery();
            Laskut.Clear();
            LoadLaskut();
        }
    }
    private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedLasku = e.CurrentSelection[0] as Lasku;
            Varaaja.Text = selectedLasku.asiakasnimi;
            LaskuID.Text = selectedLasku.LaskuId.ToString();
            VarausID.Text = selectedLasku.VarausId.ToString();
            Summa.Text = selectedLasku.Summa.ToString("C");
            ALV.Text = selectedLasku.Alv.ToString();
            Osoite.Text = selectedLasku.Katuosoite;
        }
    }

    private void LoadLaskut()
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            var command = new MySqlCommand(@"
            SELECT l.*, a.etunimi, a.sukunimi, a.lahiosoite 
            FROM lasku l
            JOIN varaus v ON l.varaus_id = v.varaus_id
            JOIN asiakas a ON v.asiakas_id = a.asiakas_id", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Laskut.Add(new Lasku
                    {
                        LaskuId = reader.GetInt32("lasku_id"),
                        VarausId = reader.GetInt32("varaus_id"),
                        Summa = reader.GetDouble("summa"),
                        Alv = reader.GetDouble("alv"),
                        Maksettu = reader.GetBoolean("maksettu"),
                        asiakasnimi = $"{reader.GetString("etunimi")} {reader.GetString("sukunimi")}",
                        Katuosoite = reader.GetString("lahiosoite")
                    });
                }
            }
        }
    }
    private void TulostaClicked(object sender, EventArgs e)
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string pdfFilePath = System.IO.Path.Combine(documentsPath, "Lasku.pdf");


        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Header()
                        .Text("Lasku")
                        .SemiBold().FontSize(35);
                        page.Content()
                .Column(x =>
                {
                    x.Spacing(10);
                    x.Item().Text("Teille on lasku Village Newbies Oy:lta");
                    x.Item().Text("Asiakasnimi: " + Varaaja.Text);
                    x.Item().Text("Osoite: " + Osoite.Text);
                    x.Item().Text("Laskun numero: " + LaskuID.Text);
                    x.Item().Text("Varauksen numero: " + VarausID.Text);
                    x.Item().Text("Laskun summa: " + Summa.Text);
                    x.Item().Text("Veron osuus: " + ALV.Text + "�");
                    x.Item().Text("Viite: XXXXXXXXXXXX");
                    x.Item().Text("Tilinumero: FIXXXXXXXXXXX");
                    x.Item().Text("P�iv�ys: " + DateTime.Now.ToString());
                });
                        page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Sivu ");
                        x.CurrentPageNumber();
                    });
                    });

        }).GeneratePdf(pdfFilePath);

    } 
}