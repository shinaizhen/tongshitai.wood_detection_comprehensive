using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace 全面瑕疵检测.Views
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : Window
    {
        double scale = 1.0;
        Point orignPoint;
        Point startPoint;
        TransformGroup transformGroup;
        public ImageView(BitmapImage image)
        {
            InitializeComponent();
            // 使用矩阵变换来进行图形的变换操作，用于调整图像的位置、大小、旋转等。
            imgCtl.RenderTransform = new MatrixTransform();
            // 以图像中心为变换原点
            imgCtl.RenderTransformOrigin = new Point(0.5, 0.5);

            // 订阅鼠标滚轮事件
            grid.MouseWheel += ContentControl_MouseWheel;

            // 订阅鼠标按下事件
            imgCtl.MouseLeftButtonDown += Image_MouseLeftButtonDown;

            // 订阅鼠标移动事件
            imgCtl.MouseMove += Image_MouseMove;

            // 订阅鼠标释放事件
            imgCtl.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform());
            transformGroup.Children.Add(new TranslateTransform());
            imgCtl.RenderTransform = transformGroup;
            imgCtl.Source = image;
        }

       

        #region 鼠标拖动功能
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(grid);
            imgCtl.CaptureMouse();
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imgCtl.ReleaseMouseCapture();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgCtl.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(grid);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;
                TranslateTransform translateTransform = (TranslateTransform)transformGroup.Children[1];
                translateTransform.X += offsetX;
                translateTransform.Y += offsetY;
                startPoint = currentPoint;
            }
        }
        #endregion
        #region 缩放功能
        private void ContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 获取鼠标在容器控件上的位置
            Point mousePosition = e.GetPosition(imgCtl);
            
            // 计算缩放比例
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            scale *= zoomFactor;

            // 计算移动距离
            double offsetX=(mousePosition.X - orignPoint.X)*(1-zoomFactor);
            double offsetY = (mousePosition.Y - orignPoint.Y) * (1 - zoomFactor);

            // 更新原点位置
            orignPoint = mousePosition;
            TranslateTransform translateTransform = (TranslateTransform)transformGroup.Children[1];
            ScaleTransform scaleTransform = (ScaleTransform)transformGroup.Children[0];
            // 更新平移变换
            translateTransform.X += offsetX;
            translateTransform.Y += offsetY;

            // 更新缩放变换
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
        }
        #endregion

        private void btnInitSize_Click(object sender, RoutedEventArgs e)
        {
            ScaleTransform scaleTransform = (ScaleTransform)transformGroup.Children[0];
            TranslateTransform translateTransform = (TranslateTransform)transformGroup.Children[1];
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY=1;
            scale = 1;
            translateTransform.X = 0;
            translateTransform.Y = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imgCtl.Source = null;
        }
    }
}
