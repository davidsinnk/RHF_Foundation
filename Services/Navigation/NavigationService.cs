using RHF_Foundation.Services.Interfaces;

namespace RHF_Foundation.Services.Navigation;


public class NavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route, parameters));

    public Task PopToRootAsync()
        => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.Navigation.PopToRootAsync());
}