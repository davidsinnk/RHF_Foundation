using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHF_Foundation.Services;

namespace RHF_Foundation.ViewModels;

public partial class GetInvolvedViewModel : ObservableObject
{

    // Replace with your actual URLs
    private const string WaiverUrl = "https://rhfnow.org/waiver/";
    private const string VolunteerUrl = "https://rhfnow.org/volunteer/";

    [ObservableProperty] private bool isWaiverVisible = true;
    [ObservableProperty] private bool isVolunteerVisible = false;

    public GetInvolvedViewModel()
    {
    }

    [RelayCommand]
    private async Task OpenWaiver() => await Launcher.OpenAsync(WaiverUrl);

    [RelayCommand]
    private async Task OpenVolunteer() => await Launcher.OpenAsync(VolunteerUrl);

        [RelayCommand]
    private void SelectWaiver()
    {
        IsWaiverVisible = true;
        IsVolunteerVisible = false;
    }

    [RelayCommand]
    private void SelectVolunteer()
    {
        IsWaiverVisible = false;
        IsVolunteerVisible = true;
    }

}