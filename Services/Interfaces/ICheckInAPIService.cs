namespace RHF_Foundation.Services.Interfaces;


public interface ICheckInAPIService
{
Task<string> CheckIn(int numberAttending);
}