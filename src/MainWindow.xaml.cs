using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("shutdown", "/t 0");
        }

        private void NoTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("shutdown", "/a");
            Hide();
        }

        private DispatcherTimer _timer;
        private TimeSpan _time;

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {           
            // if is visible
            if ((bool) e.NewValue)
            {
                _time = TimeSpan.FromSeconds(300);
                _timer?.Start();
            }
            else
            {
                YesButton.Content = "Ja (300)";
                _timer?.Stop();     
            }

            if (_timer == null)
            {
                _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, delegate
                {
                    YesButton.Content = $"Ja ({_time.TotalSeconds})";
                    if (_time == TimeSpan.Zero) _timer.Stop();
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                }, Application.Current.Dispatcher);               
            }
        }
    }
}
