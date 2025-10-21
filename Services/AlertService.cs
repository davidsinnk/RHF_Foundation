using RHF_Foundation.Services.Interfaces;


namespace RHF_Foundation.Services;


public class AlertService : IAlertService
{
    public Task ShowAsync(string title, string message, string accept = "OK")
    => MainThread.InvokeOnMainThreadAsync(() => Application.Current?.MainPage?.DisplayAlert(title, message, accept) ?? Task.CompletedTask);
}