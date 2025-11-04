#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using System.Threading.Tasks;
using RHF_Foundation.Services.Interfaces;


namespace RHF_Foundation.Platforms.Android;


public class NotificationService_Android : Java.Lang.Object, INotificationService
{
    const string ChannelId = "geofence_channel_id";
    bool _channelReady;

    void EnsureChannel()
    {
        if (_channelReady) return;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, "Geofence Alerts", NotificationImportance.High);
            var manager = (NotificationManager?)global::Android.App.Application.Context
                .GetSystemService(Context.NotificationService);
            manager?.CreateNotificationChannel(channel);
        }

        _channelReady = true;
    }

    public Task<bool> RequestPermissionAsync() => Task.FromResult(true);

    public Task ShowAsync(string title, string body, string? route = null)
    {
        EnsureChannel();

        // ✅ Fully qualify Application to use Android.App.Application
        var intent = new Intent(global::Android.App.Application.Context, typeof(MainActivity));
        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
        if (!string.IsNullOrWhiteSpace(route))
            intent.PutExtra("deeplink", route);

        var pendingFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable;
        var pending = PendingIntent.GetActivity(global::Android.App.Application.Context, 1001, intent, pendingFlags);

        var builder = new Notification.Builder(global::Android.App.Application.Context)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo) // ✅ add global::
            .SetAutoCancel(true)
            .SetContentIntent(pending);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            builder.SetChannelId(ChannelId);

        var manager = (NotificationManager?)global::Android.App.Application.Context
            .GetSystemService(Context.NotificationService);
        manager?.Notify(Java.Lang.JavaSystem.CurrentTimeMillis().GetHashCode(), builder.Build());

        return Task.CompletedTask;
    }
}
#endif
