
#if ANDROID
using Android.App;
using Android.Content;
using Android.Gms.Location;

namespace RHF_Foundation.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
public class GeofenceBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null) return;

        var evt = GeofencingEvent.FromIntent(intent);
        if (evt == null || evt.HasError) return;

        if (evt.GeofenceTransition == Geofence.GeofenceTransitionEnter)
        {
            const string placeId = "hq-300m";
            const string channelId = "geofence_channel_id";

            // If app is in foreground, navigate immediately
            if (MainActivity.IsForeground)
            {
                var nav = MauiApplication.Current.Services.GetService<LocationService.Services.INavigationService>();
                _ = nav?.GoToCheckInAsync(placeId);
            }

            // Always post a heads-up notification (covers background case)
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var nm = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
                nm.CreateNotificationChannel(new NotificationChannel(channelId, "Geofence Alerts", NotificationImportance.High));
            }

            var launch = new Intent(context, typeof(MainActivity))
                .SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop)
                .PutExtra("deeplink", $"checkin?placeId={placeId}");

            var pending = PendingIntent.GetActivity(
                context, 3001, launch, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var builder = new Notification.Builder(context)
                .SetContentTitle("Arrived")
                .SetContentText("Tap to Check In")
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetAutoCancel(true)
                .SetContentIntent(pending);

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                builder.SetChannelId(channelId);

            var mgr = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
            mgr.Notify(Java.Lang.JavaSystem.CurrentTimeMillis().GetHashCode(), builder.Build());
        }
    }
}
#endif
