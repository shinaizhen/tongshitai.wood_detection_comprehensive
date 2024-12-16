using MvCamCtrl.NET;
using OpenCvSharp;
using System.Runtime.InteropServices;
using 全面瑕疵检测.Common;
using 全面瑕疵检测.Common.HIKCamera;

namespace 全面瑕疵检测.Services
{
    /// <summary>
    /// 相机服务器v1版本
    /// </summary>
    internal class CameraServiceV1 : ICameraService
    {
        static MyCamera.cbExceptiondelegate? Exceptiondelegate;
        public static EventHandler? CameraDisconnected;

        public static List<HIK_GIGE_Camera> HIK_GIGE_Cameras = new List<HIK_GIGE_Camera>();
        // 存储需要打卡的相机名称
        public static readonly List<string> cameraNames = new()
        {
               "SXK1","SXK2","SXK3","SXK4","SXK5","SXK6","SXK7","SXK8",
               "CXK1","CXK2","CXK3","CXK4","CXK5","CXK6"
        };

        public CameraServiceV1() 
        {
            Exceptiondelegate = new MyCamera.cbExceptiondelegate(DisConnectedCallbackFunc);
            GC.KeepAlive(Exceptiondelegate);
        }

        private void DisConnectedCallbackFunc(uint nMsgType, IntPtr pUser)
        {
            if(nMsgType==MyCamera.MV_EXCEPTION_DEV_DISCONNECT)//相机断开连接
            {
                int id = (int)pUser;
                var cameraInfo = GetCameraBYID(id);
                if (cameraInfo == null)
                    return;
                CameraDisconnected?.Invoke(cameraInfo,EventArgs.Empty);
                Close(cameraInfo);
                while (cameraInfo.Connected == false)
                {
                    Open(cameraInfo);
                }
            }
        }

        private void Close(HIK_GIGE_Camera gIGE_Camera)
        {
            gIGE_Camera.Grabbing = false;
            gIGE_Camera.MyCamera.MV_CC_StopGrabbing_NET();
            gIGE_Camera.Connected = false;
            gIGE_Camera.MyCamera.MV_CC_CloseDevice_NET();
            gIGE_Camera.MyCamera.MV_CC_DestroyDevice_NET();
        }

        public HIK_GIGE_Camera? GetCameraBYID(int id)
        {
            var hIK_GIGE_Camera = HIK_GIGE_Cameras.FirstOrDefault(c => c.ID == id);
            if(hIK_GIGE_Camera == null)return null;
            return hIK_GIGE_Camera;
        }

        public HIK_GIGE_Camera? GetCameraBYName(string name)
        {
            var hIK_GIGE_Camera = HIK_GIGE_Cameras.FirstOrDefault(c => c.Name == name);
            if (hIK_GIGE_Camera == null) return null;
            return hIK_GIGE_Camera;
        }

        /// <summary>
        /// 关闭所有相机
        /// </summary>
        public void CloseAll()
        {
            foreach(var camera in  HIK_GIGE_Cameras)
            {
                camera.Grabbing = false;
                camera.MyCamera.MV_CC_StopGrabbing_NET();
                camera.Connected = false;
                camera.MyCamera.MV_CC_CloseDevice_NET();
            }
        }

        /// <summary>
        /// 设置相机参数
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int ConfigCamera(HIK_GIGE_Camera gIGE_Camera)
        {
            throw new NotImplementedException();
        }

