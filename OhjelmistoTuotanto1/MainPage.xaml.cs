using MySqlConnector;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using OhjelmistoTuotanto1.Data;

namespace OhjelmistoTuotanto1;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}
    private void LaskuClicked(object sender, EventArgs e) 
    {
        Navigation.PushAsync(new Laskuseuranta());
    }

    private void Mokkilisays(object sender, EventArgs e)
	{
        Navigation.PushAsync(new LisaysSivu());
    }

    private void AsiakasHallintaBtn(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AsiakasHallinta());
    }
    private void MakeReservation(object sender, EventArgs e)
    {
        Navigation.PushAsync(new VarausSivu());
    }
    private void PalveluRaporttiClicked (object sender, EventArgs e)
    {
        Navigation.PushAsync(new PalveluRaporttiPage());
    }
    private void MajoitusRaportClicked(object sender, EventArgs e) 
    {
        Navigation.PushAsync (new MajoitusRaport());
    }
    private async void OnDatabaseClicked(object sender, EventArgs e)
    {
        DatabaseConnection dbc = new DatabaseConnection();
        try
        {
            var conn = dbc._getConnection();
            conn.Open();
            await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK");

            // Lisää dataa tietokantaan, jos se on tyhjä
            await InitializeDatabaseAsync(conn);
        }

        catch (MySqlException ex)
        {
            await DisplayAlert("Failure", ex.Message, "OK");
        }
    }
    
    private async Task<bool> IsTableEmptyAsync(MySqlConnection connection, string tablename)
    {
        // Onko annetussa tablessa mitään
        string query = $"SELECT COUNT(*) FROM {tablename};";
        using (var command = new MySqlCommand(query, connection))
        {
            var count = (long)await command.ExecuteScalarAsync();
            return count == 0; // Palauttaa "true" jos tyhjä
        }
    }

    private async Task SeedDatabaseAsync(MySqlConnection connection)
    {
        // Seed data for `alue` table
        if (await IsTableEmptyAsync(connection, "alue"))
        {
            string insertAlue = @"
            INSERT INTO alue (nimi) VALUES 
            ('Uusimaa'), 
            ('Pirkanmaa'), 
            ('Varsinais-Suomi'), 
            ('Pohjois-Pohjanmaa'), 
            ('Kanta-Häme');";

            using (var command = new MySqlCommand(insertAlue, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `posti` table
        if (await IsTableEmptyAsync(connection, "posti"))
        {
            string insertPosti = @"
            INSERT INTO posti (postinro, toimipaikka) VALUES 
            ('00100', 'Helsinki'), 
            ('00200', 'Helsinki'), 
            ('20100', 'Turku'), 
            ('33100', 'Tampere'), 
            ('90100', 'Oulu');";

            using (var command = new MySqlCommand(insertPosti, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `asiakas` table
        if (await IsTableEmptyAsync(connection, "asiakas"))
        {
            string insertAsiakas = @"
            INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES 
            ('00100', 'John', 'Doe', 'Main Street 1', 'john.doe@example.com', '123456789'), 
            ('00200', 'Jane', 'Smith', 'Second Street 2', 'jane.smith@example.com', '987654321'), 
            ('20100', 'Alice', 'Johnson', 'Third Street 3', 'alice.johnson@example.com', '456789123'), 
            ('33100', 'Bob', 'Williams', 'Fourth Street 4', 'bob.williams@example.com', '321654987'), 
            ('90100', 'Charlie', 'Brown', 'Fifth Street 5', 'charlie.brown@example.com', '789123456');";

            using (var command = new MySqlCommand(insertAsiakas, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `mokki` table
        if (await IsTableEmptyAsync(connection, "mokki"))
        {
            string insertMokki = @"
            INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) VALUES 
            (1, '00100', 'Mökki 1', 'Lakeview Drive 1', 150.00, 'A cozy cottage by the lake.', 5, 'Fireplace, BBQ grill'),
            (2, '20100', 'Mökki 2', 'Forest Road 2', 200.00, 'A cabin in the woods.', 6, 'Sauna, Fishing gear'),
            (3, '33100', 'Mökki 3', 'Mountain View 3', 250.00, 'A beautiful mountain retreat.', 8, 'Ski equipment, Hot tub');";

            using (var command = new MySqlCommand(insertMokki, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `varaus` table
        if (await IsTableEmptyAsync(connection, "varaus"))
        {
            string insertVaraus = @"
            INSERT INTO varaus (asiakas_id, mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm) VALUES 
            (1, 1, NOW(), NOW(), '2025-06-01', '2025-06-07'), 
            (2, 2, NOW(), NOW(), '2025-06-15', '2025-06-20'), 
            (3, 3, NOW(), NOW(), '2025-07-01', '2025-07-10');";

            using (var command = new MySqlCommand(insertVaraus, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `lasku` table
        if (await IsTableEmptyAsync(connection, "lasku"))
        {
            string insertLasku = @"
            INSERT INTO lasku (varaus_id, summa, alv, maksettu) VALUES 
            (1, 1050.00, 210.00, 1), 
            (2, 600.00, 120.00, 0), 
            (3, 2000.00, 400.00, 1);";

            using (var command = new MySqlCommand(insertLasku, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `palvelu` table
        if (await IsTableEmptyAsync(connection, "palvelu"))
        {
            string insertPalvelu = @"
            INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv) VALUES 
            (1, 'Kalastuslupa', 'Lupa kalastukseen alueella.', 30.00, 6.00), 
            (2, 'Sauna', 'Saunomismahdollisuus mökillä.', 50.00, 10.00), 
            (3, 'Vene', 'Veneen vuokraus järvelle.', 100.00, 20.00);";

            using (var command = new MySqlCommand(insertPalvelu, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        // Seed data for `varauksen_palvelut` table
        if (await IsTableEmptyAsync(connection, "varauksen_palvelut"))
        {
            string insertVarauksenPalvelut = @"
            INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm) VALUES 
            (1, 1, 2), 
            (2, 2, 1), 
            (3, 3, 1);";

            using (var command = new MySqlCommand(insertVarauksenPalvelut, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task InitializeDatabaseAsync(MySqlConnection connection)
    {
        
        if (await IsTableEmptyAsync(connection, "asiakas") || await IsTableEmptyAsync(connection, "mokki") ||
            await IsTableEmptyAsync(connection, "varaus") || await IsTableEmptyAsync(connection, "lasku") ||
            await IsTableEmptyAsync(connection, "palvelu") || await IsTableEmptyAsync(connection, "varauksen_palvelut"))
        {
            await SeedDatabaseAsync(connection);
        }
    }
    
    private async void OnNavigateToFilterPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FilterPage());
    }
    private async void OnNavigateToServicesPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PalveluSivu());
    }

    private async void Laskutus_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LaskutusSivu());
    }
}
