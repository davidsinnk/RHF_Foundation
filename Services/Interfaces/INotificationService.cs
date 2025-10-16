namespace RHF_Foundation.Services.Interfaces;


public interface INotificationService
{
Task<bool> RequestPermissionAsync();
Task ShowAsync(string title, string body, string? route = null);
}