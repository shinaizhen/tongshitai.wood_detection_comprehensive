using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Media3D;
using 全面瑕疵检测.Models;
using 全面瑕疵检测.Services;

namespace 全面瑕疵检测.ViewModels
{
    public partial class PLCConfigViewModel:ObservableObject
    {
        PLCService? plc;
        public PLCConfigViewModel() 
        {
            plc = App.Current.Services.GetService<PLCService>();
            if(plc != null)
            {
                EmergencyStopStatus = plc.EmergencyStopSignal;
                ReturnZeroStatus = plc.ReturnningZero;
                plc.UpdateEmergencyStopSignalHandler += EmergencyStopChanged;
                plc.UpdateReturnningZeroHandler += ReturnningZeroChanged;
            }
            var helper = plc?.helper;
            if (helper != null)
            {
                IP=helper.IP;
                Port=helper.Port;
                Connected = helper.Connected;
                helper.ConnectedChanged += ConnectedChanged;
            }
        }

        private void ReturnningZeroChanged(object? sender, bool e)
        {
            ReturnZeroStatus = e;
        }

        private void EmergencyStopChanged(object? sender, bool e)
        {
            EmergencyStopStatus = e;
        }

        private void ConnectedChanged(object? sender, bool e)
        {
            Connected = e;
        }

        [ObservableProperty]
        private string? iP;

        [ObservableProperty]
        private int port;

        [ObservableProperty]
        private bool connected;


        #region 滑台模块
        [ObservableProperty]
        private ushort slideLength;

        [ObservableProperty]
        private ushort slideSpeed;

        [ObservableProperty]
        private bool slidingDirec;

        [ObservableProperty]
        private bool leftSlideEnable;

        [ObservableProperty]
        private bool rightSlideEnable;

        [RelayCommand]
        private void Connect()
        {
            plc?.Connect();
        }

        [RelayCommand]
        private void DisConnect()
        {
            plc?.DisConnect();
        }

        [RelayCommand]
        private void SetSlide()
        {
            plc?.SlidingTableMotorMovement(SlideLength, direc: SlidingDirec,speed:SlideSpeed, rightEnable: RightSlideEnable, leftEnable: LeftFlipEnable);
        }
        #endregion

        #region 翻转电机模块
        [ObservableProperty]
        private ushort flipLength;

        [ObservableProperty]
        private ushort flipSpeed;

        [ObservableProperty]
        private bool flipDirec;

        [ObservableProperty]
        private bool leftFlipEnable;

        [ObservableProperty]
        private bool rightFlipEnable;

        [RelayCommand]
        private void SetFlip()
        {
            plc?.SetFlipMotor(FlipLength, FlipSpeed, FlipDirec, LeftFlipEnable, RightFlipEnable);
        }
        #endregion

        [ObservableProperty]
        private bool returnZeroStatus;
        [ObservableProperty]
        private bool waitingWoodStatus;
        [ObservableProperty]
        private bool emergencyStopStatus;


        [RelayCommand]
        private void ExtendCylinder(string i)
        {
            var id=int.Parse(i);
            plc?.ExtendCylinder(id);
        }

        [RelayCommand]
        private void RetractCylinder(string i)
        {
            var id = int.Parse(i);
            plc?.RetractCylinder(id);
        }
        [RelayCommand]
        private void ReturnToZero()
        {
            plc?.ReturnZero();
        }

        [RelayCommand]
        private void ManulMove()
        {
            if (GlobalStatus.ProgramExecutable==false)
            {
                MessageModel.ShowInfo("正在执行检测！");
                return;
            }
            GlobalStatus.UpdateProgramStatus(false);
            StartDetection();
            GlobalStatus.UpdateProgramStatus(true);

        }

        private void StartDetection()
        {
            #region 检查是否可以运行
            if (WoodInfoModel.GlobalWoodInfo == null)
            {
                MessageModel.ShowError("请选择木板型号！");
                return;
            }
            var wood = WoodInfoModel.GlobalWoodInfo;


            var _config = ConfigModel.Instance.configuration.GetSection("userconfig");
            if (_config == null)
            {
                MessageModel.ShowError("无法获取系统磁盘信息");
                return;
            }
            var diskName = _config["disk_name"];
            if (diskName == null)
            {
                MessageModel.ShowError("无法获取系统磁盘信息");
                return;
            }


            if (plc == null)
            {
                MessageModel.ShowError("plc服务器缺失！");
                return;
            }
            var helper = plc.helper;
            if (helper == null)
            {
                MessageModel.ShowError("plc helper为空值！");
                return;
            }
            if (helper.Connected == false)
            {
                MessageModel.ShowError("plc helper 无连接！");
                return;
            }
            if (plc.EmergencyStopSignal == true)
            {
                MessageModel.ShowError("机械处于急停状态！");
                return;
            }
            if (plc.ReturnningZero == true)
            {
                MessageModel.ShowError("机械正在回零！");
                return;
            }
            #endregion
            // 系统配置

            #region 第一次拍照
            bool bRet = plc.MoveToPositionOne(wood);
            if (bRet == false)
            {
                return;
            }
            
            #endregion

            #region 第二次拍照
            bRet = plc.MoveToPositionTwo(wood);
            if (bRet == false)
            {
                return;
            }
            #endregion
            #region 第三次拍照
            bRet = plc.MoveToPositionThree();
            if (bRet == false)
            {
                return;
            }
            #endregion

            #region 第四次拍照

            bRet = plc.MoveToPositionFour(wood);
            if (bRet == false)
            {
                return;
            }
            plc.ReturnZero();
            #endregion
        }
    }
}
