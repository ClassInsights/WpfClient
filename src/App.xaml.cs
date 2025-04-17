using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using WpfClient.Services;

namespace WpfClient
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var identity = WindowsIdentity.GetCurrent();
            if (identity.User != null && (identity.User.IsWellKnown(WellKnownSidType.LocalSystemSid) || identity.User.IsWellKnown(WellKnownSidType.AccountDomainAdminsSid)))
            {
                Current.Shutdown();
                return;
            }                      
            
            MainWindow = new MainWindow();

            _ = new System.Windows.Forms.NotifyIcon
            {
                Icon = WpfClient.Properties.Resources.Logo,
                Text = WpfClient.Properties.Resources.NotifyIconText,
                Visible = true
            };

            _ = Task.Run(async () =>
            {
                var pipeService = new PipeService();
                await pipeService.RunClient(); // endless task
            });
        }
    }
}
