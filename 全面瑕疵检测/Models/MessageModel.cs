using System.Windows;
using System.Windows.Interop;
using 全面瑕疵检测.Services;
using 全面瑕疵检测.Views;

namespace 全面瑕疵检测.Models
{
    internal static class MessageModel
    {
        public static void ShowError(string error)
        {
            App app = App.Current;
            app.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(app.MainWindow, error, "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
                NlogService.Error(error);
            });
        }
        public static void ShowInfo(string info)
        {
            App app = App.Current;
            app.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(app.MainWindow,info , "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                NlogService.Error(info);
            });
        }
    }
}
