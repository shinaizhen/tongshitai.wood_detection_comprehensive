using Test.PythonTest;
using OpenCvSharp;
using System.Text;
using System.Text.Json;

//Console.WriteLine("读取图像...");
//string img_path = @"D:\tongshitai\通世泰木板全面瑕疵检测\wood_detection\datasets\test001.png";
//var src = Cv2.ImRead(img_path, ImreadModes.Color);

//Cv2.NamedWindow("image", WindowFlags.Normal);
//Cv2.ResizeWindow("image", 1080, 1080);
////Cv2.ImShow("image", src);

////Cv2.WaitKey(0);
////Cv2.DestroyAllWindows();
//Console.WriteLine("写入图像数据");
//byte[] img = src.ToBytes(".png");
//RunPythonModel.WriteImageData(new ImageData(DetectionService.DetectionFlag.A,new Guid(),img,src.Width,src.Height,src.Channels()));
//Console.WriteLine("开始执行命令");
//RunPythonModel.WriteCmd(CommandType.ProcessImage);
//while (true)
//{

//    var cmd = RunPythonModel.ReadCmd();
//    if (cmd == null)
//        continue;
//    Console.WriteLine($"读取命令为：{cmd.ToString()}");
//    if (cmd == CommandType.Completed)
//        break;
//    if(cmd==CommandType.Error)
//    {
//        Console.WriteLine("发生错误！");
//        Environment.Exit(-1);
//    }
//    Thread.Sleep(500);
//}

//Console.WriteLine("开始读取检测结果....");
//Console.WriteLine("读取检测结果数据");
//var results_bytes = RunPythonModel.ReadDetectionData();
//if (results_bytes != null && results_bytes.Length > 0)
//{
//    Console.WriteLine("检测到目标");
//    var jsonStr = Encoding.UTF8.GetString(results_bytes);
//    var result = JsonSerializer.Deserialize<List<DetectionItem>>(jsonStr);
//    if (result != null)
//    {
//        foreach(var item in result)
//        {
//            Console.WriteLine($"名称：{item.Name},置信度：{item.Confidence}");
//        }
//    }

//}

//Console.WriteLine("读取检测图像");
//var result_img = RunPythonModel.ReadImageData();

//if (result_img != null)
//{
//    var dist = Cv2.ImDecode(result_img.Image, ImreadModes.Color);
//    Cv2.ImShow("image", dist);
//}
//else
//{
//    Console.WriteLine("图像为空！");
//}


//Cv2.WaitKey(0);
//Cv2.DestroyAllWindows();
//RunPythonModel.WriteCmd(CommandType.ProcessImage);

var imgOne = new ImageData(DetectionService.DetectionFlag.A, new Guid(),new byte[0], 100, 200, 3);
var resultOne = new DetectionResult(imgOne.TaskFlag, imgOne.TaskId, imgOne);
resultOne.Results.Add(new DetectionItem(0,"asdfa",0.26f,new BoundingBox(100,200,300,400)));
resultOne.Results.Add(new DetectionItem(0,"asdfa",0.26f,new BoundingBox(100,200,300,400)));
resultOne.Results.Add(new DetectionItem(0,"asdfa",0.26f,new BoundingBox(100,200,300,400)));
string path = @"C:\Users\Administrator\Desktop\test_data.json";
resultOne.Save(path);
Console.ReadLine();