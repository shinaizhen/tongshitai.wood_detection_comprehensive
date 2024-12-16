using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace Test.PythonTest
{
    internal static class RunPythonModel
    {
        #region 共享内存配置
        const long IMAGE_DATA_SIZE = 3684 * 5472 * 10 + 16;// 图像数据大小（+4字节的长度信息）
        const string IMAGE_NAME = "image";// 内存空间名称
        const string IMAGE_RESULT_NAME = "image_result";
        static readonly MemoryMappedFile imgMMF = MemoryMappedFile.CreateOrOpen(IMAGE_NAME, IMAGE_DATA_SIZE);
        static readonly MemoryMappedFile imgResultMMF = MemoryMappedFile.CreateOrOpen(IMAGE_RESULT_NAME, IMAGE_DATA_SIZE);

        const long CMD_CAPACITY = 4;// 命令大小（4字节）
        const string CMD_NAME = "cmd";// 内存空间名称
        static readonly MemoryMappedFile cmdMMF = MemoryMappedFile.CreateOrOpen(CMD_NAME, CMD_CAPACITY);

        const long DATA_CAPACITY = 1024 * 1024 * 10;// 检测数据大小（+4字节的长度信息）
        const string DATA_NAME = "result";// 内存空间名称
        static readonly MemoryMappedFile dataMMF = MemoryMappedFile.CreateOrOpen(DATA_NAME, DATA_CAPACITY);
        #endregion


        #region 内存映射文件操作
        /// <summary>
        /// 写入命令
        /// </summary>
        /// <param name="cmd"></param>
        public static void WriteCmd(CommandType cmdType)
        {
            try
            {
                using var cmdAccesor = cmdMMF.CreateViewAccessor(0, CMD_CAPACITY, MemoryMappedFileAccess.Write);
                cmdAccesor.Write(0, (int)cmdType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入命令时发生错误{ex.Message}");
            }
        }

        /// <summary>
        /// 读取状态命令
        /// </summary>
        /// <returns></returns>
        public static CommandType? ReadCmd()
        {
            try
            {
                using var cmdAccesor = cmdMMF.CreateViewAccessor(0, CMD_CAPACITY, MemoryMappedFileAccess.Read);
                return (CommandType)cmdAccesor.ReadInt32(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取命令时发生错误{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 写入图像数据
        /// </summary>
        /// <param name="imageData">数据格式为：图像长度（4字节）+图像高度（4字节）+图像宽度（4字节）+图像通道数（4字节）+图像数据（长度字节）</param>
        public static void WriteImageData(ImageData imageData)
        {
            try
            {
                // 创建访问器
                using var dataAccessor = imgMMF.CreateViewAccessor(0, IMAGE_DATA_SIZE, MemoryMappedFileAccess.Write);
                // 写入长度
                dataAccessor.Write(0, imageData.Image.Length);
                dataAccessor.Write(4, imageData.Height);
                dataAccessor.Write(8, imageData.Width);
                dataAccessor.Write(12, imageData.Channels);
                // 写入图像数据
                //写入图像数据
                dataAccessor.WriteArray<byte>(16, imageData.Image, 0, imageData.Image.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入数据时发生错误{ex.Message}");
                return;
            }

        }

        /// <summary>
        /// 读取图像数据
        /// <para>返回数据格式为：图像长度（4字节）+图像高度（4字节）+图像宽度（4字节）+图像通道数（4字节）+图像数据（长度字节）</para>
        /// </summary>
        /// <returns></returns>
        public static ImageData? ReadImageData()
        {
            try
            {
                using var dataAccesor = imgResultMMF.CreateViewAccessor(0, IMAGE_DATA_SIZE, MemoryMappedFileAccess.Read);
                int imageLength = dataAccesor.ReadInt32(0);
                int height = dataAccesor.ReadInt32(4);
                int width = dataAccesor.ReadInt32(8);
                int channels = dataAccesor.ReadInt32(12);
                byte[] image = new byte[imageLength];
                dataAccesor.ReadArray(16, image, 0, imageLength);

                return new ImageData(DetectionService.DetectionFlag.None, Guid.Empty, image, width, height, channels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取数据时发生错误{ex.Message}");
                return null;
            }
        }

        public static void WriteDetectionData(byte[] bytes)
        {
            try
            {
                // 创建访问器
                using var dataAccessor = dataMMF.CreateViewAccessor(0, DATA_CAPACITY, MemoryMappedFileAccess.Write);
                // 写入长度
                int len = bytes.Length;
                dataAccessor.Write(0, len);
                //写入检测数据
                dataAccessor.WriteArray<byte>(4, bytes, 0, len);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入检测数据时发生错误{ex.Message}");
                return;
            }
        }

        public static byte[]? ReadDetectionData()
        {
            try
            {
                using var dataAccesor = dataMMF.CreateViewAccessor(0, DATA_CAPACITY, MemoryMappedFileAccess.Read);
                int length = dataAccesor.ReadInt32(0);
                byte[] bytes = new byte[length];
                dataAccesor.ReadArray(4, bytes, 0, length);
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取检测数据时发生错误{ex.Message}");
                return null;
            }
        }
        #endregion

    }

    // 定义命令类型枚举
    public enum CommandType
    {
        None=0,//无
        Completed=1,// 任务完成
        Running=2,//正在执行
        ProcessImage=3,//检测图像
        ChangeModel=4,//更改检测模型
        Exit=5,//退出程序
        Error=6,//发生错误
    }
}
