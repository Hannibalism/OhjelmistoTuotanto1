using Google.Protobuf.WellKnownTypes;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using MySqlConnector;
using OhjelmistoTuotanto1.Data;
using OhjelmistoTuotanto1.Models;
using OhjelmistoTuotanto1.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace OhjelmistoTuotanto1;

public partial class PalveluRaporttiPage : ContentPage
{
    public ObservableCollection<PalveluRaport> PalveluRaportList { get; set; }

    public PalveluRaporttiPage()
    {
        InitializeComponent();
        PalveluRaportList = new ObservableCollection<PalveluRaport>();
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

        PalveluRaportList.Clear();
        PalveluReportViewModel viewmodel = new PalveluReportViewModel();
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            {
                string query = "SELECT p.*, SUM(vp.lkm) AS total_lkm, SUM(hinta) AS total_tuotto FROM vn.palvelu p " +
                               "JOIN vn.varauksen_palvelut vp ON p.palvelu_id = vp.palvelu_id " +
                               "JOIN vn.varaus v ON vp.varaus_id = v.varaus_id " +
                               "JOIN vn.alue a ON p.alue_id = a.alue_id " +
                               "WHERE v.varattu_alkupvm BETWEEN @startDate AND @endDate " +
                               "AND a.nimi = @nimi " +
                               "GROUP BY p.palvelu_id;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", HiddenEntry1.Text);
                    command.Parameters.AddWithValue("@endDate", HiddenEntry2.Text);
                    command.Parameters.AddWithValue("@nimi", HiddenEntry3.Text);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            PalveluRaportList.Add(new PalveluRaport
                            {
                                palvelu_id = reader.GetInt32("palvelu_id"),
                                alue_id = reader.GetInt32("alue_id"),
                                nimi = reader.GetString("nimi"),
                                kuvaus = reader.GetString("kuvaus"),
                                hinta = reader.GetDouble("hinta"),
                                alv = reader.GetDouble("alv"),
                                totallkm = reader.GetInt32("total_lkm"),
                                totaltuotto = reader.GetDouble("total_tuotto")
                            });
                        }
                    }
                }
            }
        }
    }
    public class PalveluRaport
    {
        public int palvelu_id { get; set; }
        public int alue_id { get; set; }
        public string nimi { get; set; }
        public string kuvaus { get; set; }
        public double hinta { get; set; }
        public double alv { get; set; }
        public int totallkm { get; set; }
        public double totaltuotto { get; set; }
    }
    public class CompositeViewModel : INotifyPropertyChanged
    {
        public PalveluReportViewModel ReportViewModel { get; set; }
        public PalveluRaporttiPage Page { get; set; }

        public CompositeViewModel(PalveluRaporttiPage page)
        {
            Page = page;
            ReportViewModel = new PalveluReportViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    private bool FormatTarkistus(string input)
    {
        return Regex.IsMatch(input, @"^[0-9/\\]*$");
    }
}