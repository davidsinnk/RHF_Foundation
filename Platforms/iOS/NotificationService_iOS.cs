using Foundation;
using UserNotifications;
using RHF_Foundation.Services;
using RHF_Foundation.Services.Interfaces;


namespace RHF_Foundation.Platforms.iOS;


public class NotificationService_iOS : INotificationService
{
public async Task<bool> RequestPermissionAsync()
{
var (granted, _) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge);
return granted;
}


public Task ShowAsync(string title, string body, string? route = null)
{
    ShowNow(title, body, route);
    return Task.CompletedTask;
}


internal static void ShowNow(string title, string body, string? route)
{
        var content = new UNMutableNotificationContent
        {
            Title = title,
            Body = body,
            Sound = UNNotificationSound.Default
        };
    
    if (!string.IsNullOrWhiteSpace(route))
    content.UserInfo = NSDictionary.FromObjectAndKey(new NSString(route), new NSString("route"));

    var request = UNNotificationRequest.FromIdentifier(Guid.NewGuid().ToString(), content, null);
    UNUserNotificationCenter.Current.AddNotificationRequest(request, null);
}
}


public class NotificationDelegate : UNUserNotificationCenterDelegate
{
public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
{
if (response.Notification.Request.Content.UserInfo["route"] is NSString route)
{
    MainThread.BeginInvokeOnMainThread(async () =>
    {
        await Shell.Current.GoToAsync(route.ToString());
    });
}
completionHandler();
}
}