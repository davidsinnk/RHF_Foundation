using CoreLocation;
using Foundation;
using RHF_Foundation.Services.Interfaces;

namespace RHF_Foundation.Platforms.iOS;

public class LocationPermissionService_iOS : NSObject, ILocationPermissionService
{
    private CLLocationManager? _locationManager;
    private TaskCompletionSource<CLAuthorizationStatus>? _permissionTcs;

    private CLAuthorizationStatus GetCurrentStatus()
    {
        #pragma warning disable CA1422
        return CLLocationManager.Status;
        #pragma warning restore CA1422
    }

    public async Task<bool> CheckPermissionsAsync()
    {
        var status = GetCurrentStatus();
        return status == CLAuthorizationStatus.AuthorizedAlways || 
               status == CLAuthorizationStatus.AuthorizedWhenInUse;
    }

    public async Task<bool> RequestPermissionsAsync()
    {
        var currentStatus = GetCurrentStatus();
        
        System.Diagnostics.Debug.WriteLine($"[iOS] Current location permission status: {currentStatus}");
        
        // If already granted, return true
        if (currentStatus == CLAuthorizationStatus.AuthorizedAlways || 
            currentStatus == CLAuthorizationStatus.AuthorizedWhenInUse)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Location permission already granted");
            return true;
        }
        
        // If denied, can't request again - user needs to go to Settings
        if (currentStatus == CLAuthorizationStatus.Denied || 
            currentStatus == CLAuthorizationStatus.Restricted)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Location permission denied - user needs to enable in Settings");
            return false;
        }
        
        // Request permission if not determined
        if (currentStatus == CLAuthorizationStatus.NotDetermined)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Requesting when-in-use location permission");
            return await RequestPermissionInternal();
        }
        
        return false;
    }

    public async Task<bool> RequestBackgroundLocationAsync()
    {
        var currentStatus = GetCurrentStatus();
        
        System.Diagnostics.Debug.WriteLine($"[iOS] Current status for background location: {currentStatus}");
        
        // Check if we already have Always permission
        if (currentStatus == CLAuthorizationStatus.AuthorizedAlways)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Background location permission already granted");
            return true;
        }
        
        // If denied, can't request again - user needs to go to Settings
        if (currentStatus == CLAuthorizationStatus.Denied || 
            currentStatus == CLAuthorizationStatus.Restricted)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Background location permission denied - user needs to enable in Settings");
            return false;
        }
        
        // First need WhenInUse, then can request Always
        if (currentStatus == CLAuthorizationStatus.NotDetermined)
        {
            System.Diagnostics.Debug.WriteLine("[iOS] Getting when-in-use permission first");
            var whenInUseGranted = await RequestPermissionInternal();
            if (!whenInUseGranted)
                return false;
        }
        
        // Now request Always permission
        System.Diagnostics.Debug.WriteLine("[iOS] Requesting always location permission");
        return await RequestAlwaysPermissionInternal();
    }

    private async Task<bool> RequestPermissionInternal()
    {
        var tcs = new TaskCompletionSource<bool>();
        
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _locationManager = new CLLocationManager();
            _locationManager.Delegate = new LocationManagerDelegate(this);
            
            _permissionTcs = new TaskCompletionSource<CLAuthorizationStatus>();
            
            System.Diagnostics.Debug.WriteLine("[iOS] Requesting when-in-use location authorization");
            // Request when-in-use authorization first
            _locationManager.RequestWhenInUseAuthorization();
        });
        
        // Wait for the result
        var result = await _permissionTcs!.Task;
        
        _locationManager?.Dispose();
        _locationManager = null;
        _permissionTcs = null;
        
        var success = result == CLAuthorizationStatus.AuthorizedWhenInUse || 
               result == CLAuthorizationStatus.AuthorizedAlways;
               
        System.Diagnostics.Debug.WriteLine($"[iOS] When-in-use permission result: {result}, Success: {success}");
        return success;
    }
    
    private async Task<bool> RequestAlwaysPermissionInternal()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _locationManager = new CLLocationManager();
            _locationManager.Delegate = new LocationManagerDelegate(this);
            
            _permissionTcs = new TaskCompletionSource<CLAuthorizationStatus>();
            
            System.Diagnostics.Debug.WriteLine("[iOS] Requesting always location authorization");
            // Request always authorization
            _locationManager.RequestAlwaysAuthorization();
        });
        
        // Wait for the result
        var result = await _permissionTcs!.Task;
        
        _locationManager?.Dispose();
        _locationManager = null;
        _permissionTcs = null;
        
        var success = result == CLAuthorizationStatus.AuthorizedAlways;
        System.Diagnostics.Debug.WriteLine($"[iOS] Always permission result: {result}, Success: {success}");
        return success;
    }

    internal void OnAuthorizationChanged(CLAuthorizationStatus status)
    {
        _permissionTcs?.SetResult(status);
    }

    public bool HasLocationPermission()
    {
        var status = GetCurrentStatus();
        return status == CLAuthorizationStatus.AuthorizedAlways || 
               status == CLAuthorizationStatus.AuthorizedWhenInUse;
    }

    public bool HasBackgroundLocationPermission()
    {
        return GetCurrentStatus() == CLAuthorizationStatus.AuthorizedAlways;
    }

    private class LocationManagerDelegate : CLLocationManagerDelegate
    {
        private readonly LocationPermissionService_iOS _service;

        public LocationManagerDelegate(LocationPermissionService_iOS service)
        {
            _service = service;
        }

        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            _service.OnAuthorizationChanged(status);
        }
    }
}