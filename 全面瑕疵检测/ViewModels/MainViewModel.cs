using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using 全面瑕疵检测.Models;
using 全面瑕疵检测.Services;
using 全面瑕疵检测.Views;
using Timer = System.Timers.Timer;

namespace 全面瑕疵检测.ViewModels
{
    internal partial class MainViewModel:ObservableObject
    {
        Timer _timer;
        public MainViewModel() 
        {
            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Elapsed += LinstenTime;
            _timer.Start();
            GetConfig(this,EventArgs.Empty);
            ConfigModel.UpdateSettingHandler += GetConfig;
            Content = (UIElement)App.Current.Services.GetService<DetectionView>();
            //saveDiskName = _config["disk_name"];
        }

        private void GetConfig(object? sender, EventArgs e)
        {
            var _config = ConfigModel.Instance.configuration.GetSection("userconfig");
            SaveDiskName = _config.GetValue<string>("disk_name");
            RunPythonModel.LoadBatPath();
        }

        ~MainViewModel()
        {
            ConfigModel.UpdateSettingHandler -= GetConfig;
        }

        private void LinstenTime(object? sender, System.Timers.ElapsedEventArgs e)
        {
            TimeString = DateTime.Now.ToString();
        }

        [ObservableProperty]
        private string? timeString;

        [ObservableProperty]
        private string? saveDiskName;

        [ObservableProperty]
        private UIElement content;

        [RelayCommand]
        private void CloseMainView(object obj)
        {
            MainView mainView = (MainView)obj;
            mainView.Close();
        }

        [RelayCommand]
        private void MiniMainView(object obj)
        {
            MainView mainView = (MainView)obj;
            mainView.WindowState=System.Windows.WindowState.Minimized;
        }

        [RelayCommand]
        private void ChangeView(Type type)
        {
            if (type != null)
            {
                Content = (UIElement)App.Current.Services.GetRequiredService(type);
            }
        }

        [RelayCommand]
        private void OpenCameraCofingView()
        {
            //CameraConfigView view = new CameraConfigView();
            //view.Show();
        }
    }
}
