using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RHF_Foundation.ViewModels;

public partial class CheckInPopupViewModel : ObservableObject
{
    public int CheckInCount { get; }

    [ObservableProperty]
    string displayImage;

    [ObservableProperty]
    string titleMessage = "Congratulation";
    
    [ObservableProperty]
    string displayMessage = "You've reach a new visit milestone!";

    [ObservableProperty]
    string staffMessage = "Please see a staff member for a special reward!";


    // Fire this when the VM wants the popup to close
    public event EventHandler? RequestClose;

    public ICommand GotItCommand { get; }

    public CheckInPopupViewModel(int checkInCount)
    {
        CheckInCount = checkInCount;
        GotItCommand = new RelayCommand(OnGotIt);
        SetUpDisplayImage();
    }

    private void OnGotIt()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void SetUpDisplayImage()
    {
        switch (CheckInCount)
        {
            case 5:
                DisplayImage = "five.png";
                break;
            case 10:
                DisplayImage = "ten.png";
                break;
            case 25:
                DisplayImage = "twentyfive.png";
                break;
            case 50:
                DisplayImage = "fifty.png";
                break;
            default:
                DisplayImage = "one.png";
                TitleMessage = "Welcome!";
                DisplayMessage = "Thank you for being part of our community!";
                StaffMessage = "Enjoy your visit!";
                break;
        }
    }
}
