using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using NLog;
using System.Windows;
using 全面瑕疵检测.Services;
using 全面瑕疵检测.Views;
using 全面瑕疵检测.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using 全面瑕疵检测.Common.HIKCamera;

namespace 全面瑕疵检测
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            LogManager.Configuration = new XmlLoggingConfiguration("Resources\\nlog.config");
#if !DEBUG
            NlogService.Info("程序启动");
#endif
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var _config = ConfigModel.Instance.configuration.GetSection("appsettings");
            if (_config != null && _config.Exists())//判断是否读取到内容
            {
                var appMode = _config["app_mode"];
                if (appMode == "Manual")//手动模式什么也不做
                {
                    LoadingView loadingView = new LoadingView();
                    loadingView.ShowMsg("系统初始化中...");
                    loadingView.Show();
                    // ******************系统初始化*************************
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        WoodInfoModel.Load(woodInfoPath); 
                        var plc=Services.GetService<PLCService>();
                        string msg;
                        bool bRet = true;
                        #region plc连接初始化
                        if (plc != null)
                        {
                            msg = "正在进行plc连接";
                            loadingView.ShowMsg(msg);
                            NlogService.Info($"系统初始化，{msg}");
                            Thread.Sleep(1000);
                            
                            bRet = plc.Init();
                            plc.Connect();
                            bRet = plc.helper?.Connected?? false;
                            if (bRet)// 连接成功
                            {
                                msg = "plc连接成功！";
                                loadingView.ShowMsg(msg);
                                NlogService.Info($"系统初始化，{msg}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                msg = "plc连接失败！";
                                loadingView.ShowMsg(msg);
                                NlogService.Info($"系统初始化，{msg}");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            msg = "无法获取plc服务对象！";
                            loadingView.ShowMsg(msg);
                            NlogService.Info($"系统初始化，{msg}");
                            Thread.Sleep(1000);
                        }
                        #endregion

                        #region 相机初始化
                        var camera =Services.GetService<ICameraService>();
                        if(camera != null)
                        {
                            msg = "正在进行相机初始化";
                            loadingView.ShowMsg(msg);
                            NlogService.Info($"系统初始化，{msg}");
                            Thread.Sleep(1000);
                            if (camera.InitCameras())
                            {
                                msg = "相机初始化成功！";
                                loadingView.ShowMsg(msg);
                                NlogService.Info($"系统初始化，{msg}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                msg = "相机初始化失败！";
                                loadingView.ShowMsg(msg);
                                NlogService.Info($"系统初始化，{msg}");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            msg = "无法获取相机服务对象！";
                            loadingView.ShowMsg(msg);
                            NlogService.Info($"系统初始化，{msg}");
                            Thread.Sleep(1000);
                        }
                        #endregion

                        msg = "正在开启检测后台.....";
                        loadingView.ShowMsg(msg);
                        NlogService.Info($"系统初始化，{msg}");
                        Task.Run(() =>
                        {
                            RunPythonModel.LoadBatPath();
                            RunPythonModel.RunPython();
                        });
                        Thread.Sleep(1000);
                        if (RunPythonModel.Running)
                        {
                            msg = "检测后台开启成功！";
                            loadingView.ShowMsg(msg);
                            NlogService.Info($"系统初始化，{msg}");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            msg = "检测后台开启失败！";
                            loadingView.ShowMsg(msg);
                            NlogService.Info($"系统初始化，{msg}");
                            Thread.Sleep(1000);
                        }

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MainView mainView = new MainView();
                            mainView.Show();
                            loadingView.Close();
                        }));
                    });
                    
                }else if (appMode == "Auto")//自动模式
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        MainView mainView = new MainView();
                        mainView.Show();
                    }));
                }
            }
            else
            {
                NlogService.Error($"{typeof(PLCService).Name}初始化时发生错误：读取不到配置文件");
            }
            
        }
        string woodInfoPath = "Resources\\woodinfos.json";
        protected override void OnExit(ExitEventArgs e)
        {
            WoodInfoModel.Save(woodInfoPath);
            ConfigModel.Instance.SaveAppSettings();
            ConfigModel.Instance.SaveUserConfig();
            WeakReferenceMessenger.Default.Reset();
            WeakReferenceMessenger.Default.Cleanup();
            base.OnExit(e);
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            /***********services***********/
            services.AddTransient<CameraServiceV1>();
            services.AddSingleton<PLCService>();
            
            /***********views***********/
            services.AddTransient<PLCControlView>();
            services.AddTransient<SystemConfigView>();
            services.AddTransient<DetectionView>();
            services.AddTransient<WoodsConfigView>();
            return services.BuildServiceProvider();
        }
    }

}
