using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OhjelmistoTuotanto1.Models;
using OhjelmistoTuotanto1.Data;
using MySqlConnector;
using System.Data;

namespace OhjelmistoTuotanto1.Data
{
    public static class Database
    {
        private static readonly DatabaseConnection dbConnection = new DatabaseConnection();
        private static MySqlConnection GetConnection() => dbConnection._getConnection();

        public static async Task<List<Asiakas>> GetCustomersAsync()
        {
            var customers = new List<Asiakas>();

            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT * FROM asiakas", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new Asiakas
                {
                    AsiakasId = reader.GetInt32("asiakas_id"),
                    Etunimi = reader.GetString("etunimi"),
                    Sukunimi = reader.GetString("sukunimi"),
                    Email = reader.GetString("email"),
                    Puhelinnro = reader.GetString("puhelinnro"),
                    Lahiosoite = reader.GetString("lahiosoite"),
                    Postinro = reader.GetString("postinro")
                });
            }

            return customers;
        }

        public static async Task<List<MokkiModel>> GetCottagesAsync()
        {
            var list = new List<MokkiModel>();
            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT m.*, p.toimipaikka FROM mokki m JOIN posti p ON m.postinro = p.postinro", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MokkiModel
                {
                    MokkiId = reader.GetInt32("mokki_id"),
                    Mokkinimi = reader.GetString("mokkinimi"),
                    AlueId = reader.GetInt32("alue_id"),
                    Hinta = reader.GetDouble("hinta"),
                    Postinro = reader.GetString("postinro"),
                    Katuosoite = reader.GetString("katuosoite"),
                    Kuvaus = reader.GetString("kuvaus"),
                    Henkilomaara = reader.GetInt32("henkilomaara"),
                    Varustelu = reader.GetString("varustelu"),
                    Toimipaikka = reader.GetString("toimipaikka")
                });
            }

            return list;
        }

        public static async Task<List<MokkiModel>> GetAvailableCottagesAsync(DateTime start, DateTime end)
        {
            var cottages = new List<MokkiModel>();

            using var connection = new DatabaseConnection()._getConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT m.*, p.toimipaikka
                FROM mokki m
                JOIN posti p ON m.postinro = p.postinro
                WHERE m.mokki_id NOT IN (
                SELECT mokki_id FROM varaus
                WHERE (varattu_alkupvm <= @end AND varattu_loppupvm >= @start)
                )";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var mokki = new MokkiModel
                {
                    MokkiId = reader.GetInt32("mokki_id"),
                    AlueId = reader.GetInt32("alue_id"),
                    Postinro = reader.GetString("postinro"),
                    Mokkinimi = reader.GetString("mokkinimi"),
                    Katuosoite = reader.GetString("katuosoite"),
                    Hinta = reader.GetDouble("hinta"),
                    Kuvaus = reader.GetString("kuvaus"),
                    Henkilomaara = reader.GetInt32("henkilomaara"),
                    Varustelu = reader.GetString("varustelu"),
                    Toimipaikka = reader.GetString("toimipaikka")
                };

                cottages.Add(mokki);
            }

            return cottages;
        }

        public static async Task<List<Palvelu>> GetServicesAsync()
        {
            var list = new List<Palvelu>();
            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT * FROM palvelu", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Palvelu
                {
                    PalveluId = reader.GetInt32("palvelu_id"),
                    AlueId = reader.GetInt32("alue_id"),
                    Nimi = reader.GetString("nimi"),
                    Kuvaus = reader.GetString("kuvaus"),
                    Hinta = reader.GetDouble("hinta"),
                    Alv = reader.GetDouble("alv")
                });
            }

            return list;
        }

        // TODO: Add methods:
        
        public static async Task<int> InsertReservationAsync(Varaus varaus)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(@"
                INSERT INTO varaus 
                (asiakas_id, mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm) 
                VALUES (@asiakas_id, @mokki_id, @varattu_pvm, @vahvistus_pvm, @alku, @loppu);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@asiakas_id", varaus.AsiakasId);
            cmd.Parameters.AddWithValue("@mokki_id", varaus.MokkiId);
            cmd.Parameters.AddWithValue("@varattu_pvm", varaus.VarattuPvm);
            cmd.Parameters.AddWithValue("@vahvistus_pvm", varaus.VahvistusPvm);
            cmd.Parameters.AddWithValue("@alku", varaus.VarattuAlkupvm);
            cmd.Parameters.AddWithValue("@loppu", varaus.VarattuLoppupvm);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        

        public static async Task InsertReservationServiceAsync(int varausId, int palveluId, int lkm)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(@"
                INSERT INTO varauksen_palvelut 
                (varaus_id, palvelu_id, lkm) 
                VALUES (@varaus_id, @palvelu_id, @lkm);", conn);

            cmd.Parameters.AddWithValue("@varaus_id", varausId);
            cmd.Parameters.AddWithValue("@palvelu_id", palveluId);
            cmd.Parameters.AddWithValue("@lkm", lkm);

            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task InsertInvoiceAsync(Lasku lasku)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(@"
                INSERT INTO lasku 
                (lasku_id, varaus_id, summa, alv, maksettu) 
                VALUES (@varaus_id, @varaus_id, @summa, @alv, @maksettu);", conn);

            cmd.Parameters.AddWithValue("@varaus_id", lasku.VarausId);
            cmd.Parameters.AddWithValue("@summa", lasku.Summa);
            cmd.Parameters.AddWithValue("@alv", lasku.Alv);
            cmd.Parameters.AddWithValue("@maksettu", lasku.Maksettu);

            await cmd.ExecuteNonQueryAsync();
        }


        public static async Task<double> CalculateTotalAsync(int varausId)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(@"
                SELECT SUM(p.hinta * vp.lkm) AS total
                FROM varauksen_palvelut vp
                JOIN palvelu p ON vp.palvelu_id = p.palvelu_id
                WHERE vp.varaus_id = @varaus_id;", conn);

            cmd.Parameters.AddWithValue("@varaus_id", varausId);

            var result = await cmd.ExecuteScalarAsync();
            return result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
        }
    }
}
