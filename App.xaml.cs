using RHF_Foundation.Platforms;

namespace RHF_Foundation;


public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        #if IOS
            // Forward notification taps to Shell navigation via delegate (set in NotificationService_iOS)
            UserNotifications.UNUserNotificationCenter.Current.Delegate = new Platforms.iOS.NotificationDelegate();
        #endif
    }
}