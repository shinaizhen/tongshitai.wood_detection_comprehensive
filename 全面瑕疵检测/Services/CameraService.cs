//using HIKI_Camera;
//using MvCamCtrl.NET;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;
//using System.IO;
//using 全面瑕疵检测.Common.HIKCamera;
//using Image = SixLabors.ImageSharp.Image;

//namespace 全面瑕疵检测.Services
//{
//    internal class CameraService
//    {
//        public bool RunStatus { get; private set; } = false;
//        / <summary>
//        / 相机列表更新
//        / </summary>
//        public static EventHandler? UpdateCameraListHandler;
//        static Dictionary<string, HIK_GIGE_Camera> mapOfCamera = new Dictionary<string, HIK_GIGE_Camera>();
//        public static Dictionary<string, HIK_GIGE_Camera> MapOfCamera
//        {
//            get { return mapOfCamera; }
//            set
//            {
//                mapOfCamera = value;
//                UpdateCameraListHandler?.Invoke(null, EventArgs.Empty);
//            }
//        }
//        存储需要打卡的相机名称
//        public static readonly List<string> cameraNames = new()
//        {
//            "SXK1","SXK2","SXK3","SXK4","SXK5","SXK6","SXK7","SXK8",
//            "CXK1","CXK2","CXK3","CXK4","CXK5","CXK6"
//        };
//        MyCamera.cbExceptiondelegate cbExceptiondelegate;
//        public CameraService()
//        {
//            cbExceptiondelegate = new MyCamera.cbExceptiondelegate(DisConnectBackFunc);
//            GC.KeepAlive(cbExceptiondelegate);
//        }

//        public HIK_GIGE_Camera? GetDeviceByID(int id)
//        {
//            return MapOfCamera.FirstOrDefault((i) => i.Value.ID == id).Value;
//        }
//        / <summary>
//        / 断线重连
//        / </summary>
//        / <param name = "nMsgType" ></ param >
//        / < param name="pUser"></param>
//        private void DisConnectBackFunc(uint nMsgType, IntPtr pUser)
//        {
//            if (nMsgType == MyCamera.MV_EXCEPTION_DEV_DISCONNECT)
//            {
//                RunStatus = false;
//                var device = GetDeviceByID((int)pUser);
//                if (device != null)
//                {
//                    PrintMsg($"设备{device.Name}：设备断线重连！");
//                    while (true)// 相机断开
//                    {
//                        HIK_Camera_Operator.CloseDevice(device);
//                        int nRet = HIK_Camera_Operator.OpenDevice(device);
//                        if (nRet == 0)
//                            break;
//                    }
//                    RunStatus = true;
//                    PrintMsg($"设备{device.Name}：设备断线重连成功！");
//                }
//            }
//        }

//        public bool EnumDevice()
//        {
//            MapOfCamera.Clear();
//            var devices = HIK_Camera_Operator.EnumDevice();
//            if (devices.Count > 0)
//            {
//                int sum = 0;
//                foreach (var device in devices)
//                {
//                    if (cameraNames.Contains(device.Name))
//                    {
//                        MapOfCamera.Add(device.Name, device);
//                        sum++;
//                    }
//                }
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public bool Init()
//        {
//            int nRet = 0;
//            if (!EnumDevice())//枚举设备列表
//                return false;
//            foreach (var name in cameraNames)
//            {
//                if (MapOfCamera.ContainsKey(name))
//                {
//                    var device = MapOfCamera[name];
//                    nRet = HIK_Camera_Operator.OpenDevice(device);
//                    if (nRet != 0)
//                    {
//                        return false;
//                    }
//                    string configPath = $"{device.Name}ConfigFile";
//                    if (File.Exists(configPath))//判断相机配置文件是否存在
//                    {
//                        nRet = HIK_Camera_Operator.LoadParamFromFiler(device, configPath);
//                        if (nRet != 0)
//                        {
//                            return false;
//                        }
//                    }
//                    nRet = HIK_Camera_Operator.SetCommonParam(device);//设置相机共有参数
//                    if (nRet != 0)
//                    {
//                        return false;
//                    }
//                    nRet = HIK_Camera_Operator.SetExposureTime(device, 800000);//设置相机共有参数
//                    if (nRet != 0)
//                    {
//                        return false;
//                    }
//                    nRet = HIK_Camera_Operator.GetParam(device);//获取相机参数
//                    if (nRet != 0)
//                    {
//                        return false;
//                    }
//                    注册断线重连函数
//                    nRet = HIK_Camera_Operator.RegisterDisconnectedCallbackFunc(device, cbExceptiondelegate);
//                    if (nRet != 0)
//                    {
//                        PrintMsg("注册回调函数失败！");
//                        return false;
//                    }
//                    打开软触发
//                   nRet = HIK_Camera_Operator.SetTriggerMode(device, triggerMode: true, softTrigger: true);
//                    if (nRet != 0)
//                    {
//                        PrintMsg("设置软触发模式失败！");
//                        return false;
//                    }
//                    开始采集
//                   nRet = HIK_Camera_Operator.StartGrabbing(device);
//                    if (nRet != 0)
//                    {
//                        PrintMsg("开启采集失败！");
//                        return false;
//                    }
//                }
//                else
//                {
//                    PrintMsg($"相机初始化失败：枚举不到设备{name}");
//                    return false;
//                }
//            }
//            RunStatus = true;// 设备运行正常
//            return true;
//        }

