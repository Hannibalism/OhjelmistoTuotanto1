using MySqlConnector;
using OhjelmistoTuotanto1.Data;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace OhjelmistoTuotanto1;

public partial class MajoitusRaport : ContentPage
{
    public ObservableCollection<Tiedot> TiedotList { get; set; } = new ObservableCollection<Tiedot>();

    public MajoitusRaport()
    {
        InitializeComponent();
        BindingContext = this;
        LoadAlueData();

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

    private async void palveluhakuClicked(object sender, EventArgs e)
    {
        //Varmistaa ettei Entreissä ole kirjaimia tai muita ei sallittuja merkkejä.
        if (!FormatTarkistus(HiddenEntry1.Text) || !FormatTarkistus(HiddenEntry2.Text))
        {
            await DisplayAlert("Format virhe.", "Ethän syötä kirjaimia.", "OK");
            return;
        }

        TiedotList.Clear();
        PalveluReportViewModel viewmodel = new PalveluReportViewModel();
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            {
                string query = "SELECT a.etunimi, a.sukunimi, m.mokkinimi, v.varattu_pvm, v.varattu_alkupvm, v.varattu_loppupvm " +
                       "FROM vn.varaus v " +
                       "JOIN vn.asiakas a ON v.asiakas_id = a.asiakas_id " +
                       "JOIN vn.mokki m ON v.mokki_id = m.mokki_id " +
                       "WHERE m.alue_id = (SELECT alue_id FROM vn.alue WHERE nimi = @alue) " +
                       "AND v.varattu_alkupvm BETWEEN @startDate AND @endDate";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", HiddenEntry1.Text);
                    command.Parameters.AddWithValue("@endDate", HiddenEntry2.Text);
                    command.Parameters.AddWithValue("@alue", HiddenEntry3.Text);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TiedotList.Add(new Tiedot
                            {
                                AsiakasNimi = $"{reader.GetString(0)} {reader.GetString(1)}",
                                MokkiNimi = reader.GetString(2),
                                VarattuPvm = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                                VarattuAlkuPvm = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                                VarattuLoppuPvm = reader.GetDateTime(5).ToString("yyyy-MM-dd")
                            });
                        }
                    }
                }
            }
        }

    }
    private bool FormatTarkistus(string input)
    {
        return Regex.IsMatch(input, @"^[0-9/\\]*$");
    }
}
public class Tiedot
    {
        public string AsiakasNimi { get; set; }
        public string MokkiNimi { get; set; }
        public string VarattuPvm { get; set; }
        public string VarattuAlkuPvm { get; set; }
        public string VarattuLoppuPvm { get; set; }
    }