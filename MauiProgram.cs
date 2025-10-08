using CommunityToolkit.Mvvm;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;

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
/*  
// Registers the popup view + its VM for DI
        builder.Services.AddTransientPopup<
    RHF_Foundation.Views.Popups.CheckInPopup,
    RHF_Foundation.ViewModels.CheckInPopupViewModel>();

builder.Services.AddTransient<RHF_Foundation.ViewModels.MainViewModel>();
*/

#if DEBUG
        builder.Logging.AddDebug();
#endif
return builder.Build();
}
}
