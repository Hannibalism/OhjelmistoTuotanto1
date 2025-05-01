using MySqlConnector;
using OhjelmistoTuotanto1.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace OhjelmistoTuotanto1;

public partial class Aluehallinta : ContentPage
{
    public ObservableCollection<Alue> AlueList { get; set; }

    public Aluehallinta()
    {
        InitializeComponent();
        AlueList = new ObservableCollection<Alue>();
        LoadData();
        BindingContext = this;
    }

    private void LoadData()
    {
        DatabaseConnection dbc = new DatabaseConnection();
        using (var connection = dbc._getConnection())
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand("SELECT * FROM vn.alue", connection);
            MySqlDataReader reader = command.ExecuteReader();

            AlueList.Clear(); // Clear existing items before loading new data

            while (reader.Read())
            {
                AlueList.Add(new Alue
                {
                    alueId = reader.GetInt32("alue_id"),
                    nimi = reader.IsDBNull(reader.GetOrdinal("nimi")) ? string.Empty : reader.GetString("nimi"),
                });
            }
        }
    }

    private void AddClicked(object sender, EventArgs e)
    {
        string alueNimi = AlueEntry.Text;
        if (!string.IsNullOrWhiteSpace(alueNimi))
        {
            DatabaseConnection dbc = new DatabaseConnection();
            using (var connection = dbc._getConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("INSERT INTO vn.alue (nimi) VALUES (@nimi)", connection);
                command.Parameters.AddWithValue("@nimi", alueNimi);
                command.ExecuteNonQuery();
            }
            LoadData();
            AlueEntry.Text = string.Empty;
        }
    }

    private void DeleteClicked(object sender, EventArgs e)
    {
        var selectedAlue = AlueList.FirstOrDefault(a => a.nimi == AlueEntry.Text);
        if (selectedAlue != null)
        {
            DatabaseConnection dbc = new DatabaseConnection();
            using (var connection = dbc._getConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("DELETE FROM vn.alue WHERE alue_id = @alueId", connection);
                command.Parameters.AddWithValue("@alueId", selectedAlue.alueId);
                command.ExecuteNonQuery();
            }
            AlueList.Remove(selectedAlue);
            AlueEntry.Text = string.Empty;
        }
    }

    public void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedAlue = e.CurrentSelection[0] as Alue;
            if (selectedAlue != null)
            {
                AlueEntry.Text = selectedAlue.nimi;
            }
        }
    }
}

public class Alue
{
    public int alueId { get; set; }
    public string nimi { get; set; }
}