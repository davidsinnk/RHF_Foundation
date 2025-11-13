using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHF_Foundation.Views.PopUps;
using CommunityToolkit.Mvvm.Messaging;
using RHF_Foundation.Views;
using CommunityToolkit.Maui.Views;
using RHF_Foundation.Services.Interfaces;
using RHF_Foundation.Services;
using CommunityToolkit.Maui.Extensions;
using RHF_Foundation.Models;
using System.Windows.Input;
using RHF_Foundation.Messaging;
using RHF_Foundation.Services.Messaging;
using RHF_Foundation.Services.APIs;


namespace RHF_Foundation.ViewModels;


public partial class MainViewModel : ObservableObject
{

    //Observable properties used for the View
    [ObservableProperty]
    public string camperName = "RHF_Foundation Family";

    [ObservableProperty]
    private DateTime today = DateTime.Today;

    [ObservableProperty]
    private String temperature = "80 °F ";
    [ObservableProperty]
    private String weatherCondition = "Sunny";
    [ObservableProperty]
    private String wind = "7 mph NW";
    public ObservableCollection<EventItem> Events { get; } = new();
    public ObservableCollection<CampEvent> TodayEvents { get; } = new();
    public ObservableCollection<Announcement> Announcements { get; } = new();

    private ICheckInAPIService _checkInAPIService;

    // TODO: change to your target coordinates
    private const double TargetLat = 35.02637282158545;
    private const double TargetLon = -78.9730348411858;
    private const double RadiusMeters = 300;

    [ObservableProperty]
    private int numberOfVisits = Preferences.Get("NumberOfVisits", 0);


    //Buttons Commands
    public IAsyncRelayCommand GoScheduleCommand { get; }
    public IAsyncRelayCommand GoMapCommand { get; }
    public IAsyncRelayCommand GoMealsCommand { get; }
    public IAsyncRelayCommand GoActivitiesCommand { get; }
    public IAsyncRelayCommand GoAnnouncementsCommand { get; }
    public IAsyncRelayCommand CheckInCommand { get; }

    public IAsyncRelayCommand TestPagePopup { get; }


    //Navigation, Notification, Geofence services
    private readonly IGeofenceService _geofence;
    private readonly INotificationService _notify;
    private readonly INavigationService _nav;
    private string? _lastArrivalPlaceId;
    private DateTime _lastArrivalTimeUtc;
    private bool _isNavigating;
    private bool _popupOpen;


