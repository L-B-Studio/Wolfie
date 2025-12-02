using Wolfie.Pages;
using Wolfie.Popups;

namespace Wolfie
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

            Routing.RegisterRoute(nameof(ChatListPage), typeof(ChatListPage));

            Routing.RegisterRoute(nameof(MessagePage), typeof(MessagePage));
        }
    }
}
