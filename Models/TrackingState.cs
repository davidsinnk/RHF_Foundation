using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RHF_Foundation.Models;

// App-wide observable state for UI binding.
public class TrackingState : INotifyPropertyChanged
{
    double _currentLatitude;
    double _currentLongitude;
    bool _isInsideFence;

    public double CurrentLatitude
    {
        get => _currentLatitude;
        set { if (_currentLatitude != value) { _currentLatitude = value; OnPropertyChanged(); } }
    }

    public double CurrentLongitude
    {
        get => _currentLongitude;
        set { if (_currentLongitude != value) { _currentLongitude = value; OnPropertyChanged(); } }
    }

    public bool IsInsideFence
    {
        get => _isInsideFence;
        set { if (_isInsideFence != value) { _isInsideFence = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}