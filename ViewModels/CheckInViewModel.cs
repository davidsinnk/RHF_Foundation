using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHF_Foundation.Services.Interfaces;
using RHF_Foundation.Services.APIs;


namespace RHF_Foundation.ViewModels;


public partial class CheckInViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAlertService _alerts;
    private readonly INavigationService _nav;

    private ICheckInAPIService _api;

    [ObservableProperty]
    private ObservableCollection<int> adultOptions;

    [ObservableProperty]
    private ObservableCollection<int> childOptions;

    [ObservableProperty]
    private int selectedAdults;

    [ObservableProperty]
    private int selectedChildren;


    [ObservableProperty]
    private string? placeId;


    public CheckInViewModel(IAlertService alerts, INavigationService nav)
    {
        _alerts = alerts;
        _nav = nav;
        _api = new CheckInAPIService();

        AdultOptions = new ObservableCollection<int>();
        ChildOptions = new ObservableCollection<int>();

        for (int i = 0; i <= 10; i++)
            {
                AdultOptions.Add(i);
                ChildOptions.Add(i);
            }
        SelectedAdults = 0;
        SelectedChildren = 0;
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("placeId", out var value) && value is string s)
            PlaceId = s;
    }

//Not used... legacy code for referance
    [RelayCommand]
    private async Task CheckInAsync()
    {
        await _alerts.ShowAsync("Checked In", $"Checked in at {PlaceId ?? "unknown"}.");
        //await _nav.GoToAsync("checkin");
    }


[RelayCommand]
private async Task ArriveAsync()
    {
        int total = SelectedAdults + SelectedChildren;
        if(total == 0)
            {
                await _alerts.ShowAsync("OH NO!!!", $"You must select at least one person to check in");
                return;
            }
        
        var response = await _api.CheckIn(total);
        if (response == "error")
            {
                await _alerts.ShowAsync("OH NO!!!", $"Something went wrong");

            }
        else
        {
            await _alerts.ShowAsync("Checked In", $"You are all set");
            await _nav.PopToRootAsync();
        }
        
        //await _nav.GoToAsync("home", new Dictionary<string, object> { ["placeId"] = placeId }); //legacy code for referance
    }
}