using System.Runtime.Intrinsics.X86;

namespace RHF_Foundation.Views;


public partial class MainPage : ContentPage
{
	
	private ViewModels.MainViewModel _viewModel;
	
	public MainPage(ViewModels.MainViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_viewModel = vm;
	}

	    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadData(); // Call a method in your ViewModel to update values
    }

}
