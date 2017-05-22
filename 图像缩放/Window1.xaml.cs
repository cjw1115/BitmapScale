using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace BitmapScale
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Init();
        }
        BitmapSource srcBitmap;
        BitmapSource dstBitmap;
        public List<ScaleMethod> ScaleMethods = new List<ScaleMethod>();
        public void Init()
        {
            var method = new ScaleMethod();
            method.Name = "最近邻插值";
            method.Method = typeof(NearestScale);
            ScaleMethods.Add(method);

            method = new ScaleMethod();
            method.Name = "双线性插值";
            method.Method = typeof(BilinearScale);
            ScaleMethods.Add(method);

            method = new ScaleMethod();
            method.Name = "双三次插值";
            method.Method = typeof(BicubicScale);
            ScaleMethods.Add(method);
        }
        private async void btnNearest_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Stopwatch timer = new Stopwatch();
                timer.Start();

                var scale = typeof(NearestScale).Assembly.CreateInstance(typeof(NearestScale).FullName) as IBitmapScale;
                if (scale != null)
                {
                    var newSource = await scale.ScaleAsync(srcBitmap, Math.Round(ScaleRate, 1));
                    //var newSource = await scale.ScaleAsync(SrcBitmapSource,0.5);
                    imgDst.Source = newSource;
                }
                timer.Stop();
                MessageBox.Show("Finished");

            }
            catch (Exception)
            {
                MessageBox.Show("ERROR");
            }
        }
        public double ScaleRate
        {
            get { return double.Parse(tbRate.Text); }
        }
        private async void btnBilinear_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Stopwatch timer = new Stopwatch();
                timer.Start();

                var scale = typeof(BilinearScale).Assembly.CreateInstance(typeof(BilinearScale).FullName) as IBitmapScale;
                if (scale != null)
                {
                    var newSource = await scale.ScaleAsync(srcBitmap, Math.Round(ScaleRate, 1));
                    //var newSource = await scale.ScaleAsync(SrcBitmapSource,0.5);
                    imgDst.Source= newSource;
                }
                timer.Stop();MessageBox.Show("Finished");

            }
            catch (Exception)
            {
                MessageBox.Show("ERROR");
            }
        }

        private async void btnBicubic_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Stopwatch timer = new Stopwatch();
                timer.Start();

                var scale = typeof(BicubicScale).Assembly.CreateInstance(typeof(BicubicScale).FullName) as IBitmapScale;
                if (scale != null)
                {
                    var newSource = await scale.ScaleAsync(srcBitmap, Math.Round(ScaleRate, 1));
                    //var newSource = await scale.ScaleAsync(SrcBitmapSource,0.5);
                    imgDst.Source  = newSource;
                }
                timer.Stop();MessageBox.Show("Finished");

            }
            catch (Exception)
            {
                MessageBox.Show("ERROR");
            }
        }

        private void btnPicker_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "(*.bmp)|*.bmp;";
            if (ofd.ShowDialog() == true)
            {
                using (var imageFileStream = ofd.OpenFile())
                {
                    var buffer = new byte[imageFileStream.Length];
                    imageFileStream.Read(buffer, 0, buffer.Length);
                    var source = BitmapHelper.ByteArrayToBitmapImage(buffer);
                   imgSrc.Source=srcBitmap = source;
                }
            }
        }
    }
}
