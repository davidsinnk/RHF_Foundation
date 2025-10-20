using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHF_Foundation.Views.Popups;
using CommunityToolkit.Mvvm.Messaging;

using RHF_Foundation.Views;
using RHF_Foundation.Services.Interfaces;
using RHF_Foundation.Services;




using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using RHF_Foundation.Models;
using System.Windows.Input;
using RHF_Foundation.Messaging;


namespace RHF_Foundation.ViewModels;


public partial class MainViewModel : ObservableObject
{

//Observable properties used for the View
    [ObservableProperty]
    public string camperName = "RHF_Foundation Family";

    [ObservableProperty]
    private DateTime today = DateTime.Today;

    [ObservableProperty]
    private String temperature= "80 °F ";
    [ObservableProperty]
    private String weatherCondition = "Sunny";
    [ObservableProperty]
    private String wind = "7 mph NW";
    public ObservableCollection<EventItem> Events { get; } = new();
    public ObservableCollection<CampEvent> TodayEvents { get; } = new();
    public ObservableCollection<Announcement> Announcements { get; } = new();


// Used for Testing Geofencing
    [ObservableProperty]
    private string statusText = "Roaming";

        // TODO: change to your target coordinates
    private const double TargetLat = 35.02637282158545;
    private const double TargetLon = -78.9730348411858;
    private const double RadiusMeters = 300;


//Buttons Commands
    public IAsyncRelayCommand GoScheduleCommand { get; }
    public IAsyncRelayCommand GoMapCommand { get; }
    public IAsyncRelayCommand GoMealsCommand { get; }
    public IAsyncRelayCommand GoActivitiesCommand { get; }
    public IAsyncRelayCommand GoAnnouncementsCommand { get; }
    public IAsyncRelayCommand CheckInCommand { get; }


    //Navigation, Notification, Geofence services
    private readonly IGeofenceService _geofence;
    private readonly INotificationService _notify;
    private readonly INavigationService _nav;
    private string? _lastArrivalPlaceId;
    private DateTime _lastArrivalTimeUtc;
    private bool _isNavigating;


    public MainViewModel(IGeofenceService geofence, INotificationService notify, INavigationService nav)
    {
        _geofence = geofence;
        _notify = notify;
        _nav = nav;

        
        GoScheduleCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("//schedule"));
        GoMapCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("//map"));
        GoMealsCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("meals"));
        GoActivitiesCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("activities"));
        GoAnnouncementsCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("announcements"));
        CheckInCommand = new AsyncRelayCommand(OpenCheckInPopupAsync);


        // Sample data
        TodayEvents.Add(new CampEvent("Breakfast", "Dining Hall", new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0)));
        TodayEvents.Add(new CampEvent("Lake Time", "Boathouse", new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)));
        TodayEvents.Add(new CampEvent("Arts & Crafts", "Makers Cabin", new TimeSpan(14, 0, 0), new TimeSpan(15, 30, 0)));


        Announcements.Add(new Announcement("Welcome Night!", "Campfire starts at 7:30 PM by the amphitheater."));
        Announcements.Add(new Announcement("Cabin Checks", "Please complete safety checks by noon."));


        CreateEventDataForDemo();

        StartGeofenceMonitoring();


        StartGeofenceAsync();

    }

    private async Task OpenCheckInPopupAsync()
    {

        var popup = new CheckInPopup();
        //var popup = new CheckInPage(new CheckInViewModel(new Services.AlertService(), new Services.NavigationService()));
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);

    }

    private async Task ScanQrAsync()
    {
        // TODO: Integrate ZXing.Net.MAUI or CameraView for scanning
        await Application.Current.MainPage.DisplayAlert("Check-In", "QR scanner coming soon.", "OK");
    }

    public ICommand OpenLinkCommand => new Command<string>(async (url) =>
    {
        if (string.IsNullOrWhiteSpace(url))
            return;
        try
        {
            await Launcher.OpenAsync(new Uri(url));
        }
        catch
        {
            // Optional: handle error or show alert
        }
    });



    private void CreateEventDataForDemo()
    {
        Events.Add(new EventItem
        {
            MonthAbbrev = "OCT",
            EventName = "Spooky Movie Night",
            EventDate = "Oct 08, 2025",
            TimeRange = "05:00 PM – 9:00 PM",
            LinkText = "View event details",
            LinkUrl = "https://rhfnow.org/event/outdoor-movie-night/"
        });

        Events.Add(new EventItem
        {
            MonthAbbrev = "NOV",
            EventName = "Falling Into Christmas",
            EventDate = "Nov 15, 2025",
            TimeRange = "10:00 AM",
            LinkText = "View event details",
            LinkUrl = "https://rhfnow.org/event/november-fall-festival/"
        });

        Events.Add(new EventItem
        {
            MonthAbbrev = "DEC",
            EventName = "B1G Championship Night",
            EventDate = "DEC 06, 2025",
            TimeRange = "6:00 PM – 10:00 PM",
            LinkText = "View event details",
            LinkUrl = "https://example.com/event/789"
        });


    }

    void StartGeofenceMonitoring()
    {
        // Implementation for starting geofence monitoring
        // Listen for platform geofence arrival events (from receiver/delegate)
        WeakReferenceMessenger.Default.Register<ArrivedMessage>(this, async (r, m) =>
        {
            var now = DateTime.UtcNow;
            if (_isNavigating || (_lastArrivalPlaceId == m.Value && (now - _lastArrivalTimeUtc) < TimeSpan.FromSeconds(5)))
                return;

            _isNavigating = true;
            _lastArrivalPlaceId = m.Value;
            _lastArrivalTimeUtc = now;

            try
            {
                StatusText = "You are at Rick's Plase";
                await _nav.GoToAsync("checkin", new Dictionary<string, object> { ["placeId"] = m.Value });
            }
            finally
            {
                await Task.Delay(500);
                _isNavigating = false;
            }


        });

        StatusText = $"Location: {TargetLat}, {TargetLon}";
    }

    [RelayCommand]
    private async Task StartGeofenceAsync()
    {
        var perm = await _notify.RequestPermissionAsync(); // notifications (Android 13+)
        var ok = await _geofence.StartMonitoringAsync(TargetLat, TargetLon, RadiusMeters);
        StatusText = ok ? "Geofence active." : "Could not start geofence (permissions?).";


        if (ok)
        {
            // Optional: immediate hint
            await _notify.ShowAsync("Monitoring", "We'll notify you when you're within 300m.");
        }
    }

}


public record CampEvent(string Title, string Location, TimeSpan Start, TimeSpan End)
{
public string TimeRange => $"{Start:h\\:mm}–{End:h\\:mm}";
}

public record Announcement(string Title, string Body);



