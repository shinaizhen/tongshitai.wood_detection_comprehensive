using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Microsoft.Extensions.Configuration;
using 全面瑕疵检测.Models;
using NModbus;
using NLog;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;


namespace 全面瑕疵检测.Services
{
    internal class PLCService
    {
        /// <summary>
        /// 更新回零状态
        /// </summary>
        public EventHandler<bool>? UpdateReturnningZeroHandler { get; set; }
        /// <summary>
        /// 更新拍照位置状态
        /// </summary>
        public EventHandler<Position>? UpdatePointHandler { get; set; }
        /// <summary>
        /// 更新急停信号
        /// </summary>
        public EventHandler<bool>? UpdateEmergencyStopSignalHandler { get; set; }
        /// <summary>
        /// 更新木板等待状态
        /// </summary>
        public EventHandler<bool>? UpdateWaitingWoodSignalHandler { get; set; }
        /// <summary>
        /// 等待时间
        /// </summary>
        public int AwaitTime { get; private set; }
        /// <summary>
        /// mm距离转换为脉冲数的倍数
        /// </summary>
        public int Rate { get; private set; }
        
        private bool returningZero=false;
        public bool ReturnningZero
        {
            get => returningZero;
            private set
            {
                if(returningZero != value)
                {
                    returningZero = value;
                    UpdateReturnningZeroHandler?.Invoke(this,value);
                    if (value == true)// 正在回零
                    {
                        UpdatePointHandler?.Invoke(this,Position.None);
                    }
                }
            }
        }

        private bool waitingWood = true;
        public bool WaitingWood
        {
            get => waitingWood;
            private set
            {
                if (waitingWood != value)
                {
                    waitingWood = value;
                    UpdateWaitingWoodSignalHandler?.Invoke(this,value);
                }
            }
        }

        private bool emergencyStopSignal = false;
        public bool EmergencyStopSignal
        {
            get => emergencyStopSignal;
            private set
            {
                if (emergencyStopSignal != value)
                {
                    emergencyStopSignal= value;
                    UpdateEmergencyStopSignalHandler?.Invoke(this,value);
                }

            }
        }
        private int port;
        /// <summary>
        /// plc端口号
        /// </summary>
        public int Port
        {
            get { return port; }
        }
        public PLCHelper? helper { get; private set; }
        public PLCService() 
        {
            
        }

