using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using 全面瑕疵检测.Common.HIKCamera;

namespace 全面瑕疵检测.ViewModels
{
    internal partial class CameraConfigViewModel:ObservableObject
    {
        public CameraConfigViewModel() 
        {

        }

        ~CameraConfigViewModel() 
        {
            
        }

        [ObservableProperty]
        private ObservableCollection<HIK_GIGE_Camera> cameraList=new ObservableCollection<HIK_GIGE_Camera>();

        [ObservableProperty]
        private HIK_GIGE_Camera? selectedCamera;

        bool isSelectedChanged=false;
        partial void OnSelectedCameraChanged(HIK_GIGE_Camera? value)
        {
            if (value != null)
            {
                
                Opened = value.Connected;
                Grabbing = value.Grabbing;
                TriggerMode = value.TriggerMode;
                SoftTrigger=value.SoftTriggerMode;
            }
        }

        [ObservableProperty]
        private float expourserTime;
        [ObservableProperty]
        private float gain;
        [ObservableProperty]
        private float frameRate;
        [ObservableProperty]
        private uint nGevSCPD;
        [ObservableProperty]
        private uint nGevSCPS;
        [ObservableProperty]
        private bool opened=false;
        [ObservableProperty]
        private bool grabbing = false;
        [ObservableProperty]
        private bool triggerMode = false;

        

        partial void OnTriggerModeChanged(bool value)
        {
            if (!isSelectedChanged&&SelectedCamera!=null)
            {
            }
        }
        [ObservableProperty]
        private bool softTrigger=false;

        partial void OnSoftTriggerChanged(bool value)
        {
            if (!isSelectedChanged && SelectedCamera != null)
            {
            }
        }

        [RelayCommand]
        private void EnumDevice()
        {
            CameraList.Clear();
            
        }

        [RelayCommand]
        private void OpenDevice()
        {
           
        }
        [RelayCommand]
        private void CloseDevice()
        {
           
        }

        [RelayCommand]
        private void StartGrabbing()
        {
            
        }
        [RelayCommand]
        private void StopGrabbing()
        {
            
        }
    }
}
