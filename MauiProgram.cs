using CommunityToolkit.Mvvm;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using RHF_Foundation.Services.Interfaces;
using RHF_Foundation.Services;
using RHF_Foundation.Services.Navigation;
using RHF_Foundation.ViewModels;
using RHF_Foundation.Views;
using RHF_Foundation.Models;

namespace RHF_Foundation;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }); 

        // Services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<TrackingState>();

#if ANDROID
        builder.Services.AddSingleton<IGeofenceService, RHF_Foundation.Platforms.Android.GeofenceService_Android>();
        builder.Services.AddSingleton<INotificationService, RHF_Foundation.Platforms.Android.NotificationService_Android>();
        builder.Services.AddSingleton<ILocationPermissionService, RHF_Foundation.Platforms.Android.LocationPermissionService_Android>();
#elif IOS
        builder.Services.AddSingleton<IGeofenceService, RHF_Foundation.Platforms.iOS.GeofenceService_iOS>();
        builder.Services.AddSingleton<INotificationService, RHF_Foundation.Platforms.iOS.NotificationService_iOS>();
        builder.Services.AddSingleton<ILocationPermissionService, RHF_Foundation.Platforms.iOS.LocationPermissionService_iOS>();
#endif


        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();
        builder.Services.AddTransient<GetInvolvedViewModel>();
        builder.Services.AddTransient<ActivitesViewModel>(); // Note: typo in actual class name

        // Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CheckInPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}
