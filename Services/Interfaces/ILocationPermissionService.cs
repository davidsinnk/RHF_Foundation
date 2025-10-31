namespace RHF_Foundation.Services.Interfaces;

public interface ILocationPermissionService
{
    Task<bool> CheckPermissionsAsync();
    Task<bool> RequestPermissionsAsync();
    Task<bool> RequestBackgroundLocationAsync();
    bool HasLocationPermission();
    bool HasBackgroundLocationPermission();
}