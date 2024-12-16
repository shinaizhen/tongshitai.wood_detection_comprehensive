using Microsoft.Extensions.DependencyInjection;


namespace 全面瑕疵检测.ViewModels
{
    internal class ViewModelLocator
    {
        IServiceProvider Services;
        public ViewModelLocator() 
        {
            Services = ConfigureServices();
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<CameraConfigViewModel>();
            services.AddTransient<PLCConfigViewModel>();
            services.AddTransient<WoodsConfigViewModel>();
            services.AddSingleton<SystemConfigViewModel>();
            services.AddSingleton<DetectionViewModel>();

            return services.BuildServiceProvider();
        }

        public MainViewModel MainViewModel { get => Services.GetRequiredService<MainViewModel>(); }
        public SystemConfigViewModel SystemConfigViewModel { get => Services.GetRequiredService<SystemConfigViewModel>(); }
        public CameraConfigViewModel CameraConfigViewModel { get => Services.GetRequiredService<CameraConfigViewModel>(); }
        public DetectionViewModel DetectionViewModel { get => Services.GetRequiredService<DetectionViewModel>(); }
        public WoodsConfigViewModel WoodsConfigViewModel { get => Services.GetRequiredService<WoodsConfigViewModel>(); }
        public PLCConfigViewModel PLCConfigViewModel { get => Services.GetRequiredService<PLCConfigViewModel>(); }
    }
}
