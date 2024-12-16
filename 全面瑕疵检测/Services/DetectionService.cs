using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Media.Imaging;
using 全面瑕疵检测.Models;

namespace 全面瑕疵检测.Services
{
    internal class DetectionService
    {
        /// <summary>
        /// 检测任务标签
        /// </summary>
        internal enum DetectionFlag
        {
            None,
            A,
            B,
            C,
            D,
            E,
            F
        }
        public static Queue<ImageData> taskQuenue = new Queue<ImageData>();

        /// <summary>
        /// 获取检测任务
        /// </summary>
        /// <returns></returns>
        public static ImageData? GetDetectionTask()
        {

            if (taskQuenue.Count > 0)
            {
                return taskQuenue.Dequeue();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 添加检测任务
        /// </summary>
        /// <param name="imageData"></param>
        public static void AddDetectionTask(ImageData imageData)
        {
            taskQuenue.Enqueue(imageData);
        }

        /// <summary>
        /// 执行检测任务
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public static DetectionResult PerformDetection(ImageData imageData)
        {
            // 写入图像数据
            RunPythonModel.WriteImageData(imageData);
            DateTime startTime = new DateTime();
            while (true)// 等待20秒
            {
                var cmd = RunPythonModel.ReadCmd();
                if (cmd == null)
                    continue;
                if (cmd == CommandType.Completed)
                    break;
                if (TimeSpan.FromSeconds(20) < DateTime.Now - startTime)
                {
                    string error = $"任务ID：{imageData.TaskId}, 任务类型：{imageData.TaskFlag}，检测超时，请检查模型是否正常运行。";
                    NlogService.Error(error);
                    return new DetectionResult(imageData.TaskFlag, imageData.TaskId, error);// 检测任务失败
                }
            }
            var resultData = RunPythonModel.ReadDetectionData();

            var image = RunPythonModel.ReadImageData();

            if (image == null)
            {
                string error = $"任务ID：{imageData.TaskId}, 任务类型：{imageData.TaskFlag}，检测失败，无法获取图像数据。";
                return new DetectionResult(imageData.TaskFlag, imageData.TaskId, error);// 检测任务失败
            }

            DetectionResult detectionResult = new DetectionResult(imageData.TaskFlag, imageData.TaskId, image);


            // 解析检测结果
            if (resultData == null)
            {
                detectionResult.IsOk = true;
            }
            else
            {
                try
                {
                    var results = JsonSerializer.Deserialize<List<DetectionItem>>(Encoding.UTF8.GetString(resultData));
                    if (results == null)
                        detectionResult.IsOk = true;
                    else
                    {
                        detectionResult.IsOk = false;
                        detectionResult.Results = results;
                    }
                }
                catch (Exception ex)
                {
                    string error = $"任务ID：{imageData.TaskId}, 任务类型：{imageData.TaskFlag}，解析检测结果失败，错误信息：{ex.Message}";
                    NlogService.Error(error);
                }
            }
            return detectionResult;
        }
    }

    internal class ImageData
    {
        /// <summary>
        /// 任务标签
        /// </summary>
        public DetectionService.DetectionFlag TaskFlag { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public long TaskId { get; set; }
        /// <summary>
        /// 图像数据
        /// </summary>
        public byte[] Image { get; set; }
        /// <summary>
        /// 图像宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        public string? SavePath { get;set; }

        public int Channels { get; set; }

        /// <summary>
        /// 图像高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskFlag"></param>
        /// <param name="taskId"></param>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ImageData(DetectionService.DetectionFlag taskFlag, int taskId, byte[] image, int width, int height, int channels)
        {
            TaskFlag = taskFlag;
            TaskId = taskId;
            Image = image;
            Width = width;
            Height = height;
            Channels = channels;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskFlag"></param>
        /// <param name="taskId"></param>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ImageData(DetectionService.DetectionFlag taskFlag, Guid taskId, byte[] image, int width, int height, int channels)
        {
            TaskFlag = taskFlag;
            Image = image;
            Width = width;
            Height = height;
            Channels = channels;
            byte[] guidBytes = taskId.ToByteArray();
            TaskId = BitConverter.ToInt64(guidBytes, 0);
        }
    }

    internal class DetectionItem
    {
        /// <summary>
        /// 检测目标的名称或类型
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 检测目标的置信度
        /// </summary>
        public float Confidence { get; set; }

        public int ID { get; set; }

        /// <summary>
        /// 检测目标的位置信息（您可以根据需要定义，例如边界框）
        /// </summary>
        public BoundingBox Box { get; set; }

        public DetectionItem(int id, string name, float confidence, BoundingBox box)
        {
            ID = id;
            Name = name;
            Confidence = confidence;
            Box = box;
        }
    }

    internal class BoundingBox
    {
        /// <summary>
        /// 左上角X坐标
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// 左上角Y坐标
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }

        public BoundingBox(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    internal class DetectionResult
    {
        /// <summary>
        /// 任务类型
        /// </summary>
        public DetectionService.DetectionFlag TaskType { get; set; }

        /// <summary>
        /// 任务 ID，保持与检测时一致
        /// </summary>
        public long TaskId { get; set; }
        
        public string? ImagePath {  get; set; }

        /// <summary>
        /// 检测结果，您可以根据需要定义具体的类型
        /// </summary>
        public List<DetectionItem> Results { get; set; }

        /// <summary>
        /// 操作状态，例如成功或失败
        /// </summary>
        public string Status { get; set; }

        public ImageData? Image { get; set; }

        public bool IsOk = false;// 是否检测到瑕疵

        /// <summary>
        /// 错误信息，若有
        /// </summary>
        public string? ErrorMessage { get; set; }

        public const string STATUS_SUCCESS = "Success";
        public const string STATUS_FAILURE = "Failure";

        /// <summary>
        /// 检测成功时调用该构造函数
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="taskId"></param>
        /// <param name="image"></param>
        public DetectionResult(DetectionService.DetectionFlag taskType, long taskId,ImageData image)
        {
            TaskType = taskType;
            TaskId = taskId;
            Results = new List<DetectionItem>();
            Status = DetectionResult.STATUS_SUCCESS; // 默认状态为成功
            Image = image;
        }

        /// <summary>
        /// 检测失败时调用该构造函数
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="taskId"></param>
        /// <param name="errorMessage"></param>
        public DetectionResult(DetectionService.DetectionFlag taskType, long taskId,string errorMessage)
        {
            TaskType = taskType;
            TaskId = taskId;
            Results = new List<DetectionItem>();
            Status = DetectionResult.STATUS_FAILURE; 
            ErrorMessage = errorMessage;
            Image = null;
        }

        public BitmapImage? ToBitmapImage()
        {
            if (Image == null)
                return null;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(Image.Image);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public void Save(string filePath)
        {
            // 判断文件是否存在，存在则以追加模式打开，不存在则创建新文件
            FileMode fileMode = File.Exists(filePath) ? FileMode.Append : FileMode.Create;

            using FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using Utf8JsonWriter writer = new Utf8JsonWriter(fs, new JsonWriterOptions() { Indented = true });
            // 根据文件模式判断是否需要先写入数组开始符号
            if (fileMode == FileMode.Create)
            {
                // 如果是新建文件，先写入JSON数组开始符号
                byte[] arrayStartBytes = Encoding.UTF8.GetBytes("[");
                fs.Write(arrayStartBytes, 0, arrayStartBytes.Length);
            }
            else if (fs.Length > 0)
            {
                // 如果是追加模式，需要先删除文件末尾多余的结束符号（如果有）
                fs.Seek(-1, SeekOrigin.End);
                byte[] lastByte = new byte[1];
                fs.Read(lastByte, 0, 1);
                if (lastByte[0] == (byte)']')
                {
                    fs.Seek(-1, SeekOrigin.End);
                    byte[] newEndBytes = Encoding.UTF8.GetBytes(",");
                    fs.Write(newEndBytes, 0, newEndBytes.Length);
                }

            }
            else
            {
                writer.WriteStartArray();
            }
            fs.Flush();
            writer.WriteStartObject();
            writer.WriteString("TaskType", TaskType.ToString());
            writer.WriteString("ImagePath", Image.SavePath);
            writer.WriteStartArray("Results");
            foreach (DetectionItem item in Results)
            {
                writer.WriteStartObject();
                writer.WriteNumber("ID", item.ID);
                writer.WriteString("Name", item.Name);
                // 写入bbox
                writer.WriteNumber("X", item.Box.X);
                writer.WriteNumber("Y", item.Box.Y);
                writer.WriteNumber("Width", item.Box.Width);
                writer.WriteNumber("Height", item.Box.Height);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();

            if (fs.Length > 0)
            {
                fs.Seek(0, SeekOrigin.End);
                byte[] newEndBytes01 = Encoding.UTF8.GetBytes("]");
                fs.Write(newEndBytes01, 0, newEndBytes01.Length);
                fs.Flush();
            }

            //fs.Flush();

        }
    }
}
