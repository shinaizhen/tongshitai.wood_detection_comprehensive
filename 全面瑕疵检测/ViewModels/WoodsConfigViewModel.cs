using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using 全面瑕疵检测.Models;

namespace 全面瑕疵检测.ViewModels
{
    internal partial class WoodsConfigViewModel : ObservableObject
    {
        
        public WoodsConfigViewModel()
        {
            WoodInfos = WoodInfoModel.AllWood;
        }

        [ObservableProperty]
        private WoodInfo? selectedWoodInfo=new WoodInfo();

        [ObservableProperty]
        private ObservableCollection<WoodInfo> woodInfos;

        [RelayCommand]
        private void Select(string or)
        {
            var woodInfo=WoodInfos.FirstOrDefault((w)=>w.ORCode==or);
            if (woodInfo!=null)
            {
                SelectedWoodInfo = new WoodInfo() 
                {
                    ORCode = woodInfo.ORCode,
                    Width = woodInfo.Width,
                    Height = woodInfo.Height,
                    MaxWindth = woodInfo.MaxWindth,
                    MiddleOffeset = woodInfo.MiddleOffeset,
                };
            }
            else
            {
                MessageModel.ShowError($"无法找到木板型号{or}！");
            }
        }

        [RelayCommand]
        private void Delete(string or)
        {
            var woodInfo = WoodInfos.FirstOrDefault((w) => w.ORCode == or);
            if (woodInfo != null)
            {
                WoodInfos.Remove(woodInfo);
            }
            else
            {
                MessageModel.ShowError($"无法找到木板型号{or}！");
            }
        }

        [RelayCommand]
        private void Add()
        {
            if(SelectedWoodInfo==null)
            {
                MessageModel.ShowInfo("请填写正确信息！");
                return;
            }
            var wood = WoodInfos.FirstOrDefault((w)=>w.ORCode == SelectedWoodInfo.ORCode);
            if (wood!=null)
            {
                MessageModel.ShowInfo("该型号已存在！");
                return;
            }
            WoodInfos.Add(new WoodInfo() { ORCode=SelectedWoodInfo.ORCode,Width=SelectedWoodInfo.Width,Height=SelectedWoodInfo.Height,MaxWindth= SelectedWoodInfo.MaxWindth,MiddleOffeset= SelectedWoodInfo.MiddleOffeset});
        }

        [RelayCommand]
        private void LoadFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = "json文件|.json";
            openFileDialog.ShowDialog();
            if(openFileDialog.CheckFileExists)
            {
                WoodInfoModel.Load(openFileDialog.FileName);
            }
        }

        [RelayCommand] 
        private void SaveToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "json文件|.json";
            saveFileDialog.ShowDialog();
            WoodInfoModel.Save(saveFileDialog.FileName);
        }

        [RelayCommand]
        private void Update()
        {
            if(SelectedWoodInfo == null)
            {
                MessageModel.ShowInfo("选择更改项为空！");
                return;
            }
            var wood = WoodInfos.FirstOrDefault((w) => w.ORCode == SelectedWoodInfo.ORCode);
            if (wood != null)
            {
                wood.ORCode = SelectedWoodInfo.ORCode;
                wood.Width = SelectedWoodInfo.Width;
                wood.Height = SelectedWoodInfo.Height;
                wood.MiddleOffeset= SelectedWoodInfo.MiddleOffeset;
                wood.MaxWindth= SelectedWoodInfo.MaxWindth;
            }
        }
    }
}
