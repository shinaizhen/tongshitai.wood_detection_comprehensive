using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 控件库
{
    /// <summary>
    /// IntUpDownControl.xaml 的交互逻辑
    /// </summary>
    public partial class IntUpDownControl : UserControl
    {
        public int MyInt
        {
            get { return (int)GetValue(MyIntProperty); }
            set { SetValue(MyIntProperty, value); }
        }
        public readonly DependencyProperty MyIntProperty = DependencyProperty.Register(nameof(MyInt), typeof(int), typeof(IntUpDownControl), new PropertyMetadata(0));
        public IntUpDownControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newFontSize = CalculateFontSizeBasedOnWindowSize(e.NewSize);
            txtbox_int.FontSize = newFontSize;
        }

        /// <summary>
        /// 计算大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private double CalculateFontSizeBasedOnWindowSize(Size size)
        {
            // 根据窗口大小进行计算的逻辑，例如
            if (size.Height < 30)
            {
                return 16;
            }
            else
            {
                return 18;
            }
        }

        private void btn_Down_Click(object sender, RoutedEventArgs e)
        {
            if (MyInt == 0)
                MessageBox.Show("输入值不小于0");
            else
                MyInt--;
        }

        private void btn_UP_Click(object sender, RoutedEventArgs e)
        {
            MyInt++;
        }
    }
}
