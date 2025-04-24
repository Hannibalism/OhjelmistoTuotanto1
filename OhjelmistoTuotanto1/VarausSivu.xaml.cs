using OhjelmistoTuotanto1.Data;
using OhjelmistoTuotanto1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime;
using OhjelmistoTuotanto1.ViewModels;


namespace OhjelmistoTuotanto1;

public partial class VarausSivu : ContentPage
{
    public ReservationViewModel ViewModel { get; }
    private List<Asiakas> _customers;
    private List<Mokki> _cottages;
    private List<Palvelu> _services;
    private List<Palvelu> _selectedServices;

    public VarausSivu()
	{
		InitializeComponent();
        ViewModel = new ReservationViewModel();
        BindingContext = ViewModel;
        //_selectedServices = new List<Palvelu>();
        //LoadData();
        
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadDataAsync();
    }
/*
    private async void LoadData()
    {
        _customers = await Database.GetCustomersAsync();
        _cottages = await Database.GetCottagesAsync();
        _services = await Database.GetServicesAsync();

        CustomerPicker.ItemsSource = _customers;
        CottagePicker.ItemsSource = _cottages;
        ServiceListView.ItemsSource = _services;
    }

    private void OnDateChanged(object sender, DateChangedEventArgs e)
    {
        var startDate = StartDatePicker.Date;
        var endDate = StartDatePicker.Date;
        if (startDate > endDate)
        {
            DisplayAlert("Error", "Varauksen loppu ei saa olla ennen varauksen alkua", "OK");
            EndDatePicker.Date = startDate;
        }
    }
    private void OnServiceSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var selectedService = e.SelectedItem as Palvelu;
        if (selectedService != null)
        {
            _selectedServices.Add(selectedService);
        }
    }

    private async void OnSubmitReservation(object sender, EventArgs e)
    {
        var selectedCustomer = CustomerPicker.SelectedItem as Asiakas;
        var selectedCottage = CottagePicker.SelectedItem as Mokki;
        var startDate = StartDatePicker.Date;
        var endDate = EndDatePicker.Date;

        if (selectedCustomer == null || selectedCottage == null)
        {
            await DisplayAlert("Error", "Mökki ja Asiakas pitää molemmat olla valittuna", "Ok");
            return;
        }

        if (endDate < startDate)
        {
            await DisplayAlert("Error", "Varauksen loppu ei saa olla ennen varauksen alkua", "OK");
            return;
        }

        var reservation = new Varaus
        {
            AsiakasId = selectedCustomer.AsiakasId,
            MokkiId = selectedCottage.MokkiId,
            VarattuPvm = DateTime.Now,
            VahvistusPvm = DateTime.Now,
            VarattuAlkupvm = startDate,
            VarattuLoppupvm = endDate,
        };

        var reservationId = await Database.InsertReservationAsync(reservation);

        foreach (var service in _selectedServices)
        {
            await Database.InsertReservationServiceAsync(reservationId, service.PalveluId, 1);
        }

        var totalAmount = await Database.CalculateTotalAsync(reservationId);
        var lasku = new Lasku
        {
            VarausId = reservationId,
            Summa = totalAmount,
            Alv = totalAmount * 0.24,
            Maksettu = false
        };
        
        await Database.InsertInvoiceAsync(lasku);

        await DisplayAlert("Success", "Varaus luotu onnistuneesti!", "OK");
    }*/
}