using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace 全面瑕疵检测.Views
{
    /// <summary>
    /// LoadingView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingView : Window
    {
        DispatcherTimer timer;
        public LoadingView()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += ChangBackground;
            timer.Start();
            InitializeComponent();
        }
        int indx = 0;
        private void ChangBackground(object? sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (indx % 2 == 0)
                {
                    this.back.Background = Brushes.Red;
                }
                else
                {
                    this.back.Background = Brushes.Green;
                }
                indx++;
            }));
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        public void ShowMsg(string msg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.txtblock_Loading.Text = msg;
            }));
        }
    }
}
