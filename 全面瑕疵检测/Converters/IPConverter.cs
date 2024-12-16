using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 全面瑕疵检测.Converters
{
    internal class IPConverter
    {
        public static string Uint2String(uint ipAsUint)
        {
            byte[] bytes = BitConverter.GetBytes(ipAsUint);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            string ipAddress = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
            return ipAddress;
        }

        public static uint String2Uint(string ipAddress)
        {
            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4)
            {
                throw new ArgumentException("Invalid IP address format.");
            }

            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = byte.Parse(parts[i]);
            }
            uint ipAsUint = BitConverter.ToUInt32(bytes, 0);
            return ipAsUint;
        }

    }
}