    public MainViewModel(IGeofenceService geofence, INotificationService notify, INavigationService nav)
    {
        _geofence = geofence;
        _notify = notify;
        _nav = nav;

        //yes... we need to talk about this later...
        //and put htis in the constructor for the DI service.... 
        _checkInAPIService = new CheckInAPIService();

        _popupOpen = true;

        GoScheduleCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("//schedule"));
        GoMapCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("//map"));
        GoMealsCommand = new AsyncRelayCommand(async () => await ScanQrAsync());
        GoActivitiesCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("activities"));
        GoAnnouncementsCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("announcements"));
        CheckInCommand = new AsyncRelayCommand(OpenCheckInPopupAsync);

        TestPagePopup = new AsyncRelayCommand(async () => await TestPagePopupAsync());


        // Sample data
        TodayEvents.Add(new CampEvent("Breakfast", "Dining Hall", new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0)));
        TodayEvents.Add(new CampEvent("Lake Time", "Boathouse", new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)));
        TodayEvents.Add(new CampEvent("Arts & Crafts", "Makers Cabin", new TimeSpan(14, 0, 0), new TimeSpan(15, 30, 0)));


        Announcements.Add(new Announcement("Welcome Night!", "Campfire starts at 7:30 PM by the amphitheater."));
        Announcements.Add(new Announcement("Cabin Checks", "Please complete safety checks by noon."));


        CreateEventDataForDemo();

        StartGeofenceMonitoring();

        // Auto-start geofencing on app launch
        _ = Task.Run(async () => await AutoStartGeofencingAsync());

    }

    private async Task OpenCheckInPopupAsync()
    {

        //var popup = new CheckInPopup();
        //await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "userName", "Dave" }
        };

        await _nav.GoToAsync("checkin", parameters);

    }


    private async Task TestPagePopupAsync()
    {

        //var popup = new CheckInPopup();
        //await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "userName", "Dave" }
        };

        await _nav.GoToAsync("getinvolved", parameters);

    }


    private async Task ScanQrAsync()
    {
        // TODO: Integrate ZXing.Net.MAUI or CameraView for scanning
        //await Application.Current.MainPage.DisplayAlert("Check-In", "QR scanner coming soon.", "OK");

        await _notify.ShowAsync("Check-In", "QR scanner coming soon.");
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
            MonthAbbrev = "Weekly",
            EventName = "Messy Mondays",
            EventDate = "Mondays",
            TimeRange = "1 PM or 5:30 PM",
            LinkText = "Get Messy here",
            LinkUrl = "https://rhfnow.org/program/messy-mondays-2/"
        });

        Events.Add(new EventItem
        {
            MonthAbbrev = "Weekly",
            EventName = "Bend & Brew (Yoga & Coffee)",
            EventDate = "Thursdays",
            TimeRange = "9:30am – 10:30am",
            LinkText = "Stretch out for details",
            LinkUrl = "https://rhfnow.org/program/bend-brew-yoga-coffee/"
        }); 

        Events.Add(new EventItem
        {
            MonthAbbrev = "Coming Soon",
            EventName = "Weekend at Rick’s Place",
            EventDate = "Check back in 2026!",
            //TimeRange = "5:30 PM – 6:30 PM",
            LinkText = "View event details",
            LinkUrl = "https://rhfnow.org/program/dad-me-weekend/"
        });

        Events.Add(new EventItem
        {
            MonthAbbrev = "DEC",
            EventName = "RHF App Launch (Tenative)",
            EventDate = "DEC 01, 2025",
            TimeRange = "All Day",
            //LinkText = "View event details",
            //LinkUrl = "https://example.com/event/789"
        });

        Events.Add(new EventItem
        {
            MonthAbbrev = "May",
            EventName = "Towel Day",
            EventDate = "May 25, 2026",
            TimeRange = "All Day",
            LinkText = "Don't Panic! and click here for details",
            LinkUrl = "https://en.wikipedia.org/wiki/Towel_Day"
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
                await _checkInAPIService.CheckInArrival();
                await _nav.GoToAsync("checkin", new Dictionary<string, object> { ["placeId"] = m.Value });
            }
            finally
            {
                await Task.Delay(500);
                _isNavigating = false;
            }


        });


        WeakReferenceMessenger.Default.Register<DepartedMessage>(this, (r, m) =>
        {
            _lastArrivalPlaceId = null;   // allow next arrival to navigate again
            _notify.ShowAsync("Leaving", "You have left the geofenced area.");
        });


    }

    [RelayCommand]
    private async Task StartGeofenceAsync()
    {
        var perm = await _notify.RequestPermissionAsync(); // notifications (Android 13+)

#if ANDROID

        await BeginMonitoringAsync();

#endif

        var ok = await _geofence.StartMonitoringAsync(TargetLat, TargetLon, RadiusMeters);

        if (ok)
        {
            // Optional: immediate hint
            //This may be overkill... it sedn an alert when you start monitoring.
            //await _notify.ShowAsync("Monitoring", "We'll notify you when you're within 300m.");
        }

        //await _notify.ShowAsync("Test Notification", "Geofence monitoring started.");


    }

    [RelayCommand]
    private async Task BeginMonitoringAsync()
    {
        System.Diagnostics.Debug.WriteLine("[VM] BeginMonitoringAsync() called");

        // 1) Foreground location
        var fg = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (fg != PermissionStatus.Granted)
            fg = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (fg != PermissionStatus.Granted)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Permission needed",
                "Location permission is required to monitor your location.",
                "OK");
            return;
        }

#if ANDROID
        // 2) Background location (Android 10+/API29+)
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
        {
            var bg = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (bg != PermissionStatus.Granted)
                bg = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (bg != PermissionStatus.Granted)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Background Location Required",
                    "Please allow 'Always' location in Settings so we can detect arrival.",
                    "Open Settings");
                Microsoft.Maui.ApplicationModel.AppInfo.ShowSettingsUI();
                return;
            }
        }

        // 3) Notifications (Android 13+/API33+)
        var notificationAllowed = await _notify.RequestPermissionAsync();
        if (!notificationAllowed)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Notifications blocked",
                "Please enable notifications so we can alert you when you arrive.",
                "Open Settings");
            Microsoft.Maui.ApplicationModel.AppInfo.ShowSettingsUI();
        }

        // 4) Register geofence with platform geofencing system for background monitoring
        await RHF_Foundation.Platforms.Android.GeofenceRegistrar_Android
            .RegisterAsync(global::Android.App.Application.Context,
                    TargetLat, TargetLon, (float)RadiusMeters,
                    "hq-300m");
