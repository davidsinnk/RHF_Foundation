#if ANDROID
using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using RHF_Foundation.Services.Interfaces;

namespace RHF_Foundation.Platforms.Android;

public class LocationPermissionService_Android : ILocationPermissionService
{
    private readonly Context _context;

    public LocationPermissionService_Android()
    {
        _context = global::Android.App.Application.Context ?? throw new InvalidOperationException("Android Application.Context is null");
    }

    public Task<bool> CheckPermissionsAsync()
    {
        var fineLocation = ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessFineLocation) == Permission.Granted;
        var coarseLocation = ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessCoarseLocation) == Permission.Granted;
        
        return Task.FromResult(fineLocation && coarseLocation);
    }

    public async Task<bool> RequestPermissionsAsync()
    {
        if (await CheckPermissionsAsync())
            return true;

        var tcs = new TaskCompletionSource<bool>();
        
        // This would typically be handled by your MainActivity
        // For now, return false if permissions aren't already granted
        return false;
    }

    public async Task<bool> RequestBackgroundLocationAsync()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            return true; // Not needed on older versions

        if (HasBackgroundLocationPermission())
            return true;

        // Background location requires special handling
        // Usually involves showing a system dialog
        return false;
    }

    public bool HasLocationPermission()
    {
        var fineLocation = ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessFineLocation) == Permission.Granted;
        var coarseLocation = ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessCoarseLocation) == Permission.Granted;
        
        return fineLocation && coarseLocation;
    }

    public bool HasBackgroundLocationPermission()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            return true;

        return ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessBackgroundLocation) == Permission.Granted;
    }
}
#endif