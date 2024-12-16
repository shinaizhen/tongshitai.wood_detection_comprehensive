using NLog;

namespace 全面瑕疵检测.Services
{
    internal static class NlogService
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        #region Debug，调试
        public static void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public static void Debug(string msg, Exception err)
        {
            _logger.Debug(err, msg);
        }
        #endregion

        #region Info，信息
        public static void Info(string msg)
        {
            _logger.Info(msg);
        }

        public static void Info(string msg, Exception err)
        {
            _logger.Info(err, msg);
        }
        #endregion

        #region Warn，警告
        public static void Warn(string msg)
        {
            _logger.Warn(msg);
        }

        public static void Warn(string msg, Exception err)
        {
            _logger.Warn(err, msg);
        }
        #endregion

        #region Trace，追踪
        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }

        public static void Trace(string msg, Exception err)
        {
            _logger.Trace(err, msg);
        }
        #endregion

        #region Error，错误
        public static void Error(string msg)
        {
            _logger.Error(msg);
        }

        public static void Error(string msg, Exception err)
        {
            _logger.Error(err, msg);
        }
        #endregion

        #region Fatal,致命错误
        public static void Fatal(string msg)
        {
            _logger.Fatal(msg);
        }

        public static void Fatal(string msg, Exception err)
        {
            _logger.Fatal(err, msg);
        }
        #endregion
    }
}
