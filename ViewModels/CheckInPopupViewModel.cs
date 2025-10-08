using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace RHF_Foundation.ViewModels;


public partial class CheckInPopupViewModel : ObservableObject
{


    public ObservableCollection<Announcement> Announcements { get; } = new();
    public string UserName { get; set; } = "John Doe";

    public CheckInPopupViewModel()
    {
    }



}