namespace RHF_Foundation.Views;


public partial class GetInvolved : ContentPage
{
    public GetInvolved(ViewModels.GetInvolvedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}