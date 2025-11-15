using CommunityToolkit.Maui.Views;
using RHF_Foundation.ViewModels;

namespace RHF_Foundation.Views.PopUps;

public partial class CheckInPopup : Popup
{
    public CheckInPopup(int checkInCount)
    {
        InitializeComponent();

        var vm = new CheckInPopupViewModel(checkInCount);
        vm.RequestClose += async (_, __) => await this.CloseAsync(); // closes the popup
        BindingContext = vm;
    }
}