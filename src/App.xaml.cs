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
