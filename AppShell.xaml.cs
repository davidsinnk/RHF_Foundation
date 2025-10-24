namespace RHF_Foundation;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("checkin", typeof(Views.CheckInPage));
		Routing.RegisterRoute("getinvolved", typeof(Views.GetInvolved));
		Routing.RegisterRoute("activites", typeof(Views.ActivitiesPage));
	}
}
