
namespace 全面瑕疵检测
{
    public class GlobalStatus
    {
        // 定义静态变量来保存状态
        private static bool plcAvailable = false;
        private static bool cameraAvailable = false;
        private static bool programExecutable = false;
        private static bool pythonAvailable = false;

        public static EventHandler<bool>? PLCStatusChanged;
        public static EventHandler<bool>? CameraStatusChanged;
        public static EventHandler<bool>? ProgramStatusChanged;
        public static EventHandler<bool>? PythonStatusChanged;

        // 属性来访问状态
        /// <summary>
        /// Python程序是否正常运行
        /// </summary>
        public static bool PythonAvailable
        {
            get { return pythonAvailable; }
            private set { 
                if(pythonAvailable!=value)
                {
                    pythonAvailable = value;
                    PythonStatusChanged?.Invoke(null,value);
                }    
            }
        }

        /// <summary>
        /// plc的可用状态
        /// </summary>
        public static bool PlcAvailable
        {
            get { return plcAvailable; }
            private set { 
                if(plcAvailable!=value)
                {
                    plcAvailable = value;
                    PLCStatusChanged?.Invoke(null, value);
                }
            }
        }

        /// <summary>
        /// 相机的可用状态
        /// </summary>
        public static bool CameraAvailable
        {
            get { return cameraAvailable; }
            private set {
                if (cameraAvailable != value)
                {
                    cameraAvailable = value;
                    CameraStatusChanged?.Invoke(null, value);
                }
            }
        }

        /// <summary>
        /// 程序的可执行状态
        /// </summary>
        public static bool ProgramExecutable
        {
            get { return programExecutable; }
            private set {
                if (programExecutable != value)
                {
                    programExecutable = value;
                    ProgramStatusChanged?.Invoke(null, value);
                }
            }
        }

        // 方法来更新状态
        public static void UpdatePLCStatus(bool plcStatus)
        {
            PlcAvailable = plcStatus;
        }
        public static void UpdateCameraStatus(bool cameraStatus)
        {
            CameraAvailable = cameraStatus;
        }

        public static void UpdatePythonStatus(bool pythonStatus)
        {
            PythonAvailable = pythonStatus;
        }

        public static void UpdateProgramStatus(bool programStatus)
        {
            ProgramExecutable = programStatus;
        }
    }
}
