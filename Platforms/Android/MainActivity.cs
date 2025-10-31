using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Collections.Generic;
using RHF_Foundation.Services;
using Microsoft.Maui;
using RHF_Foundation.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace RHF_Foundation.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize
      | ConfigChanges.Orientation
      | ConfigChanges.UiMode
      | ConfigChanges.ScreenLayout
      | ConfigChanges.SmallestScreenSize
      | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //These are used to determine if app is in foreground or background
    public static bool IsForeground { get; set; }
  
    protected override void OnStart() { base.OnStart(); IsForeground = true; }
    protected override void OnStop()  { IsForeground = false; base.OnStop(); }
    
    
  

    const int RequestLocationPermissionsId = 42;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestAllRuntimePermissions();
        TryHandleDeepLink(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        TryHandleDeepLink(intent);
    }


    [SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
    void RequestAllRuntimePermissions()
    {
        if ((int)Build.VERSION.SdkInt < 23) return; // No runtime permissions needed before API 23

        var needed = new List<string>();

        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
            needed.Add(Manifest.Permission.AccessFineLocation);

        if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            needed.Add(Manifest.Permission.AccessCoarseLocation);

        if ((int)Build.VERSION.SdkInt >= 29 &&
            CheckSelfPermission(Manifest.Permission.AccessBackgroundLocation) != Permission.Granted)
            needed.Add(Manifest.Permission.AccessBackgroundLocation);

        if ((int)Build.VERSION.SdkInt >= 33 &&
            CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
            needed.Add(Manifest.Permission.PostNotifications);

        if (needed.Count > 0)
            RequestPermissions(needed.ToArray(), RequestLocationPermissionsId);
    }

    void TryHandleDeepLink(Intent? intent)
    {
        var route = intent?.GetStringExtra("deeplink");
        if (string.IsNullOrWhiteSpace(route)) return;

        // Delay navigation to ensure MAUI is fully initialized
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Wait a bit for the app to be ready
            await Task.Delay(500);
            
            try
            {
                var nav = IPlatformApplication.Current?.Services?.GetService<INavigationService>();
                if (nav == null) return;

                var placeId = ParsePlaceId(route);
                if (placeId != null)
                {
                    await nav.GoToAsync("checkin", new Dictionary<string, object>
                    {
                        { "placeId", placeId }
                    });
                }
                else
                {
                    // If no placeId, just navigate to checkin page
                    await nav.GoToAsync("checkin");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deep link navigation failed: {ex.Message}");
            }
        });
    }

    static string? ParsePlaceId(string route)
    {
        var idx = route.IndexOf("placeId=", System.StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var value = route[(idx + "placeId=".Length)..];
        var amp = value.IndexOf('&');
        return amp >= 0 ? value[..amp] : value;
    }
}