#endif

        // Start fence monitoring
        var ok = await _geofence.StartMonitoringAsync(TargetLat, TargetLon, RadiusMeters);
        System.Diagnostics.Debug.WriteLine($"[VM] StartMonitoringAsync returned {ok}");
    }

    private async Task AutoStartGeofencingAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[VM] AutoStartGeofencingAsync() - Starting automatic permission requests and geofencing");
            
            // Small delay to ensure UI is ready
            await Task.Delay(2000);
            
            // Check if permissions are already granted
            var locationGranted = await CheckLocationPermissionsAsync();
            var notificationGranted = await _notify.RequestPermissionAsync();
            
            System.Diagnostics.Debug.WriteLine($"[VM] Initial permission check - Location: {locationGranted}, Notifications: {notificationGranted}");

            if (!locationGranted || !notificationGranted)
            {
                // Show instructions to user for manual setup
                await ShowPermissionInstructionsAsync();
            }

            if (locationGranted)
            {
                // Start geofencing automatically
                await StartGeofenceAsync();
                System.Diagnostics.Debug.WriteLine("[VM] Auto-geofencing started successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[VM] Cannot start geofencing - location permissions denied");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] AutoStartGeofencingAsync error: {ex.Message}");
        }
    }

    private async Task<bool> CheckLocationPermissionsAsync()
    {
        try
        {
            var fg = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                var bg = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                return fg == PermissionStatus.Granted && bg == PermissionStatus.Granted;
            }
#endif
            return fg == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] CheckLocationPermissionsAsync error: {ex.Message}");
            return false;
        }
    }

    private async Task ShowPermissionInstructionsAsync()
    {
        try
        {
            var tcs = new TaskCompletionSource<bool>();
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var currentPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
                    if (currentPage != null)
                    {
                        var result = await currentPage.DisplayAlert(
                            "Setup Required",
                            "For geofencing to work properly, please:\n\n" +
                            "1. Enable Location: Set to 'Allow all the time'\n" +
                            "2. Enable Notifications\n\n" +
                            "Tap 'Open Settings' to configure these permissions now.",
                            "Open Settings",
                            "Later");
                        
                        if (result == true)
                        {
                            Microsoft.Maui.ApplicationModel.AppInfo.ShowSettingsUI();
                        }
                    }
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[VM] Error showing permission instructions: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            
            await tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] ShowPermissionInstructionsAsync error: {ex.Message}");
        }
    }

    private async Task<bool> RequestLocationPermissionsAsync()
    {
        try
        {
            // Check if we've already requested permissions and been denied
            var prefsKey = "LocationPermissionsRequested";
            var alreadyRequested = Preferences.Get(prefsKey, false);

            // 1) Foreground location
            var fg = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (fg != PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("[VM] Requesting foreground location permission");
                fg = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                // Save that we've requested permissions
                Preferences.Set(prefsKey, true);
            }

            if (fg != PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("[VM] Foreground location permission denied");

                // Show alert for user to manually enable in settings
                if (alreadyRequested)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            var currentPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
                            if (currentPage != null)
                            {
                                var result = await currentPage.DisplayAlert(
                                    "Location Permission Required",
                                    "Location permission is required for geofencing. Please enable 'Allow all the time' in Settings > Apps > RHF Foundation > Permissions > Location.",
                                    "Open Settings",
                                    "Cancel");

                                if (result == true)
                                {
                                    Microsoft.Maui.ApplicationModel.AppInfo.ShowSettingsUI();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[VM] Error showing location permission alert: {ex.Message}");
                        }
                    });
                }
                return false;
            }

#if ANDROID
            // 2) Background location (Android 10+/API29+)
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                var bg = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                if (bg != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("[VM] Requesting background location permission");
                    bg = await Permissions.RequestAsync<Permissions.LocationAlways>();
                }

                if (bg != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("[VM] Background location permission denied");
                    
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            var currentPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
                            if (currentPage != null)
                            {
                                var result = await currentPage.DisplayAlert(
                                    "Background Location Required",
                                    "For geofencing to work when the app is closed, please enable 'Allow all the time' for location access in Settings.",
                                    "Open Settings",
                                    "Cancel");
                                
                                if (result == true)
                                {
                                    Microsoft.Maui.ApplicationModel.AppInfo.ShowSettingsUI();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[VM] Error showing background location permission alert: {ex.Message}");
                        }
                    });
                    return false;
                }
            }
#endif

            System.Diagnostics.Debug.WriteLine("[VM] All location permissions granted");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] RequestLocationPermissionsAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task LoadData()
    {
        // Simulate loading data or updating a value
        NumberOfVisits = Preferences.Get("NumberOfVisits", 0);

        if (NumberOfVisits != 1 && NumberOfVisits != 5 && NumberOfVisits != 10 && NumberOfVisits != 20){
            _popupOpen = true;
        }
        else if ( _popupOpen)
        {
            int totalCheckIns = NumberOfVisits;

            _popupOpen = false;
            
            // Show popup through the current page since ViewModels don't have direct UI access
            var currentPage = Application.Current?.MainPage ?? Shell.Current.CurrentPage;
            if (currentPage != null)
            {
                var popup = new CheckInPopup(NumberOfVisits);
                await currentPage.ShowPopupAsync(popup);
            }
        }
    }

}


    public record CampEvent(string Title, string Location, TimeSpan Start, TimeSpan End)
    {
        public string TimeRange => $"{Start:h\\:mm}–{End:h\\:mm}";
    }

    public record Announcement(string Title, string Body);




