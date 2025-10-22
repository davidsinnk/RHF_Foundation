using RHF_Foundation.ViewModels;

namespace RHF_Foundation.Views;



public partial class CheckInPage : ContentPage
{
    public CheckInPage(ViewModels.CheckInViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}