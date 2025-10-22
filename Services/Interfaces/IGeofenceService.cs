namespace RHF_Foundation.Services.Interfaces;


public interface IGeofenceService
{
Task<bool> StartMonitoringAsync(double latitude, double longitude, double radiusMeters);
Task StopMonitoringAsync();
}