namespace RHF_Foundation.Services.Interfaces;


public interface ICheckInAPIService
{
    Task<string> CheckInNumberOfVisitors(int numberAttending);

    Task<string> CheckInArrival();
}