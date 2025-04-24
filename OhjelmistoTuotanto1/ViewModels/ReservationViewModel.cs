
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OhjelmistoTuotanto1.Models;
using OhjelmistoTuotanto1.Data;


namespace OhjelmistoTuotanto1.ViewModels
{

    public class ReservationViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Asiakas> Customers { get; set; } = new();
        public ObservableCollection<Mokki> Cottages { get; set; } = new();
        public ObservableCollection<Palvelu> Services { get; set; } = new();
        public bool ShowCottagePicker => StartDate < EndDate;

        private Asiakas _selectedCustomer;
        public Asiakas SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(nameof(SelectedCustomer)); }
        }

        private Mokki _selectedCottage;
        public Mokki SelectedCottage
        {
            get => _selectedCottage;
            set { _selectedCottage = value; OnPropertyChanged(nameof(SelectedCottage)); }
        }

        private DateTime _startDate { get; set; } = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if(_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(ShowCottagePicker));
                    _ = ReloadCottagesAsync();
                }
            }
        }

        private DateTime _endDate { get; set; } = DateTime.Today.AddDays(1);
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                    OnPropertyChanged(nameof(ShowCottagePicker));
                    _ = ReloadCottagesAsync();
                }
            }
        }
        private async Task ReloadCottagesAsync()
        {
            var cottages = await Database.GetAvailableCottagesAsync(StartDate, EndDate);

            Cottages.Clear();
            foreach (var c in cottages)
                Cottages.Add(c);
        }

        public ICommand ReserveCommand { get; }

        public ReservationViewModel()
        {
            ReserveCommand = new Command(async () => await MakeReservationAsync());
        }

        public async Task LoadDataAsync()
        {
            var customers = await Database.GetCustomersAsync();
            var cottages = await Database.GetCottagesAsync();
            var services = await Database.GetServicesAsync();
            var availableCottages = await Database.GetAvailableCottagesAsync(StartDate, EndDate);
            
            Customers.Clear();
            foreach (var c in customers)
                Customers.Add(c);

            Cottages.Clear();
            foreach (var c in cottages)
                Cottages.Add(c);

            Services.Clear();
            foreach (var s in services)
            {
                s.Quantity = 0; // Initialize Quantity for UI input
                Services.Add(s);
            }
        }

        private async Task MakeReservationAsync()
        {

            try
            {
                if (SelectedCottage == null || SelectedCustomer == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Sinun täytyy valita asiakas ja mökki.", "OK");
                    return;
                }

                if (StartDate > EndDate)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Varauksen loppu ei saa olla ennen varauksen alkua", "OK");
                    EndDate = StartDate;
                }

                var varaus = new Varaus
                {
                    AsiakasId = SelectedCustomer.AsiakasId,
                    MokkiId = SelectedCottage.MokkiId,
                    VarattuPvm = DateTime.Now,
                    VahvistusPvm = DateTime.Now,
                    VarattuAlkupvm = StartDate,
                    VarattuLoppupvm = EndDate
                };

                int varausId = await Database.InsertReservationAsync(varaus);

                foreach (var service in Services.Where(s => s.Quantity > 0))
                {
                    await Database.InsertReservationServiceAsync(varausId, service.PalveluId, service.Quantity);
                }

                int days = (EndDate - StartDate).Days;
                double cottageTotal = SelectedCottage.Hinta * days;
                double serviceTotal = await Database.CalculateTotalAsync(varausId);
                double total = cottageTotal + serviceTotal;

                await Database.InsertInvoiceAsync(new Lasku
                {
                    VarausId = varausId,
                    Summa = total,
                    Alv = total * 0.24,
                    Maksettu = false
                });

                await Application.Current.MainPage.DisplayAlert("Success", "Reservation created!", "OK");
                await Application.Current.MainPage.Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
