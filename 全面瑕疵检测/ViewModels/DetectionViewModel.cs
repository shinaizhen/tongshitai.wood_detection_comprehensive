using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using 全面瑕疵检测.Models;
using 全面瑕疵检测.Services;
using 全面瑕疵检测.Views;

namespace 全面瑕疵检测.ViewModels
{
    /// <summary>
    /// 检测模型
    /// 
    /// </summary>
    internal partial class DetectionViewModel: ObservableObject
    {
        PLCService? plc { get; set; }
        CameraServiceV1 camera { get; set; }

        public DetectionViewModel()
        {
            camera = new CameraServiceV1();
            WoodInfos = WoodInfoModel.AllWood;
            plc = App.Current.Services.GetService<PLCService>();
            if( plc != null )
            {
                plc.UpdateWaitingWoodSignalHandler += WaitingWoodSignalChanged;// 监听等待木板到来信号
            }
            
            if(DetectionModel.Classes!= null)
            {
                for (int i = 0; i < DetectionModel.Classes.Length; i++)
                {
                    DetectClasses.Add(new DetectionModel() { ID = i, Name = i.ToString() + ":" + DetectionModel.Classes[i], IsOK = true });
                }
            }

            SelectedWoodInfo = WoodInfoModel.GlobalWoodInfo;

            GlobalStatus.ProgramStatusChanged += ProgramStatusChangedFunc;
            ListenningTaskQueue();

        }

        private void ProgramStatusChangedFunc(object? sender, bool e)
        {
            IsRunning = !e;
        }

        [ObservableProperty]
        private bool isRunning = false;

        partial void OnIsRunningChanging(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                GlobalStatus.UpdateProgramStatus(!newValue);
            }
        }


