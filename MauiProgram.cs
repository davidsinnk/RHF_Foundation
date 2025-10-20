using CommunityToolkit.Mvvm;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using RHF_Foundation.Services.Interfaces;
using RHF_Foundation.Services;
using RHF_Foundation.Services.Navigation;
using RHF_Foundation.ViewModels;
using RHF_Foundation.Views;

namespace RHF_Foundation;


public static class MauiProgram
{
public static MauiApp CreateMauiApp()
{
var builder = MauiApp.CreateBuilder();
        builder
        .UseMauiApp<App>()
        .UseMauiCommunityToolkit()  // <-- add this line
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });


        builder.UseMauiApp<App>()
                .UseMauiCommunityToolkit(); 

        // Services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
                        builder.Services.AddSingleton<IAlertService, AlertService>();

        #if ANDROID
        builder.Services.AddSingleton<IGeofenceService, Platforms.Android.GeofenceService_Android>();
        builder.Services.AddSingleton<INotificationService, Platforms.Android.NotificationService_Android>();
        #elif IOS
        builder.Services.AddSingleton<IGeofenceService, Platforms.iOS.GeofenceService_iOS>();
        builder.Services.AddSingleton<INotificationService, Platforms.iOS.NotificationService_iOS>();
        #endif


        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();


        // Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CheckInPage>();

        #if DEBUG
                        builder.Logging.AddDebug();
        #endif
        return builder.Build();
}
}
