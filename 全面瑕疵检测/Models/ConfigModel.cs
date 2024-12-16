using Microsoft.Extensions.Configuration;
using System.IO;

namespace 全面瑕疵检测.Models
{
    public class ConfigModel
    {
        public static EventHandler? UpdateSettingHandler{ get; set; }

        public void Update()
        {
            UpdateSettingHandler?.Invoke(this,EventArgs.Empty);
            SaveAppSettings();
            SaveUserConfig();
        }
        static ConfigModel instance=new ConfigModel();
        public static ConfigModel Instance { get { return instance; } }
        public readonly IConfiguration configuration;
        public ConfigModel() 
        {
            string currentDir = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(currentDir);
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile("userconfig.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public void SaveAppSettings()
        {
            string path = "appsettings.json";
            // 将修改后的配置重新保存到 JSON 文件
            Dictionary<string, Dictionary<string, string>> settings = new Dictionary<string, Dictionary<string, string>>();
            var json = configuration.GetSection("appsettings").Get<Dictionary<string, string>>();

            if (json != null)
            {
                settings.Add("appsettings",json);
                File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            
        }

        public void SaveUserConfig()
        {
            string path = "userconfig.json";
            // 将修改后的配置重新保存到 JSON 文件
            Dictionary<string, Dictionary<string, string>> settings = new Dictionary<string, Dictionary<string, string>>();
            var json = configuration.GetSection("userconfig").Get<Dictionary<string, string>>();

            if (json != null)
            {
                settings.Add("userconfig", json);
                File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }

        }
    }

}
