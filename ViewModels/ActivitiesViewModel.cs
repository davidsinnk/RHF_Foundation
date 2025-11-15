using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHF_Foundation.Services;

namespace RHF_Foundation.ViewModels;

public partial class ActivitesViewModel : ObservableObject
{

    // Replace with your actual URLs
    private const string EventsUrl = "https://rhfnow.org/events/";
    private const string ProgramsUrl = "https://rhfnow.org/programs/";

    [ObservableProperty] private bool isProgramsVisible = true;
    [ObservableProperty] private bool isEventsVisible = false;

    public ActivitesViewModel()
    {
    }

    [RelayCommand]
    private async Task OpenEvents() => await Launcher.OpenAsync(EventsUrl);

    [RelayCommand]
    private async Task OpenPrograms() => await Launcher.OpenAsync(ProgramsUrl);

        [RelayCommand]
    private void SelectEvents()
    {
        IsEventsVisible = true;
        IsProgramsVisible = false;
    }

    [RelayCommand]
    private void SelectPrograms()
    {
        IsEventsVisible = false;
        IsProgramsVisible = true;
    }

}