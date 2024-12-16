using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace 全面瑕疵检测.Models
{
    internal partial class WoodInfoModel
    {
        private static ObservableCollection<WoodInfo> allWood=new ObservableCollection<WoodInfo>();
        public static ObservableCollection<WoodInfo> AllWood
        {
            get { return allWood; }
            set {
                if(allWood != value)
                {
                    allWood = value;
                    UpdateAllWood?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 保存到制定文件
        /// </summary>
        /// <param name="path"></param>
        public static void Save(string path)
        {
            var jsonStr = JsonSerializer.Serialize(allWood);
            File.WriteAllText(path, jsonStr);
        }

        /// <summary>
        /// 从指定文件加载
        /// </summary>
        /// <param name="path"></param>
        public static void Load(string path)
        {
            if(File.Exists(path))
            {
                var txt = File.ReadAllText(path);
                var woodInfoList = JsonSerializer.Deserialize<List<WoodInfo>>(txt);
                if(woodInfoList != null && woodInfoList.Count > 0)
                {
                    AllWood.Clear();
                    foreach(var wood in woodInfoList)
                    {
                        AllWood.Add(wood);
                    }
                }
            }

        }

        /// <summary>
        /// 更新木板信息列表
        /// </summary>
        public static EventHandler? UpdateAllWood;
        /// <summary>
        /// 更新全局木板型号
        /// </summary>
        public static EventHandler? UpdateWoodInfo;

        static WoodInfo? globalWoodInfo;
        public static WoodInfo? GlobalWoodInfo
        {
            get { return globalWoodInfo; }
            set
            {
                if(globalWoodInfo != value)
                {
                    globalWoodInfo = value;
                    UpdateWoodInfo?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        
    }

    internal partial class WoodInfo:ObservableObject
    {
        private string? oRCode;
        public string? ORCode
        {
            get { return oRCode; }
            set { SetProperty(ref oRCode, value?.ToUpper()); }
        }
        [ObservableProperty]
        private int width;
        [ObservableProperty]
        private int height;
        [ObservableProperty]
        private int middleOffeset=213;//移动到中心位置的偏移
        [ObservableProperty]
        private int maxWindth = 445;//最长型号宽度
    }
}
