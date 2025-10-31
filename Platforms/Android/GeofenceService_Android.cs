#if ANDROID
using System;
using System.Threading.Tasks;
using RHF_Foundation.Services;
using Android.Locations;
using Android.OS;

using RHF_Foundation.Models;

using CommunityToolkit.Mvvm.Messaging;
using RHF_Foundation.Services.Messaging;
using RHF_Foundation.Services.Interfaces;
using RHF_Foundation.Platforms.Android;
using RHF_Foundation.Messaging;


namespace RHF_Foundation.Platforms.Android;

public sealed class GeofenceService_Android : Java.Lang.Object, IGeofenceService
{
    private readonly LocationManager? _locationManager;
    private readonly TrackingState _tracking;
    private readonly INotificationService _notifier;
    private readonly INavigationService _nav;

    private readonly AndroidLocationListener _listener;

    private double _fenceLat;
    private double _fenceLon;
    private double _fenceRadiusMeters;
    private string _placeId = "hq-300m";

    private bool _active;
    private bool _wasInside;

    public GeofenceService_Android(
        TrackingState trackingState,
        INotificationService notificationService,
        INavigationService navigationService)
    {
        _tracking  = trackingState;
        _notifier  = notificationService;
        _nav       = navigationService;

        _locationManager = (LocationManager?)global::Android.App.Application.Context
            .GetSystemService(global::Android.Content.Context.LocationService);

        _listener = new AndroidLocationListener(HandleNewLocation);
    }

    public async Task<bool> StartMonitoringAsync(double latitude, double longitude, double radiusMeters)
    {
        _fenceLat = latitude;
        _fenceLon = longitude;
        _fenceRadiusMeters = radiusMeters;
        _active = true;

        try
        {
#pragma warning disable CA1422
            // Fine (GPS)
            _locationManager?.RequestLocationUpdates(
                LocationManager.GpsProvider,
                2000L,
                1f,
                _listener);

            // Coarse (Network) — helps indoors/emulators
            _locationManager?.RequestLocationUpdates(
                LocationManager.NetworkProvider,
                2000L,
                1f,
                _listener);
#pragma warning restore CA1422
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ANDROID][Geofence] RequestLocationUpdates failed: {ex}");
            return false;
        }

        _ = _notifier.RequestPermissionAsync();
        return true;
    }

    public Task StopMonitoringAsync()
    {
        _active = false;
        try { _locationManager?.RemoveUpdates(_listener); } catch { /* ignore */ }
        return Task.CompletedTask;
    }

    private void HandleNewLocation(global::Android.Locations.Location loc)
    {
        if (!_active || loc is null) return;

        _tracking.CurrentLatitude  = loc.Latitude;
        _tracking.CurrentLongitude = loc.Longitude;

        var distMeters = HaversineMeters(loc.Latitude, loc.Longitude, _fenceLat, _fenceLon);
        var inside = distMeters <= _fenceRadiusMeters;
        _tracking.IsInsideFence = inside;

        // Edge: outside -> inside
        if (inside && !_wasInside)
        {
            // Always notify (covers background)
            _ = _notifier.ShowAsync("Arrived", "Tap to Check In", route: $"checkin?placeId={_placeId}");

            // Foreground? Navigate immediately
            if (MainActivity.IsForeground)
            {
                _ = _nav.GoToAsync(_placeId, new Dictionary<string, object>
                {
                    { "placeId", _placeId }
                } );
            }
        }

        _wasInside = inside;
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6_371_000.0; // meters
        double dLat = DegToRad(lat2 - lat1);
        double dLon = DegToRad(lon2 - lon1);

        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;

    // Nested listener to keep Android callbacks out of the public surface
    private sealed class AndroidLocationListener : Java.Lang.Object, ILocationListener
    {
        private readonly Action<global::Android.Locations.Location> _push;

        public AndroidLocationListener(Action<global::Android.Locations.Location> onLocation)
            => _push = onLocation;

        [Obsolete("Required by current Android bindings.")]
        public void OnLocationChanged(global::Android.Locations.Location location)
        {
            System.Diagnostics.Debug.WriteLine($"[ANDROID][Geofence] OnLocationChanged: {location.Latitude}, {location.Longitude}");
            _push(location);
        }

        [Obsolete] public void OnStatusChanged(string provider, [global::Android.Runtime.GeneratedEnum] Availability status, Bundle extras) { }
        public void OnProviderEnabled(string provider) { }
        public void OnProviderDisabled(string provider) { }
    }
}
#endif
