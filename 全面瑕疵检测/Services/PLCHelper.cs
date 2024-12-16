using NModbus;
using System.Net;
using System.Net.Sockets;

namespace 全面瑕疵检测.Services
{
    internal class PLCHelper:IDisposable
    {
        public EventHandler<bool>? ConnectedChanged { get; set; }
        IModbusMaster? master {  get; set; }
        TcpClient? client { get; set; }
        Mutex mutex { get; set; }
        public string IP {  get;private set; }
        public int Port { get;private set; }
        public PLCHelper(string ip,int port) 
        {
            IP = ip;
            Port = port;
            mutex = new Mutex(false);
        }

        private bool connected;

        public bool Connected
        {
            get { return connected; }
            set 
            {
                if(connected != value)
                {
                    connected = value;
                    ConnectedChanged?.Invoke(this, value);
                    GlobalStatus.UpdatePLCStatus(value);
                }
            }
        }


        public bool Connect()
        {
            try
            {
                mutex.WaitOne();
                master?.Dispose();
                client?.Dispose();
                client = new TcpClient();
                client.Connect(IPAddress.Parse(IP), Port);
                var factory = new ModbusFactory();
                master = factory.CreateMaster(client);
                master.Transport.Retries = 0;   //don't have to do retries||不需要重试
                master.Transport.ReadTimeout = 3000;
                Connected = true;
                return true;
            }
            catch (Exception ex)
            {
                Connected = false;
                NlogService.Error($"plc连接时发生错误：{ex.Message}");
                return false;
            }
            finally {  mutex.ReleaseMutex(); }
        }

        public void DisConnect()
        {
            mutex.WaitOne();
            master?.Dispose();
            client?.Dispose();
            Connected = false;
            mutex.ReleaseMutex();
        }
        #region 线圈操作
        public bool WriteCoils(bool[] coils,ushort startAddress,byte slaveAddress=1)
        {
            if (Connected==false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return false;
            }
            try
            {
                mutex.WaitOne();
                master.WriteMultipleCoils(slaveAddress: slaveAddress, startAddress, coils);
                return true;
            }catch(Exception ex) 
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return false;
            }
            finally { mutex.ReleaseMutex(); }
        }

        public bool WriteSingleCoil(bool coil,ushort startAddress,byte slaveAddress = 1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return false;
            }
            try
            {
                mutex.WaitOne();
                master.WriteSingleCoil(slaveAddress: slaveAddress, startAddress, coil);
                return true;
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return false;
            }finally { mutex.ReleaseMutex(); }
        }

        public bool[]? ReadCoils(ushort startAddress,ushort length,byte slaveAddress=1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return null;
            }
            try
            {
                mutex.WaitOne();
                var coils = master.ReadCoils(slaveAddress: slaveAddress, startAddress, length);
                return coils;
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return null;
            }
            finally { mutex.ReleaseMutex(); }
        }
        public bool? ReadSingleCoil(ushort startAddress, byte slaveAddress = 1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return null;
            }
            try
            {
                mutex.WaitOne();
                var coils = master.ReadCoils(slaveAddress: slaveAddress, startAddress, 1);
                if(coils == null)
                    return null;
                return coils[0];
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return null;
            }finally { mutex.ReleaseMutex(); }
        }

        #endregion

        #region 保持寄存器操作
        public bool WriteRegisters(ushort[] registers, ushort startAddress, byte slaveAddress = 1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return false;
            }
            try
            {
                mutex.WaitOne();
                master.WriteMultipleRegisters(slaveAddress: slaveAddress, startAddress, registers);
                return true;
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return false;
            }
            finally { mutex.ReleaseMutex(); }
        }

        public bool WriteSingleRegister(ushort register, ushort startAddress, byte slaveAddress = 1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return false;
            }
            try
            {
                mutex.WaitOne();
                master.WriteSingleRegister(slaveAddress: slaveAddress, startAddress, register);
                return true;
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return false;
            }finally { mutex.ReleaseMutex(); }
        }

        public ushort[]? ReadRegisters(ushort startAddress, ushort length, byte slaveAddress = 1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return null;
            }
            try
            {
                mutex.WaitOne();
                var registers = master.ReadHoldingRegisters(slaveAddress: slaveAddress, startAddress, length);
                return registers;
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return null;
            }
            finally { mutex.ReleaseMutex(); }
        }
        public ushort? ReadSingleRegister(ushort startAddress, byte slaveAddress = 1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return null;
            }
            try
            {
                mutex.WaitOne();
                var registers = master.ReadHoldingRegisters(slaveAddress: slaveAddress, startAddress, 1);
                if (registers == null)
                    return null;
                return registers[0];
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return null;
            }
            finally { mutex.ReleaseMutex(); }
        }

        public void Dispose()
        {
            ConnectedChanged = null;
            DisConnect();
            mutex?.Dispose();
        }

        #endregion

        #region 输入寄存器
        public bool[]? ReadInputs(ushort startAddress, ushort length,byte slaveAddress=1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return null;
            }
            try
            {
                mutex.WaitOne();
                var inputs = master.ReadInputs(slaveAddress: slaveAddress, startAddress, numberOfPoints:length);
                return inputs;
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return null;
            }
            finally { mutex.ReleaseMutex(); }
        }

        public bool? ReadSingleInput(ushort startAddress, byte slaveAddress=1)
        {
            if (Connected == false || master == null)
            {
                NlogService.Error("plc无连接或master对象为null");
                Connected = false;
                return null;
            }
            try
            {
                mutex.WaitOne();
                var inputs = master.ReadInputs(slaveAddress: slaveAddress,startAddress:startAddress,numberOfPoints:1);
                if (inputs == null)
                    return null;
                return inputs[0];
            }
            catch (Exception ex)
            {
                NlogService.Error($"plc通讯时发生错误：{ex.Message}");
                Connected = false;
                return null;
            }
            finally { mutex.ReleaseMutex(); }
        }
        #endregion
    }
}