//        public void DeInit()
//        {
//            if (MapOfCamera.Count > 0)
//            {
//                foreach (var keyValue in MapOfCamera)
//                {
//                    var value = keyValue.Value;
//                    HIK_Camera_Operator.StopGrabbing(value);
//                    HIK_Camera_Operator.CloseDevice(value);
//                }
//                MapOfCamera.Clear();
//            }
//            RunStatus = false;
//        }

//        void PrintMsg(string msg)
//        {
//            NlogService.Debug(msg);
//        }

//        #region 服务状态检测
//        / <summary>
//        / 检测全部相机是否可用
//        / </summary>
//        / <returns></returns>
//        public bool CheckAllCameraOpened()
//        {
//            检测所有相机是否全部获取
//            foreach (var item in mapOfCamera)
//            {
//                if (item.Value.Opened == false)
//                    return false;
//            }
//            return true;
//        }

//        / <summary>
//        / 检测所有相机是否全部获取
//        / </summary>
//        / <returns></returns>
//        public bool IsContainAllCamera()
//        {
//            检测所有相机是否全部获取
//            foreach (string name in cameraNames)
//            {
//                if (!mapOfCamera.ContainsKey(name))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        #endregion

//        #region 相机控制取图
//        public SortedDictionary<string, Image>? GrabSXKImageV2()
//        {
//            try
//            {
//                int nRet;
//                SortedDictionary<string, Image> images = new SortedDictionary<string, Image>();
//                foreach (var item in MapOfCamera)
//                {
//                    if (item.Value.Name.Contains("SXK"))
//                    {
//                        nRet = HIK_Camera_Operator.StartTriggerOnce(item.Value);
//                        if (nRet == 0)//触发成功
//                        {
//                            Thread.Sleep(500);
//                            var imgdata = HIK_Camera_Operator.ReceiveThreadProcess(item.Value);
//                            if (imgdata != null)
//                            {
//                                Image image = Image.LoadPixelData<L8>(imgdata.Data, imgdata.Width, imgdata.Height);
//                                image.Mutate((mtx) => mtx.Rotate(270));
//                                images.Add(imgdata.CameraName, image);
//                            }

//                        }
//                    }
//                }
//                return images;
//            }
//            catch (Exception ex)
//            {
//                NlogService.Error($"采集图象时发生错误：{ex.Message}");
//                return null;
//            }
//        }

//        public SortedDictionary<string, Image> GrabCXKImageV2()
//        {
//            int nRet;
//            SortedDictionary<string, Image> images = new SortedDictionary<string, Image>();
//            foreach (var item in MapOfCamera)
//            {
//                if (item.Value.Name.Contains("CXK"))
//                {
//                    nRet = HIK_Camera_Operator.StartTriggerOnce(item.Value);
//                    if (nRet == 0)
//                    {
//                        Thread.Sleep(500);
//                        var imgdata = HIK_Camera_Operator.ReceiveThreadProcess(item.Value);
//                        Image image = Image.LoadPixelData<L8>(imgdata.Data, imgdata.Width, imgdata.Height);
//                        images.Add(imgdata.CameraName, image);
//                    }
//                    Thread.Sleep(500);

//                }
//            }
//            return images;
//        }
//        #endregion
//    }
//}