        // 获取设备列表信息
        static MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        public int GetCameraList()
        {
            HIK_GIGE_Cameras.Clear();
            // ch:创建设备列表 | en:Create Device List
            m_stDeviceList.nDeviceNum = 0;
            System.GC.Collect();
            //枚举设备列表
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE, ref m_stDeviceList);//枚举千兆网口设备列表
            if (0 != nRet)//获取失败
            {
                PrintError("相机枚举设备失败！", nRet);
                return nRet;
            }
            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    if (gigeInfo.chUserDefinedName != "")
                    {
                        var camera = new HIK_GIGE_Camera(gigeInfo.chUserDefinedName, i, gigeInfo.chSerialNumber, device,gigeInfo.nCurrentIp);
                        HIK_GIGE_Cameras.Add(camera);
                    }
                }
            }
            return 0;
        }

        public SortedDictionary<string, Mat> GetCXKImages()
        {
            SortedDictionary<string,Mat> keyValuePairs = new SortedDictionary<string,Mat>();
            foreach(var item in HIK_GIGE_Cameras)
            {
                if (item.Name.Contains("SXK"))
                {
                    var image=GetImage(item);
                    if (image != null)
                        keyValuePairs.Add(item.Name, image);
                }
            }
            return keyValuePairs;
        }

        public SortedDictionary<string, Mat> GetSXKImages()
        {
            SortedDictionary<string, Mat> keyValuePairs = new SortedDictionary<string, Mat>();
            foreach (var item in HIK_GIGE_Cameras)
            {
                if (item.Name.Contains("CXK"))
                {
                    var image = GetImage(item);
                    if (image != null)
                    {
                        // 旋转270
                        Cv2.Rotate(image,image,RotateFlags.Rotate90Counterclockwise);
                        keyValuePairs.Add(item.Name, image);
                    }
                }
            }
            return keyValuePairs;
        }

        public bool InitCameras()
        {
            int nRet = 0;
            if (HIK_GIGE_Cameras.Count == 0)
            {
                nRet = GetCameraList();
                if (nRet != 0)
                {
                    return false;
                }
            }
            foreach(string name in cameraNames)
            {
                var cameraInfo=GetCameraBYName(name);
                if(cameraInfo==null)
                {
                    PrintError($"设备{name}:没有在设备列表中！",-1);
                    return false;
                }
                if(CheckIP.PingIP(cameraInfo.CurrentIP)==false)
                {
                    PrintError($"设备{cameraInfo.Name}:IP地址ping不通！",-1);
                    return false;
                }
                nRet = Open(cameraInfo);
                if(nRet != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public int LoadCameraConfigs(string filePath)
        {
            throw new NotImplementedException();
        }

        public int OpenAllCamera()
        {
            if(HIK_GIGE_Cameras.Count == 0)
            {
                PrintError("相机列表为空！",0);
                return -1;
            }

            foreach(var gigeCamera in HIK_GIGE_Cameras)
            {
                if (cameraNames.Contains(gigeCamera.Name ?? "unknown"))
                {
                    continue;
                }
                int nRet=Open(gigeCamera);
                if (nRet != 0)
                    return nRet;

            }
            return 0;
        }

        Mat? GetImage(HIK_GIGE_Camera gIGE_Camera)
        {
            Thread.Sleep(500);
            MyCamera.MV_FRAME_OUT mV_FRAME_OUT = new MyCamera.MV_FRAME_OUT();
            try
            {
                gIGE_Camera.MyCamera.MV_CC_GetImageBuffer_NET(ref mV_FRAME_OUT, 1000);
                var stFrameInfo = mV_FRAME_OUT.stFrameInfo;
                byte[] bytes = new byte[stFrameInfo.nWidth * stFrameInfo.nHeight];
                Marshal.Copy(mV_FRAME_OUT.pBufAddr, bytes, 0, (int)stFrameInfo.nFrameLen);
                Mat? mat;
                if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    mat=Mat.FromPixelData(stFrameInfo.nHeight, stFrameInfo.nWidth, MatType.CV_8UC1, bytes);
                    return mat;
                }
                return null;

            }
            catch (Exception ex)
            {
                PrintError($"获取图像数据时发生错误：{ex.Message}", 0);
                return null;
            }
            finally { gIGE_Camera.MyCamera.MV_CC_FreeImageBuffer_NET(ref mV_FRAME_OUT); }
        }

        /// <summary>
        /// 获取参数曝光、增益、帧率、触发延时
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static int GetParam(HIK_GIGE_Camera info)
        {
            MyCamera camera = info.MyCamera;
            int nRet = 0;
            // ch:获取参数 
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            nRet = camera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);//获取曝光时间
            if (MyCamera.MV_OK == nRet)
            {
                info.ExpoursurTime = stParam.fCurValue;
            }
            else
            {
                PrintError($"设备{info.Name}，获取曝光时间失败！", nRet);
                return nRet;
            }
            return nRet;
        }

        public int Open(HIK_GIGE_Camera hIK_GIGE_Camera)
        {
            MyCamera camera = hIK_GIGE_Camera.MyCamera;
            if (camera == null) return -1;
            hIK_GIGE_Camera.Connected = false;
            int nRet=camera.MV_CC_CreateDevice_NET(ref hIK_GIGE_Camera.DeviceInfo);
            if(nRet !=0)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：获取句柄失败", 0);
                return nRet;
            }

            nRet = camera.MV_CC_OpenDevice_NET();
            if(nRet !=0)
            {
                camera.MV_CC_DestroyDevice_NET();
                PrintError($"设备（{hIK_GIGE_Camera.Name}）打开失败！", nRet);
                return nRet;
            }
            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
            if (hIK_GIGE_Camera.DeviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = camera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = camera.MV_CC_SetIntValueEx_NET("GevSCPSPacketSize", nPacketSize);
                    if (nRet != MyCamera.MV_OK)
                    {
                        PrintError($"设备{hIK_GIGE_Camera.Name}：网络最佳包设置失败", nRet);
                        return nRet;
                    }
                }
                else
                {
                    PrintError($"设备{hIK_GIGE_Camera.Name}：网络最佳包获取失败！", nPacketSize);
                    return nPacketSize;
                }
            }
            if (hIK_GIGE_Camera.Name.Contains("SXK"))
            {
                hIK_GIGE_Camera.ExpoursurTime = 1200000;
            }
            else
            {
                hIK_GIGE_Camera.ExpoursurTime = 800000;
            }
            camera.MV_CC_SetExposureTime_NET(hIK_GIGE_Camera.ExpoursurTime);
            if (nRet != 0)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置曝光时间失败！", 0);
                return nRet;
            }

            nRet = camera.MV_CC_SetTriggerMode_NET((uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            if (nRet != 0)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：打开触发模式失败！", 0);
                hIK_GIGE_Camera.TriggerMode = false;
                return nRet;
            }
            hIK_GIGE_Camera.TriggerMode = true;

            nRet = camera.MV_CC_SetTriggerSource_NET((uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            if (nRet != 0)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置软触发模式失败！", 0);
                hIK_GIGE_Camera.SoftTriggerMode = false;
                return nRet;
            }
            hIK_GIGE_Camera.SoftTriggerMode = true;

            nRet = camera.MV_CC_SetExposureAutoMode_NET((uint)MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_OFF);
            if (nRet != 0)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：关闭自动曝光失败！", 0);
                return nRet;
            }
            
            nRet = camera.MV_GIGE_SetGvspTimeout_NET(300);
            if (MyCamera.MV_OK != nRet)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置GVSP超时失败", nRet);
                return nRet;
            }
            nRet = camera.MV_GIGE_SetResendMaxRetryTimes_NET(50);
            if (MyCamera.MV_OK != nRet)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置重传命令最大尝试次数失败", nRet);
                return nRet;
            }
            nRet = camera.MV_GIGE_SetResendTimeInterval_NET(20);
            if (MyCamera.MV_OK != nRet)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置同一重传包多次请求之间的时间间隔失败", nRet);
                return nRet;
            }
            nRet = camera.MV_GIGE_SetRetryGvcpTimes_NET(50);
            if (MyCamera.MV_OK != nRet)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置重传GVCP命令次数失败", 0);
                return nRet;
            }
            nRet = camera.MV_GIGE_SetGvcpTimeout_NET(500);
            if (MyCamera.MV_OK != nRet)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：设置GVCP命令超时时间失败", nRet);
            }
            hIK_GIGE_Camera.Connected = true;
            nRet = RegisterDisconnectedCallback(hIK_GIGE_Camera);
            if(nRet!=0)
                return nRet;
            nRet = camera.MV_CC_StartGrabbing_NET();
            if(MyCamera.MV_OK != nRet)
            {
                PrintError($"设备{hIK_GIGE_Camera.Name}：开启采集失败！", nRet);
                return nRet;
            }
            return nRet;
        }

        public int RegisterDisconnectedCallback(HIK_GIGE_Camera gIGE_Camera)
        {
            int nRet = gIGE_Camera.MyCamera.MV_CC_RegisterExceptionCallBack_NET(Exceptiondelegate, (IntPtr)gIGE_Camera.ID);
            if (nRet != 0)
            {
                PrintError($"设备{gIGE_Camera.Name}:注册回调函数失败！", nRet);
            }
            return nRet;
        }

        public int SaveCameraConfigs(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool CheckAllCameraOpened()
        {
            foreach (string name in cameraNames)
            {
                var camera=HIK_GIGE_Cameras.FirstOrDefault(x => x.Name == name);
                if(camera ==  null)
                    return false;
                if(camera.Connected==false) return false;
            }
            return true;
        }

        /// 打印相机错误码信息
        /// </summary>
        /// <param name="csMessage">操作信息</param>
        /// <param name="nErrorNum">错误码</param>
        /// <returns></returns>
        public static void PrintError(string csMessage, int nErrorNum)
        {
            string Message;

            if (nErrorNum == 0)
            {
                Message = csMessage;
            }
            else
            {
                Message = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: Message += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: Message += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: Message += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: Message += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: Message += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: Message += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: Message += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: Message += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: Message += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: Message += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: Message += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: Message += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: Message += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: Message += " No permission "; break;
                case MyCamera.MV_E_BUSY: Message += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: Message += " Network error "; break;
            }
            Message += "PROMPT";
            if (nErrorNum == 0)
                NlogService.Info(Message);
            else
                NlogService.Error(Message);
        }
    }
}