        #region 目标检测逻辑
        // 监控任务列表
        public void ListenningTaskQueue()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    try
                    {
                        DetectionService.taskQuenue.TryDequeue(out var task);
                        if (task == null)
                            continue;
                        var result = DetectionService.PerformDetection(task);
                        if (result == null)
                            continue;
                        var _config = ConfigModel.Instance.configuration.GetSection("userconfig");
                        if (_config == null)
                        {
                            MessageModel.ShowError("无法获取系统磁盘信息");
                            return;
                        }
                        var diskName = _config["disk_name"];
                        var savePath = diskName + "tongshitai\\" + "data_json\\"+ DateTime.Now.ToString("yy_MM_dd")+".json";
                        result.Save(savePath);

                        // 更新UI图像显示
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (task.TaskFlag == DetectionService.DetectionFlag.A)
                            {
                                BitmapImageA = result.ToBitmapImage();
                            }
                            else if (task.TaskFlag == DetectionService.DetectionFlag.B)
                            {
                                BitmapImageB = result.ToBitmapImage();
                            }
                            else if (task.TaskFlag == DetectionService.DetectionFlag.C)
                            {
                                BitmapImageC = result.ToBitmapImage();
                            }
                            else if (task.TaskFlag == DetectionService.DetectionFlag.D)
                            {
                                BitmapImageD = result.ToBitmapImage();
                            }
                            else if (task.TaskFlag == DetectionService.DetectionFlag.E)
                            {
                                BitmapImageE = result.ToBitmapImage();
                            }
                            else if (task.TaskFlag == DetectionService.DetectionFlag.F)
                            {
                                BitmapImageF = result.ToBitmapImage();
                            }
                            else
                            {
                                //TODO: 其他任务类型
                            }
                        });

                        
                        // 更新检测结果
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            foreach(var cls in result.Results)
                            {
                                DetectClasses[cls.ID].IsOK = false;
                                IsOK = false;
                            }
                        });
                    }catch(Exception ex)
                    {
                        NlogService.Error(ex.Message);
                    }
                    continue;
                }
            });
        }

        

       #endregion
        // 当开始检测任务时，初始化UI
       private void InitUIThenStartDetection()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                BitmapImageA = null;
                BitmapImageB = null;
                BitmapImageC = null;
                BitmapImageD = null;
                BitmapImageE = null;
                BitmapImageF = null;
                IsOK = true;
                foreach (var cls in DetectClasses)
                {
                    cls.IsOK = true;
                }
            });
        }

        // 等待木板到来信号
        private void WaitingWoodSignalChanged(object? sender, bool e)
        {
            Task.Run(() =>
            {
                if (e)// 等待木板到来信号
                {
                    try
                    {
                        if (IsRunning == true)
                        {
                            return;
                        }
                        IsRunning = true;
                        StartDetection();
                    }
                    catch (Exception ex)
                    {
                        NlogService.Error($"检测时发生错误：{ex.Message}");
                    }
                    finally { IsRunning = false; }
                }
            });
        }

        #region 木板型号选择
        [ObservableProperty]
        private ObservableCollection<WoodInfo>? woodInfos;

        [ObservableProperty]
        private WoodInfo? selectedWoodInfo;

        [RelayCommand]
        private void SelectWoodInfo()
        {
            if(SelectedWoodInfo != null)
            {
                WoodInfoModel.GlobalWoodInfo = SelectedWoodInfo;
            }
        }
        #endregion

        #region 图像显示

        [ObservableProperty]
        private BitmapImage? bitmapImageA;

        [ObservableProperty]
        private BitmapImage? bitmapImageB;

        [ObservableProperty]
        private BitmapImage? bitmapImageC;

        [ObservableProperty]
        private BitmapImage? bitmapImageD;

        [ObservableProperty]
        private BitmapImage? bitmapImageE;

        [ObservableProperty]
        private BitmapImage? bitmapImageF;
        #endregion

        #region 检测结果显示
        [ObservableProperty]
        private bool isOK=true;

        [ObservableProperty]
        private ObservableCollection<DetectionModel> detectClasses = new ObservableCollection<DetectionModel>();
        #endregion

        [ObservableProperty]
        private bool isAutomaticMode = true; // 自动检测模式，初始值为 true，表示默认是自动检测

        [ObservableProperty]
        private bool canManual = false;// 手动检测模式是否可用

        // 当自动检测模式发生改变时需要做的事
        partial void OnIsAutomaticModeChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)// 当值发生改变时
                CanManual = !newValue;
        }

        /// <summary>
        /// 当检测结果ng时，触发警报
        /// </summary>
        /// <param name="value"></param>
        partial void OnIsOKChanged(bool value)
        {
            if (value)
                return;
            Task.Run(() =>
            {
                if (plc != null)
                {
                    plc.Alarm();
                    Thread.Sleep(3000);
                    plc.DisAlarm();
                }
            });
        }

        // 手动检测
        [RelayCommand]
        private void ManualDetection()
        {
            if (IsRunning == true)
            {
                return;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                IsRunning = true;
                CanManual = false;
            });
            Task.Run(() =>
            {
                try
                {
                    StartDetection();
                    //Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    NlogService.Error($"检测时发生错误：{ex.Message}");
                }
                finally 
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        IsRunning = false;
                        CanManual = true;
                    });
                }
            });
        }

        private void StartDetection()
        {
            InitUIThenStartDetection();// 初始化UI
            #region 检查是否可以运行
            if (SelectedWoodInfo == null)
            {
                MessageModel.ShowError("请选择木板型号！");
                return;
            }
            var wood = SelectedWoodInfo;


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

            if (plc == null || camera == null)
            {
                MessageModel.ShowError("plc或相机服务器缺失！");
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
            if (camera.CheckAllCameraOpened() == false)
            {
                MessageModel.ShowError("相机没有完全打开！");
                return;
            }
            #endregion
            // 系统配置
            var saveDir = diskName + "tongshitai\\" + "images\\" + DateTime.Now.ToString("yy-MM-dd") + "\\" + DateTime.Now.ToString("hh-mm-ss")+"_"+wood.ORCode + "\\";

            #region 第一次拍照
            bool bRet = plc.MoveToPositionOne(wood);
            if (bRet == false)
            {
                return;
            }
            else
            {
                plc.TurnOnTheUpperLightSource();
                var imgsA = camera.GetSXKImages();
                Task.Run(() =>
                {
                    string path = Path.Combine(saveDir, "imgA.jpg");
                    AddDetectionTask(imgsA, DetectionService.DetectionFlag.A,path);
                });
            }
            #endregion

            #region 第二次拍照
            bRet = plc.MoveToPositionTwo(wood);
            if (bRet == false)
            {
                return;
            }
            else
            {
                var imgsB = camera.GetSXKImages();
                Task.Run(() =>
                {
                    string path = Path.Combine(saveDir, "imgB.jpg");
                    AddDetectionTask(imgsB, DetectionService.DetectionFlag.B, path);
                });
            }

            #endregion
            #region 第三次拍照
            bRet = plc.MoveToPositionThree();
            if (bRet == false)
            {
                return;
            }
            else
            {
                plc.TurnOnTheLowerLightSource();
                var imgsC = camera.GetSXKImages();
                var imgsD = camera.GetCXKImages();
                Task.Run(() =>
                {
                    string path = Path.Combine(saveDir, "imgC.jpg");
                    AddDetectionTask(imgsC, DetectionService.DetectionFlag.C, path);
                    path = Path.Combine(saveDir, "imgD.jpg");
                    AddDetectionTask(imgsD, DetectionService.DetectionFlag.D, path);
                });
            }
            #endregion

            #region 第四次拍照

            bRet = plc.MoveToPositionFour(wood);
            if (bRet == false)
            {
                return;
            }
            else
            {
                var imgsE = camera.GetSXKImages();
                var imgsF = camera.GetCXKImages();
                Task.Run(() =>
                {
                    string path = Path.Combine(saveDir, "imgE.jpg");
                    AddDetectionTask(imgsE, DetectionService.DetectionFlag.E, path);
                    path = Path.Combine(saveDir, "imgF.jpg");
                    AddDetectionTask(imgsF, DetectionService.DetectionFlag.F, path);
                });
            }
            plc.TurnOffTheLowerLightSource();
            plc.TurnOffTheUpperLightSource();
            plc?.ReturnZero();
            #endregion
        }

        [RelayCommand]
        private void Change(object obj)
        {
            var imgname=obj.ToString();
            if(imgname =="A")
            {
                if (BitmapImageA == null)
                    return;
                ImageView imageView = new ImageView(BitmapImageA);
                imageView.Show();
            }
            if (imgname == "B")
            {
                if (BitmapImageB == null)
                    return;
                ImageView imageView = new ImageView(BitmapImageB);
                imageView.Show();
            }
            if (imgname == "C")
            {
                if (BitmapImageC == null)
                    return;
                ImageView imageView = new ImageView(BitmapImageC);
                imageView.Show();
            }
            if (imgname == "D")
            {
                if (BitmapImageD == null)
                    return;
                ImageView imageView = new ImageView(BitmapImageD);
                imageView.Show();
            }
            if (imgname == "E")
            {
                if (BitmapImageE == null)
                    return;
                ImageView imageView = new ImageView(BitmapImageE);
                imageView.Show();
            }
            if (imgname == "F")
            {
                if (BitmapImageF == null)
                    return;
                ImageView imageView = new ImageView(BitmapImageF);
                imageView.Show();
            }

        }

        /// <summary>
        /// 添加到检测任务队列
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <param name="flag"></param>
        public void AddDetectionTask(SortedDictionary<string,Mat> keyValuePairs,DetectionService.DetectionFlag flag,string savePath)
        {
            // 拼接图像
            var src = StitchImage(keyValuePairs);
            var imageData = new ImageData(flag, new Guid(), src.ToBytes(), src.Width, src.Height, src.Channels());
            imageData.SavePath = savePath;
            src.SaveImage(savePath);
            src.Dispose();
            DetectionService.AddDetectionTask(imageData);
        }
        /// <summary>
        /// 拼接图像
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public Mat StitchImage(SortedDictionary<string, Mat> keyValues) {
            int count = keyValues.Count;
            // 获取单张图像的宽高
            var img=keyValues.First().Value;
            int height=img.Height;
            int width=img.Width;
            Mat stitchImage = new Mat(height,width*count,img.Type());
            int index = 0;
            foreach(var keyValue in keyValues)
            {
                keyValue.Value.CopyTo(new Mat(stitchImage, new Rect(width * index, 0, width, height)));
                keyValue.Value.Dispose();
                index++;
            }
            return stitchImage;
        }
    }
}
