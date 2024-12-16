

using MvCamCtrl.NET;
using System.Security.Cryptography;

namespace 全面瑕疵检测.Common.HIKCamera
{
    internal class HIK_GIGE_Camera
    {
		private string name;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private int id;

		public int ID
		{
			get { return id; }
			set { id = value; }
		}

		private string? serialNum;

		public string? SerialNum
		{
			get { return serialNum; }
			set { serialNum = value; }
		}

		private float expousureTime;

		public float ExpoursurTime
		{
			get { return expousureTime; }
			set { expousureTime = value; }
		}

		private bool triggerMode=false;
		public bool TriggerMode { get=>triggerMode; set => triggerMode = value; }

        private bool hardTriggerMode = false;
        public bool HardTriggerMode { get => hardTriggerMode; set => hardTriggerMode = value; }
        private bool softTriggerMode = false;
        public bool SoftTriggerMode { get => softTriggerMode; set => softTriggerMode = value; }


        public static EventHandler? ConnectedChanged { get; set; }

		private bool connected;

		public bool Connected
		{
			get { return connected; }
			set 
			{
				if(connected != value)
				{
					connected = value;
					ConnectedChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public static EventHandler? GrabbingChanged { get; set; }

		private bool grabbing;

		public bool Grabbing
		{
			get { return grabbing; }
			set 
			{
				if(grabbing != value)
				{
					grabbing = value;
					GrabbingChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private string ip;

		public string CurrentIP
		{
			get { return ip; }
			set { ip = value; }
		}


		public MyCamera MyCamera = new MyCamera();
        public MyCamera.MV_CC_DEVICE_INFO DeviceInfo;

        public HIK_GIGE_Camera(string name, int id, string serialNumber, MyCamera.MV_CC_DEVICE_INFO device,uint ip)
        {
            this.name = name;
            this.id = id;
            this.serialNum = serialNumber;
            this.DeviceInfo = device;
			this.ip = UintToIP(ip);
        }
        static string UintToIP(uint ipAddressUint)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddressUint);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
        }

    }
}
