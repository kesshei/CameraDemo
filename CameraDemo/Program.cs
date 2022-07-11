using OpenCvSharp;
using System.Drawing;
using OpenCvSharp.Extensions;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using Point = OpenCvSharp.Point;
namespace CameraDemo
{
    internal class Program
    {
static void Main(string[] args)
{
    Mat frame = new Mat();
    var Capture = new VideoCapture("http://192.168.1.3:4747/video?1280x720"); // 这里是DroidCam手机端的IP地址和端口号 ?1280x720
    Console.WriteLine($"摄像头是否打开:{Capture.IsOpened()}");
    //声明窗口
    Cv2.NamedWindow("video", WindowFlags.KeepRatio);
    Cv2.ResizeWindow("video", 1280, 720);

    //加载人眼、人脸模型数据
    CascadeClassifier faceFinder = new CascadeClassifier(@"haarcascades\haarcascade_frontalface_default.xml");
    CascadeClassifier eyeFinder = new CascadeClassifier(@"haarcascades\haarcascade_eye_tree_eyeglasses.xml");
    var Count = new ConcurrentDictionary<long, int>();
    long PreviousTime = 0;
    long FPS = 0;
    while (true)
    {
        bool res = Capture.Read(frame);//会阻塞线程
        if (!res || frame.Empty())
        {
            continue;
        }
        var times = GettimeStamp();
        if (PreviousTime != times && Count.ContainsKey(times - 1))
        {
            PreviousTime = times;
            FPS = Count[times - 1];
            Console.WriteLine($"每秒处理图片:{FPS}帧");
        }
        Count.AddOrUpdate(times, 1, (k, v) => v + 1);

        Cv2.PutText(frame, $"{FPS}", new Point(10, 70), HersheyFonts.HersheyPlain, 4, new Scalar(0, 0, 255), 3);
        //进行检测识别
        Rect[] faceRects = faceFinder.DetectMultiScale(frame);
        Rect[] eyeRects = eyeFinder.DetectMultiScale(frame);
        //如果有检测到，就绘制结果到图像上
        if (faceRects.Length > 0)
        {
            Cv2.Rectangle(frame, faceRects[0], new Scalar(0, 0, 255), 3);
        }
        if (eyeRects.Length > 1)
        {
            Cv2.Rectangle(frame, eyeRects[0], new Scalar(255, 0, 0), 3);
            Cv2.Rectangle(frame, eyeRects[1], new Scalar(255, 0, 0), 3);
        }
        //显示结果
        Cv2.ImShow("video", frame);
        Cv2.WaitKey(1);
        //省略几个以加快图像的速度
        Capture.Grab();
        Capture.Grab();
        Capture.Grab();
        Capture.Grab();
    }
    Capture.Release();
    //销毁所有的窗口
    Cv2.DestroyAllWindows();
    Console.WriteLine("录制完毕!");
    Console.ReadLine();
}
public static long GettimeStamp()
{
    return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
}
    }
}