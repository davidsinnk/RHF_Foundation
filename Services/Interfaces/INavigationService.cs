namespace RHF_Foundation.Services.Interfaces;


public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    public Task PopToRootAsync();
}