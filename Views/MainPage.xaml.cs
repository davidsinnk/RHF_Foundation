namespace RHF_Foundation.Views;


public partial class MainPage : ContentPage
{
	public MainPage(ViewModels.MainViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

}
