using System.Net.NetworkInformation;

namespace 全面瑕疵检测.Common
{
    internal static class CheckIP
    {
        public static bool PingIP(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ipAddress);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"Ping操作出现异常: {ex.Message}");
                return false;
            }
        }
    }
}
