using MvCamCtrl.NET;
using OpenCvSharp;
namespace 全面瑕疵检测.Common.HIKCamera
{
    internal interface ICameraService
    {
        int GetCameraList();// 获取相机列表
        int OpenAllCamera();// 打开所有相机
        void CloseAll();// 关闭所有相机
        int ConfigCamera(HIK_GIGE_Camera gIGE_Camera);// 设置相机相关参数
        int LoadCameraConfigs(string filePath);
        int SaveCameraConfigs(string filePath);
        bool InitCameras();// 初始化相机
        int RegisterDisconnectedCallback(HIK_GIGE_Camera gIGE_Camera);
        SortedDictionary<string, Mat> GetSXKImages();
        SortedDictionary<string, Mat> GetCXKImages();
    }
}
