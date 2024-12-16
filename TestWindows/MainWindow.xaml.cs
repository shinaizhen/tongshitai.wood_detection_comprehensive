using System.IO;
using System.IO.MemoryMappedFiles;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        const string IMAGE_NAME = "image_result";
        const long IMAGE_SIZE = 3684 * 5472 * 10 + 16;
        static MemoryMappedFile imgMMF = MemoryMappedFile.CreateOrOpen(IMAGE_NAME,IMAGE_SIZE,MemoryMappedFileAccess.ReadWrite);
        private void btn_readImg_Click(object sender, RoutedEventArgs e)
        {
            this.img_ctl.Source = null;
            try
            {
                using MemoryMappedViewAccessor accessor = imgMMF.CreateViewAccessor(0,IMAGE_SIZE);
                // read img length
                //byte[] img_shape_data = new byte[16];
                //accessor.WriteArray(0,img_shape_data,0,16);
                //int length = BitConverter.ToInt32(img_shape_data, 0);
                //int height = BitConverter.ToInt32(img_shape_data, 4);
                //int width = BitConverter.ToInt32(img_shape_data, 8);
                //int channels = BitConverter.ToInt32(img_shape_data, 12);
                int length = accessor.ReadInt32(0);
                int width= accessor.ReadInt32(4);
                int height= accessor.ReadInt32(8);
                int channels= accessor.ReadInt32(12);
                
                byte[] img_data = new byte[length];
                accessor.ReadArray(16, img_data, 0, length);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(img_data);
                bitmapImage.EndInit();
                this.img_ctl.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}