using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using 全面瑕疵检测.Common.HIKCamera;
using 全面瑕疵检测.Models;
using 全面瑕疵检测.Services;
using 全面瑕疵检测.Views;

namespace 全面瑕疵检测.ViewModels
{
    internal partial class SystemConfigViewModel:ObservableObject
    {
        public SystemConfigViewModel()
        {
            //相机列表更新
            //CameraServiceV1.UpdateCameraListHandler += UpdateCameraListCallback;
            //相机状态更新
            //GIGE_Camera.UpdateOpenedStatusHandler += UpdateOpenCallback;

            
            
            //获取系统除去系统盘外所有磁盘列表
            DiskList = DiskModel.GetAllDrives();
        }

        public void InitPLC()
        {
            var plc = App.Current.Services.GetService<PLCService>();
            if (plc == null)
            {
                return;
            }
            var helper = plc.helper;
            if (helper == null)
                return;
            PLCConnect = plc.helper?.Connected ?? false;
            EmergencyStopSignal = plc.EmergencyStopSignal;
            WaitingWoodSignal = plc.WaitingWood;
            RetunZero = plc.ReturnningZero;
            helper.ConnectedChanged += PLCConnectChanged;
            plc.UpdateWaitingWoodSignalHandler += WaitingWoodSignalChanged;
            plc.UpdateEmergencyStopSignalHandler += EmergencyStopSignalChanged;
            plc.UpdateReturnningZeroHandler += RetunZeroChanged;
        }

        [ObservableProperty]
        private bool emergencyStopSignal;

        private void EmergencyStopSignalChanged(object? sender, bool e)
        {
            EmergencyStopSignal = e;
        }

        [ObservableProperty]
        private bool retunZero;

        private void RetunZeroChanged(object? sender, bool e)
        {
            RetunZero = e;
        }
        [ObservableProperty]
        private bool waitingWoodSignal;

        private void WaitingWoodSignalChanged(object? sender, bool e)
        {
            WaitingWoodSignal= e;
        }

        [ObservableProperty]
        private bool pLCConnect;

        private void PLCConnectChanged(object? sender, bool e)
        {
            PLCConnect = e;
        }

        ~SystemConfigViewModel()
        {
            //CameraService.UpdateCameraListHandler -= UpdateCameraListCallback;
            //GIGE_Camera.UpdateOpenedStatusHandler -= UpdateOpenCallback;
        }

        /// <summary>
        /// 更新相机是否连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateOpenCallback(object? sender, bool e)
        {
            if (sender != null)
            {
                var gIGE_Camera = (HIK_GIGE_Camera)sender;
                var camera = CameraList.FirstOrDefault((c) =>c.Name==gIGE_Camera.Name);
                if (camera != null)
                {
                    camera.Opened = gIGE_Camera.Connected;
                }
            }
           
        }
        /// <summary>
        /// 相机列表发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCameraListCallback(object? sender, EventArgs e)
        {
            LoadCamera();
        }

        [ObservableProperty]
        private ObservableCollection<DiskModel>? diskList;

        [ObservableProperty]
        private DiskModel? selectedDiskModel;

        [RelayCommand]
        private void UpdateDiskNameChanged()
        {
            if(SelectedDiskModel != null)
            {
                var _config = ConfigModel.Instance.configuration.GetSection("userconfig");
                if (_config != null)
                {
                    _config["disk_name"] = SelectedDiskModel.Name;
                    ConfigModel.Instance.Update();
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<CameraStatusModel> cameraList=new ObservableCollection<CameraStatusModel>();

        [RelayCommand]
        private void InitCamera()
        {
            
        }
        [RelayCommand]
        private void LoadCamera()
        {
            CameraList.Clear();
            foreach (var device in CameraServiceV1.HIK_GIGE_Cameras)
            {
                CameraList.Add(new CameraStatusModel() { Name = device.Name, Opened = device.Connected });
            }
        }

        [RelayCommand]
        private void Test()
        {
            var showImgView =new ImageView(null);
            showImgView.Show();
        }
    }
}
