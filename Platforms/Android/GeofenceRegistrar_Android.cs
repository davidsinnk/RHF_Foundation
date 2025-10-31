#if ANDROID
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.OS;

namespace RHF_Foundation.Platforms.Android;

public static class GeofenceRegistrar_Android
{
    public static async Task RegisterAsync(Context ctx, double lat, double lon, float radiusMeters, string requestId)
    {
        try
        {
            // Check Google Play Services availability
            var playServicesAvailable = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(ctx);
            System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Google Play Services status: {playServicesAvailable}");
            
            if (playServicesAvailable != ConnectionResult.Success)
            {
                System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Google Play Services not available: {playServicesAvailable}");
                return;
            }

            var client = LocationServices.GetGeofencingClient(ctx);

            var geofence = new GeofenceBuilder()
                .SetRequestId(requestId)
                .SetCircularRegion(lat, lon, radiusMeters)
                .SetExpirationDuration(Geofence.NeverExpire)
                .SetTransitionTypes(Geofence.GeofenceTransitionEnter)
                .Build();

            var request = new GeofencingRequest.Builder()
                .SetInitialTrigger(GeofencingRequest.InitialTriggerEnter) // fire immediately if already inside
                .AddGeofence(geofence)
                .Build();

            var intent = new Intent(ctx, typeof(GeofenceBroadcastReceiver));
            
            // Use API level appropriate flags
            var flags = PendingIntentFlags.UpdateCurrent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                flags |= PendingIntentFlags.Immutable;
#pragma warning restore CA1416 // Validate platform compatibility
            }
            
            var pending = PendingIntent.GetBroadcast(ctx, 2001, intent, flags);

            System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Registering geofence: {lat}, {lon}, radius {radiusMeters}m");
            await client.AddGeofencesAsync(request, pending);
            System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Geofence registration successful");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Registration failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Exception type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Stack trace: {ex.StackTrace}");
            
            // Check for specific Google Play Services errors
            if (ex.Message.Contains("13"))
            {
                System.Diagnostics.Debug.WriteLine($"[ANDROID][GeofenceRegistrar] Error 13: Likely insufficient permissions or Google Play Services unavailable");
            }
        }
    }

    public static Task UnregisterAsync(Context ctx)
    {
        var intent = new Intent(ctx, typeof(GeofenceBroadcastReceiver));
        
        // Use API level appropriate flags
        var flags = PendingIntentFlags.UpdateCurrent;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            flags |= PendingIntentFlags.Immutable;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        
        var pending = PendingIntent.GetBroadcast(ctx, 2001, intent, flags);

        var client = LocationServices.GetGeofencingClient(ctx);
        return client.RemoveGeofencesAsync(pending);
    }
}
#endif
