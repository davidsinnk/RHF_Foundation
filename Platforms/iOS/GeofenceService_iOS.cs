using CoreLocation;
using Foundation;
using UserNotifications;
using CommunityToolkit.Mvvm.Messaging;
using RHF_Foundation.Messaging;
using RHF_Foundation.Services;
using RHF_Foundation.Services.Interfaces;
using UIKit;
using RHF_Foundation.Services.Messaging;
using RHF_Foundation.Services.APIs;



namespace RHF_Foundation.Platforms.iOS;


public class GeofenceService_iOS : NSObject, IGeofenceService
{
const string RegionId = "hq-300m";
    readonly CLLocationManager _manager = new();

    public GeofenceService_iOS()
    {
        _manager.Delegate = new GeofenceDelegate();
        _manager.AllowsBackgroundLocationUpdates = true;
        _manager.PausesLocationUpdatesAutomatically = false;
    }


public async Task<bool> StartMonitoringAsync(double latitude, double longitude, double radiusMeters)
    {
        var status = await RequestPermissionsAsync();
        if (status is not CLAuthorizationStatus.AuthorizedAlways)
        return false;


        var center = new CLLocationCoordinate2D(latitude, longitude);
        var region = new CLCircularRegion(center, radiusMeters, RegionId)
        {
        NotifyOnEntry = true,
        NotifyOnExit = false
        };


        _manager.StartMonitoring(region);
        _manager.RequestState(region); // may trigger immediately if inside
        return true;
    }


public Task StopMonitoringAsync()
    {
        foreach (var r in _manager.MonitoredRegions.OfType<CLCircularRegion>().Where(r => r.Identifier == RegionId))
        _manager.StopMonitoring(r);
        return Task.CompletedTask;
    }


    static async Task<CLAuthorizationStatus> RequestPermissionsAsync()
    {
        // Current status
        var status = CLLocationManager.Status;

        // Ask only if undecided
        if (status == CLAuthorizationStatus.NotDetermined)
        {
            using var m = new CLLocationManager();
            m.RequestAlwaysAuthorization();

            // Wait briefly for the user's choice to resolve
            for (int i = 0; i < 20; i++) // ~10 seconds max
            {
                await Task.Delay(500);
                status = CLLocationManager.Status;
                if (status != CLAuthorizationStatus.NotDetermined)
                    break;
            }
        }

        // Always return a status
        return status;
    }

    sealed class GeofenceDelegate : CLLocationManagerDelegate
    {

        //yes yes... I know this is tightly couples and need to be refed to the constructor or DI service
        private ICheckInAPIService _checkInAPIService = new CheckInAPIService();
            
        // NOTE: In .NET for iOS, the managed override name is RegionEntered (not DidEnterRegion)
        public override void RegionEntered(CLLocationManager manager, CLRegion region)
        {
            if (region?.Identifier == "hq-300m")
            {
                
                _checkInAPIService.CheckInArrival();;
                
                
                var isForeground = UIApplication.SharedApplication.ApplicationState == UIApplicationState.Active;

                if (isForeground)
                {
                    WeakReferenceMessenger.Default.Send(new ArrivedMessage("hq-300m"));
                }
                else
                {
                    NotificationService_iOS.ShowNow(
                        "Arrived at Rick's Place",
                        "Please Check In",
                        "checkin?placeId=hq-300m"
                    );
                }
            }
        }
    
    public override void RegionLeft(CLLocationManager manager, CLRegion region)
    {
        if (region?.Identifier == "hq-300m")
        {
            var appState = UIApplication.SharedApplication?.ApplicationState ?? UIApplicationState.Inactive;
            var isForeground = appState == UIApplicationState.Active;


            if (isForeground)
            {
                WeakReferenceMessenger.Default.Send(new DepartedMessage("hq-300m"));
            }
            else
            {
                NotificationService_iOS.ShowNow("You left the area", "You're now outside 300m.", null);
            }
        }
    }

    // Keep this a no-op to avoid double-firing with RegionEntered
    public override void DidDetermineState(CLLocationManager manager, CLRegionState state, CLRegion region) { }
    }
}