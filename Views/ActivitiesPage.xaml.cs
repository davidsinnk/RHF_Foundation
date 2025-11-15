
namespace RHF_Foundation.Views;


public partial class ActivitiesPage : ContentPage
{
public ActivitiesPage(ViewModels.ActivitesViewModel vm)
{
    InitializeComponent();
    BindingContext = vm;
}
}