        public bool Init()
        {
            try
            {
                var _config = ConfigModel.Instance.configuration.GetSection("appsettings");
                if (_config != null && _config.Exists())//判断是否读取到内容
                {
                    AwaitTime = _config.GetValue<int>("await_time");
                    Rate = _config.GetValue<int>("rate_conversion");
                    var ip = _config.GetValue<string>("plc_ip");
                    var port = _config.GetValue<int>("plc_port");
                    if (ip == null)
                        return false;
                    helper = new PLCHelper(ip, port);
                }
                else
                {
                    PrintError($"{typeof(PLCService).Name}初始化时发生错误：读取不到配置文件");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                PrintError($"{typeof(PLCService).Name}初始化时发生错误：{ex.Message}");
                return false;
            }
        }

        public bool Connect()
        {
            return helper?.Connect()?? false;
        }

        public void DisConnect()
        {
            helper?.DisConnect();
        }

        /// <summary>
        /// 等待任务完成
        /// </summary>
        /// <returns>执行成功返回0，失败返回-1</returns>
        public int AwaitCompleted()
        {
            Thread.Sleep(AwaitTime);
            return 0;
        }


        public static void PrintError(string msg, bool isError = true)
        {
            if (isError)
            {
                string error = "";
                error += $"调用类：{typeof(PLCService)},错误：{msg}";
                NlogService.Error(error);
            }
            else
            {
                NlogService.Info(msg);
            }
        }
        #region 滑台电机模块
        /*
            * 滑动电机模块数据信息：
            * 地址|作用|ps
            * VW262|脉冲数|控制移动距离
            * V213.0|正反转|0：正转（远离原点）1：反转
            * V213.1|电机是否移动|控制电机移动|1：移动
            * V213.2|左滑台移动使能|0：false；1：true
            * V213.3|右滑台移动使能|0：false；1：true
            */
        /// <summary>
        /// 滑台电机移动
        /// </summary>
        /// <param name="moveLength">移动距离</param>
        /// <param name="direc">移动方向True:正转，Fals：反转</param>
        /// <param name="rightEnable">右滑台使能</param>
        /// <param name="leftEnable">左滑台使能(靠近配电柜)</param>
        /// <returns></returns>
        public bool SlidingTableMotorMovement(ushort moveLength, ushort speed = 10000, bool direc = true, bool rightEnable = false, bool leftEnable = false)
        {
            bool bRet = true;
            //写入脉冲数
            bRet = helper?.WriteSingleRegister(moveLength,31)?? false;
            if (bRet==false)
            {
                return false;
            }
            // 写入移动速度
            bRet = helper?.WriteSingleRegister(speed, 10)?? false;
            if (bRet==false)
            {
                return false;
            }

            // 读取原有寄存器地址的值
            var currentValueV213 = helper?.ReadSingleRegister(6);
            if (currentValueV213 == null)
            {
                return false;
            }
            ushort data= currentValueV213.Value;
            // 设置正反转（假设设置为正转）
            if (direc)
            {
                data &= (ushort)(~(1 << 0) & 0xFFFF); // 清除位 0，设置为正转（远离原点）
            }
            else
            {
                data |= (ushort)(1 << 0);
            }

            data |= (ushort)(1 << 1); // 设置位 1，电机移动

            // 设置左滑台移动使能（假设设置为使能）
            if (leftEnable)
            {
                data |= (ushort)(1 << 2); // 设置位 2，左滑台移动使能
            }
            else
            {
                data &= (ushort)(~(1 << 2) & 0xFFFF); // 清除位 2
            }

            // 设置右滑台移动使能（假设设置为使能）
            if (rightEnable)
            {
                data |= (ushort)(1 << 3); // 设置位 3，右滑台移动使能
            }
            else
            {
                data &= (ushort)(~(1 << 3) & 0xFFFF); // 清除位 3
            }

            bRet = helper?.WriteSingleRegister(data, 6) ?? false;
            if (bRet==false)
            {
                return false;
            }
            // 等待完成信号
            int res = AwaitCompleted();
            return res == 0;
    }
        #endregion
        #region 翻转电机
        /*
        * 翻转电机数据位信息
        * 寄存器地址|作用|ps
        * vw218|翻转电机脉冲数|ps：持续给脉冲后续需要转换为角度信息
        * v215.0|手动：翻转电机正反转状态
        * v215.1|手动：翻转电机是否移动
        * v215.2|手动：左翻转电机使能状态
        * v215.3|手动：右翻转电机使能状态
        */

        public bool SetFlipMotor(ushort flipAngle, ushort speed = 10000, bool direct = true, bool leftEnable = false, bool rightEnable = false)
        {
            
            bool bRet = true;
            // 写入脉冲数
            bRet = helper?.WriteSingleRegister(flipAngle,9)?? false;
            if(bRet == false)
            {
                return false;
            }
            // 写入速度
            bRet = helper?.WriteSingleRegister(speed,11)?? false;
            if(bRet == false)
            {
                return bRet;
            }

            // 读取控制参数
            var currentValue = helper?.ReadSingleRegister(7);
            if (currentValue == null)
                return false;

            ushort data=currentValue.Value;
            // 设置电机正反转
            if (direct)
            {
                data &= (ushort)(~(1 << 0) & 0xFFFF);//清除正反转信息，设为0；
            }
            else
            {
                data |= (ushort)(1 << 0);//设置正反转信息为1
            }
            data |= (ushort)(1 << 1);//设置移动信息

            //设置左翻转电机使能状态
            if (leftEnable)
            {
                data |= (ushort)(1 << 2);
            }
            else
            {
                data &= (ushort)(~(1 << 2) & 0xFFFF);
            }

            //设置右翻转电机使能状态
            if (rightEnable)
            {
                data |= (ushort)(1 << 3);
            }
            else
            {
                data &= (ushort)(~(1 << 3) & 0xFFFF);
            }
            bRet = helper?.WriteSingleRegister(data,7)?? false;
            if (bRet == false)
                return false;

            // 等待完成信号
            int res = AwaitCompleted();
            return res == 0;
        }
        #endregion
        #region 光源控制模块
        /*
         * 光源控制模块
         * 寄存器地址|作用信息
         * V225.6|上面光源控制信号
         * V225.7|下面光源控制信号
         */

        public bool TurnOnTheUpperLightSource()
        {
            // 判断连接是否连接
            bool bRet = false;
            var value = helper?.ReadSingleRegister(12,1);
            // 判断是否读取到数据
            if (value == null) return false;
            ushort data12 = value.Value;
            data12 |= (ushort)(1 << 6);
            bRet = helper?.WriteSingleRegister(data12, 12) ?? false;
            if (bRet == false) return false;
            return true;
        }

        public bool TurnOffTheUpperLightSource()
        {
            var value = helper?.ReadSingleRegister(12,1);
            if (value == null) return false;
            ushort data12 = value.Value;
            data12 &= (ushort)(~(1 << 6) & 0xFFFF);
            bool bRet =helper?.WriteSingleRegister(data12, 12)?? false;
            if (bRet == false) return false;
            return true;
        }

        public bool TurnOnTheLowerLightSource()
        {
            var value = helper?.ReadSingleRegister(12,1);
            if (value == null) return false;
            ushort data12 = value.Value;
            data12 |= (ushort)(1 << 7);
            bool bRet = helper?.WriteSingleRegister(data12, 12) ?? false;
            if (bRet == false) return false;
            return true;
        }

        public bool TurnOffTheLowerLightSource()
        {
            var value = helper?.ReadSingleRegister(12,1);
            if (value == null) return false;
            ushort data12 = value.Value;
            data12 &= (ushort)(~(1 << 7) & 0xFFFF);
            bool bRet = helper?.WriteSingleRegister(data12, 12) ?? false;
            if (bRet == false) return false;
            return true;
        }
        #endregion
        #region 气缸模块
        /*
         * 光源侧气缸
         * 寄存器地址|作用信息
         * V217.0|抬起气缸升起
         * V217.1|抬起气缸下降
         * V217.2|来料侧气缸伸出
         * V217.3|来料侧气缸缩回
         * V217.4|光源侧气缸伸出
         * V217.5|光源侧气缸缩回
         */
        /// <summary>
        /// 控制气缸伸出
        /// </summary>
        /// <param name="id">id=0|木板抬起、id=2|光源侧气缸、id=1|来料侧气缸</param>
        /// <returns></returns>
        public bool ExtendCylinder(int id)
        {
            var values = helper?.ReadSingleRegister(12, 1);
            if (values == null) return false;
            ushort data = values.Value;
            switch (id)
            {
                case 0:
                    data |= (ushort)(1 << 0);
                    break;
                case 1:
                    data |= (ushort)(1 << 2);
                    break;
                case 2:
                    data |= (ushort)(1 << 4);
                    break;
                default: break;
            }
            bool bRet =helper?.WriteSingleRegister(data,12)?? false;
            if (bRet == false) return false;
            int ret = AwaitCompleted();
            return ret == 0;

        }

        /*
         * 光源侧气缸
         * 寄存器地址|作用信息
         * V225.0|抬起气缸升起
         * V225.1|抬起气缸下降
         * V225.2|来料侧气缸伸出
         * V225.3|来料侧气缸缩回
         * V225.4|光源侧气缸伸出
         * V225.5|光源侧气缸缩回
         */
        /// <summary>
        /// 控制气缸缩回
        /// </summary>
        /// <param name="id">id=0|木板抬起、id=1|光源侧气缸、id=2|来料侧气缸</param>
        /// <returns></returns>
        public bool RetractCylinder(int id)
        {
            var values = helper?.ReadSingleRegister(12,1);
            if(values==null)
                return false;
            ushort data = values.Value;
            switch (id)
            {
                case 0:
                    data |= (ushort)(1 << 1);
                    break;
                case 1:
                    data |= (ushort)(1 << 3);
                    break;
                case 2:
                    data |= (ushort)(1 << 5);
                    break;
                default: break;
            }
            bool bRet = helper?.WriteSingleRegister(data, 12) ?? false;
            if(bRet == false) return false;
            int ret = AwaitCompleted();
            return ret == 0;
        }
        #endregion

        /// <summary>
        /// 设置成手动模式
        /// </summary>
        /// <param name="manualModel"></param>
        /// <returns></returns>
        public bool SetManualModel()
        {
            return helper?.WriteSingleRegister(0, 5)?? false;
        }

        #region 回零
        /*
        * 光源侧气缸
        * 寄存器地址|作用信息
        * V242.0|上位机发出回零命令
        * V233.0|下位机通知上位机回零执行完毕
        */
        /// <summary>
        /// 读取回零状态
        /// </summary>
        /// <returns></returns>
        public bool ReadReturnZeroStatus()
        {
            var values = helper?.ReadSingleRegister(16,1);
            if (values != null)
            {
                ushort status = values.Value;
                status &= (ushort)1;
                if (status == 1)
                    return true;
                else
                    return false;
            }
            else
            {
                PrintError("读取回零状态时发生错误！");
                return false;
            }
        }

        /// <summary>
        /// 回零功能
        /// </summary>
        /// <returns></returns>
        public bool ReturnZero()
        {
            if (ReturnningZero) return false;
            var registers = helper?.ReadSingleRegister(21, 1);
            if (registers == null)
                return false;
            ushort data = registers.Value;
            data |= (ushort)(1 << 8);// V242.0 | 上位机发出回零命令
            return helper?.WriteSingleRegister(data, 21) ?? false;
            
        }
        #endregion

        #region 报警器模块
        /*
         * 24V声光报警器控制模块
         * 寄存器地址|作用信息
         * Q2.5|Q2.5=1声光报警器报警，Q2.5=0关闭声光报警器；
         */
        public bool Alarm()
        {
            return helper?.WriteSingleCoil(true, 25)?? false;
        }
        public bool DisAlarm()
        {
            return helper?.WriteSingleCoil(true, 25)?? false;
        }
        #endregion

        public bool MoveToPositionOne(WoodInfo wood)
        {
            if(EmergencyStopSignal==true) return false;
            if (ReturnningZero == true) return false;
            if(wood.Width==0)return false;
            //寻找木板中线位置
            bool bRet = true;
            ushort moveLength = 0;
            string msg = "执行移动到任务点One时发生错误：";

            //寻找中位线
            try
            {
                moveLength = (ushort)(wood.MiddleOffeset * Rate);
                bRet = SlidingTableMotorMovement(moveLength, 20000, true, true, true);
                if (!bRet)
                {
                    PrintError($"{msg}滑台电机移动失败！");
                    return false;
                }
            }
            catch (OverflowException ex)
            {
                PrintError($"{msg}计算移动脉冲:{160}*{Rate}时发生值溢出问题:{ex.Message}");
                return false;
            }

            //翻转电机固定中线
            bRet = SetFlipMotor(14000, 7000, true, true, true);
            if (!bRet)
            {
                PrintError($"{msg}翻转电机发生错误！");
                return false;
            }

            // 抬起木板
            bRet = ExtendCylinder(0);
            if (!bRet)
            {
                PrintError($"{msg}抬起木板发生错误");
                return false;
            }

            //翻转电机放平开始拍照
            bRet = SetFlipMotor(14000, 7000, false, true, true);
            if (!bRet)
            {
                PrintError($"{msg}翻转电机放平开始拍照,发生错误");
                return false;
            }
            UpdatePointHandler?.Invoke(this, Position.One);
            return true;
        }

        public bool MoveToPositionTwo(WoodInfo wood)
        {
            if (EmergencyStopSignal == true) return false;
            if (ReturnningZero == true) return false;
            if(wood.Width==0)return false;
            bool bRet = true;
            ushort moveLength = 0;
            string msg = "执行移动到任务点Two时发生错误：";


            

            //移动到拍照位置
            try
            {
                //翻转电机固定中线
                bRet = SetFlipMotor(14000, 7000, true, true, true);
                if (bRet==false)
                {
                    PrintError($"{msg}翻转电机固定中线时发生错误");
                    return false;
                }
                moveLength = (ushort)(wood.Width * Rate);
                bRet = SlidingTableMotorMovement(moveLength, 20000, true, true, true);
                if (bRet==false)
                {
                    PrintError($"{msg}移动到拍照位置时发生错误");
                    return false;
                }
                //翻转电机放平开始拍照
                bRet = SetFlipMotor(14000, 7000, false, true, true);
                if (bRet == false)
                {
                    PrintError($"{msg}翻转电机放平开始拍照时发生错误");
                    return false;
                }
                UpdatePointHandler?.Invoke(this, Position.Two);
                return true;
            }
            catch (OverflowException ex)
            {
                PrintError($"计算移动脉冲:{340}*{Rate}时发生值溢出问题:{ex.Message}");
                return false;
            }

            
        }

        public bool MoveToPositionThree()
        {
            if (EmergencyStopSignal == true) return false;
            if (ReturnningZero == true) return false;
            bool bRet = true;
            string msg = "执行移动到任务点Three时发生错误：";

            //翻转电机固定中线
            bRet = SetFlipMotor(14000, 7000, true, true, true);
            if (!bRet)
            {
                PrintError($"{msg}翻转电机固定中线时发生错误");
                return false;
            }

            //来料侧气缸伸出
            bRet = ExtendCylinder(1);
            if (!bRet)
            {
                PrintError($"{msg}来料侧气缸伸出时发生错误");
                return false;
            }

            //来料侧气缸缩回
            bRet = RetractCylinder(1);
            if (!bRet)
            {
                PrintError($"{msg}来料侧气缸缩回时发生错误");
                return false;
            }

            //翻转电机放平开始拍照
            bRet = SetFlipMotor(14000, 7000, false, true, true);
            if (!bRet)
            {
                PrintError($"{msg}翻转电机放平开始拍照时发生错误");
                return false;
            }
            UpdatePointHandler?.Invoke(this, Position.Three);
            return true;
        }

        public bool MoveToPositionFour(WoodInfo wood)
        {
            if (EmergencyStopSignal == true) return false;
            if(wood.Width==0)return false;
            if (ReturnningZero == true) return false;
            bool bRet = true;
            string msg = "执行移动到任务点Four时发生错误：";

            //翻转电机固定中线
            bRet = SetFlipMotor(14000, 7000, true, true, true);
            if (!bRet)
            {
                PrintError($"{msg}翻转电机固定中线时发生错误");
                return false;
            }

            //光源测气缸伸出
            bRet = ExtendCylinder(2);
            if (!bRet)
            {
                PrintError($"{msg}光源测气缸伸出时发生错误");
                return false;
            }

            //光源测气缸缩回
            bRet = RetractCylinder(2);
            if (!bRet)
            {
                PrintError($"{msg}光源测气缸缩回时发生错误");
                return false;
            }

            //移动到拍照位置
            bRet = SlidingTableMotorMovement((ushort)(wood.Width*Rate),20000, direc: false, leftEnable: true, rightEnable: true);
            if (!bRet)
            {
                PrintError($"{msg}移动到拍照位置时发生错误");
                return false;
            }

            //翻转电机放平开始拍照
            bRet = SetFlipMotor(14000, 7000, false, true, true);
            if (!bRet)
            {
                PrintError($"{msg}翻转电机放平开始拍照时发生错误");
                return false;
            }
            UpdatePointHandler?.Invoke(this, Position.Four);
            return true;
        }

        bool listenning = false;
        DateTime dtLast = DateTime.Now;
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ListenSignal(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (listenning) return;
            listenning = true;
            try
            {
                bool bRet = Awake();
                if (bRet==false)
                {
                    Connect();
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }
            finally { listenning = false; }
        }

        #region 监听plc状态
        /*
         * 监听plc状态
         * 寄存器地址|作用信息
         * I0.0|检测木板来的传感器信号
         * I0.1|急停功能键
         * I0.2|回零功能键
         * I0.3|左侧滑台回零点传感器
         * I0.4|左侧滑台远端限位传感器
         * I0.5|右侧滑台回零点传感器
         * I0.6|右侧滑台远端限位传感器
         * I1.1|左侧翻转机构平放限位传感器
         * I1.3|右侧翻转机构平放限位传感器
         */
        public bool Awake()
        {
            ReturnningZero = ReadReturnZeroStatus();//回零信号
            var signal = helper?.ReadInputs(0, 3);
            if (signal != null)
            {
                EmergencyStopSignal = signal[1];//急停信号
                if (signal[0] && !WaitingWood)//检测到信号
                {
                    WaitingWood = true;
                }
                if (!signal[0] && WaitingWood)
                    WaitingWood = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }



    internal enum Position
    {
        None = 0,
        One=1,
        Two=2,
        Three=3,
        Four=4
    }
}
