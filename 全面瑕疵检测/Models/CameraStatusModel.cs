using CommunityToolkit.Mvvm.ComponentModel;

namespace 全面瑕疵检测.Models
{
    // 相机状态模块
    internal partial class CameraStatusModel:ObservableObject
    {
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private bool opened;
    }
}
