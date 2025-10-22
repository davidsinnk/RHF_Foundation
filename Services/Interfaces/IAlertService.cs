namespace RHF_Foundation.Services.Interfaces;


public interface IAlertService
{
Task ShowAsync(string title, string message, string accept = "OK");
}