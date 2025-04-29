using System.Collections.ObjectModel;
using System.ComponentModel;
using MySqlConnector;
using OhjelmistoTuotanto1.Data;
using OhjelmistoTuotanto1.Models;


public class PalveluReportViewModel : INotifyPropertyChanged
{
    private DateTime _startDate = DateTime.Now;
    private DateTime _endDate = DateTime.Now;

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (_startDate != value)
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(StartDateFormatted));
            }
        }
    }

    public string StartDateFormatted => StartDate.ToString("yyyy/MM/dd");

    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            _endDate = value;
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(EndDateFormatted));
        }
    }
    public string EndDateFormatted => EndDate.ToString("yyyy/MM/dd");
     


    public event PropertyChangedEventHandler PropertyChanged;


    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}