using Wolfie.Pages;

namespace Wolfie
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }
    }
}
