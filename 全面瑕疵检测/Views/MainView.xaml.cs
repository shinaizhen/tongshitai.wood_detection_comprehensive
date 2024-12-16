using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Interop;
using 全面瑕疵检测.Models;
using 全面瑕疵检测.Services;

namespace 全面瑕疵检测.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取工作区的大小
            var workArea = SystemParameters.WorkArea;

            // 设置窗口大小和位置
            this.Left = workArea.Left;
            this.Top = workArea.Top;
            this.Width = workArea.Width;
            this.Height = workArea.Height;

            // 使窗口最大化
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize;
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadingView loadingView = new LoadingView();
            loadingView.ShowMsg("正在关闭系统");
            loadingView.Show();
            Task.Run(() =>
            {

                Thread.Sleep(1000);
                App app = App.Current;
                var plc = app.Services.GetService<PLCService>();
                string msg;
                if(plc != null)
                {
                    msg = "正在断开plc连接......";
                    loadingView.ShowMsg(msg);
                    NlogService.Info($"系统初始化，{msg}");
                    Thread.Sleep(1000);
                    plc.helper?.Dispose();
                    plc.UpdateEmergencyStopSignalHandler = null;
                    plc.UpdatePointHandler = null;
                    plc.UpdateReturnningZeroHandler = null;
                    plc.UpdateWaitingWoodSignalHandler = null;
                }
                if (RunPythonModel.Running)
                {
                    msg = "正在终止检测后台.....";
                    loadingView.ShowMsg(msg);
                    NlogService.Info($"系统初始化，{msg}");
                    Thread.Sleep(1000);
                    RunPythonModel.StopPython();
                }
                var camera = app.Services.GetService<CameraServiceV1>();
                if(camera != null)
                {
                    msg = "正在关闭所有相机.....";
                    loadingView.ShowMsg(msg);
                    NlogService.Info($"系统初始化，{msg}");
                    Thread.Sleep(1000);
                    camera.CloseAll();
                }
                
                
                app.Dispatcher.Invoke(new Action(() =>
                {
                    loadingView.Close();
                }));
            });
        }
    }
}
