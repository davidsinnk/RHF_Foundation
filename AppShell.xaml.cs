namespace RHF_Foundation;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("checkin", typeof(Views.CheckInPage));
	}
}
