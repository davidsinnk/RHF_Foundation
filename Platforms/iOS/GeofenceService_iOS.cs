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
    readonly ILocationPermissionService _permissionService;

    public GeofenceService_iOS()
    {
        _manager.Delegate = new GeofenceDelegate();
        _permissionService = new LocationPermissionService_iOS();
        
        System.Diagnostics.Debug.WriteLine("[iOS] GeofenceService_iOS initialized with delegate");
        
        // Only set background updates if we have Always permission
        #pragma warning disable CA1422
        var currentStatus = CLLocationManager.Status;
        #pragma warning restore CA1422
        System.Diagnostics.Debug.WriteLine($"[iOS] Current location authorization: {currentStatus}");
        
        if (currentStatus == CLAuthorizationStatus.AuthorizedAlways)
        {
            _manager.AllowsBackgroundLocationUpdates = true;
            _manager.PausesLocationUpdatesAutomatically = false;
            System.Diagnostics.Debug.WriteLine("[iOS] Background location updates enabled");
        }
    }

    public async Task<bool> StartMonitoringAsync(double latitude, double longitude, double radiusMeters)
    {
        System.Diagnostics.Debug.WriteLine($"[iOS] StartMonitoringAsync called for lat={latitude}, lon={longitude}, radius={radiusMeters}m");
        
        // First request when-in-use permission
        var hasPermission = await _permissionService.RequestPermissionsAsync();
        if (!hasPermission)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Location permission denied");
            return false;
        }

        // Then request always permission for background monitoring
        var hasAlwaysPermission = await _permissionService.RequestBackgroundLocationAsync();
        if (hasAlwaysPermission)
        {
            _manager.AllowsBackgroundLocationUpdates = true;
            _manager.PausesLocationUpdatesAutomatically = false;
            System.Diagnostics.Debug.WriteLine("[iOS] Background location permission granted");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Background location permission denied - geofencing will only work when app is active");
        }

        var center = new CLLocationCoordinate2D(latitude, longitude);
        System.Diagnostics.Debug.WriteLine($"[iOS] Creating geofence region with center: {center.Latitude}, {center.Longitude}");
        
        // Use pragma to suppress iOS 17+ warnings since we're targeting iOS 15+
        #pragma warning disable CA1422, CA1416
        var region = new CLCircularRegion(center, radiusMeters, RegionId)
        {
            NotifyOnEntry = true,
            NotifyOnExit = false
        };

        System.Diagnostics.Debug.WriteLine($"[iOS] Starting monitoring for region: {region.Identifier}, NotifyOnEntry: {region.NotifyOnEntry}");
        _manager.StartMonitoring(region);
        _manager.RequestState(region); // may trigger immediately if inside
        #pragma warning restore CA1422, CA1416
        
        System.Diagnostics.Debug.WriteLine($"[iOS] Started monitoring geofence at {latitude}, {longitude} with radius {radiusMeters}m");
        
        // Start location updates to see current position
        _manager.StartUpdatingLocation();
        
        return true;
    }

    public Task StopMonitoringAsync()
    {
        System.Diagnostics.Debug.WriteLine("[iOS] StopMonitoringAsync called");
        
        #pragma warning disable CA1422, CA1416
        foreach (var r in _manager.MonitoredRegions.OfType<CLCircularRegion>().Where(r => r.Identifier == RegionId))
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Stopping monitoring for region: {r.Identifier}");
            _manager.StopMonitoring(r);
        }
        #pragma warning restore CA1422, CA1416
        
        _manager.StopUpdatingLocation();
        return Task.CompletedTask;
    }

    sealed class GeofenceDelegate : CLLocationManagerDelegate
    {
        //yes yes... I know this is tightly couples and need to be refed to the constructor or DI service
        private ICheckInAPIService _checkInAPIService = new CheckInAPIService();

        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Location authorization changed to: {status}");
            
            // Update background location settings if needed
            if (status == CLAuthorizationStatus.AuthorizedAlways)
            {
                manager.AllowsBackgroundLocationUpdates = true;
                manager.PausesLocationUpdatesAutomatically = false;
                System.Diagnostics.Debug.WriteLine("[iOS] Enabled background location updates after authorization change");
            }
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Location manager failed: {error.LocalizedDescription}");
        }

        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            if (locations?.Length > 0)
            {
                var location = locations[0];
                var geofenceCenter = new CLLocationCoordinate2D(35.02637282158545, -78.9730348411858);
                var geofenceLocation = new CLLocation(geofenceCenter.Latitude, geofenceCenter.Longitude);
                var distance = location.DistanceFrom(geofenceLocation);
                
                System.Diagnostics.Debug.WriteLine($"[iOS] Current location: {location.Coordinate.Latitude}, {location.Coordinate.Longitude}");
                System.Diagnostics.Debug.WriteLine($"[iOS] Distance to geofence center: {distance:F1}m (threshold: 300m)");
                
                if (distance <= 300)
                {
                    System.Diagnostics.Debug.WriteLine($"[iOS] *** WITHIN GEOFENCE RADIUS *** (distance: {distance:F1}m)");
                }
            }
        }

        public override void MonitoringFailed(CLLocationManager manager, CLRegion? region, NSError error)
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Monitoring failed for region {region?.Identifier}: {error.LocalizedDescription}");
        }

        public override void DidStartMonitoringForRegion(CLLocationManager manager, CLRegion region)
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Started monitoring region: {region.Identifier}");
            
            // Request initial state to check if already inside
            #pragma warning disable CA1422, CA1416
            manager.RequestState(region);
            #pragma warning restore CA1422, CA1416
        }

        public override void DidDetermineState(CLLocationManager manager, CLRegionState state, CLRegion region)
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Determined state for region {region.Identifier}: {state}");
            
            // If we're already inside when monitoring starts, trigger entry
            if (state == CLRegionState.Inside && region?.Identifier == "hq-300m")
            {
                System.Diagnostics.Debug.WriteLine("[iOS] Already inside geofence - triggering entry event");
                RegionEntered(manager, region);
            }
        }
            
        // NOTE: In .NET for iOS, the managed override name is RegionEntered (not DidEnterRegion)
        public override void RegionEntered(CLLocationManager manager, CLRegion region)
        {
            System.Diagnostics.Debug.WriteLine($"[iOS] Entered region: {region?.Identifier}");
            
            if (region?.Identifier == "hq-300m")
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[iOS] Processing hq-300m entry event");
                    _checkInAPIService.CheckInArrival();
                    
                    var isForeground = UIApplication.SharedApplication.ApplicationState == UIApplicationState.Active;
                    System.Diagnostics.Debug.WriteLine($"[iOS] App is foreground: {isForeground}");

                    if (isForeground)
                    {
                        System.Diagnostics.Debug.WriteLine("[iOS] Sending ArrivedMessage to foreground app");
                        WeakReferenceMessenger.Default.Send(new ArrivedMessage("hq-300m"));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[iOS] Showing notification for background entry");
                        NotificationService_iOS.ShowNow(
                            "Arrived at Rick's Place",
                            "Please Check In",
                            "checkin?placeId=hq-300m"
                        );
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[iOS] Error processing region entry: {ex.Message}");
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
    }
}