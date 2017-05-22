using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BitmapScale
{
    public class ViewModel : INotifyPropertyChanged
    {
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapSource _srcBitmapSource;

        public BitmapSource SrcBitmapSource
        {
            get { return _srcBitmapSource; }
            set { _srcBitmapSource = value; OnPropertyChanged(); }
        }

        private BitmapSource _dstBitmapSource;
        public BitmapSource DstBitmapSource
        {
            get { return _dstBitmapSource; }
            set { _dstBitmapSource = value; OnPropertyChanged(); }
        }

        public List<ScaleMethod> _scaleMethods;
        public List<ScaleMethod> ScaleMethods
        {
            get { return _scaleMethods; }
            set { _scaleMethods = value; OnPropertyChanged(); }
        }

        private double _scaleRate;

        public double ScaleRate
        {
            get { return _scaleRate; }
            set { _scaleRate = value; OnPropertyChanged(); }
        }

        private ScaleMethod _selectMethod;

        public ScaleMethod SelectMethod
        {
            get { return _selectMethod; }
            set { _selectMethod = value; OnPropertyChanged(); }
        }

        public ICommand ScaleCommand { get; set; }
        public async void Scale()
        {
            if (SelectMethod == null)
            {
                return;
            }
           
            try
            {
                LogMessage($"开始缩放:", SelectMethod.Name);
                LogMessage($"缩放倍率({ScaleRate})", SelectMethod.Name);
                Stopwatch timer = new Stopwatch();
                timer.Start();

                var scale = SelectMethod.Method.Assembly.CreateInstance(SelectMethod.Method.FullName) as IBitmapScale;
                if (scale != null)
                {
                    var newSource = await scale.ScaleAsync(SrcBitmapSource, Math.Round(ScaleRate,1));
                    //var newSource = await scale.ScaleAsync(SrcBitmapSource,0.5);
                    DstBitmapSource = newSource;
                }
                timer.Stop();
                
                LogMessage($"缩放完成", SelectMethod.Name);
                LogMessage($"========================");
                LogMessage($"{timer.ElapsedMilliseconds}毫秒", $"耗时({SelectMethod.Name})");

                LogMessage($"{Environment.NewLine}");
            }
            catch (Exception e)
            {
                LogMessage($"{e.Message}", "发生异常");
            }
        }

        public ICommand PickImageCommand { get; set; }
        public void PickImage()
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
                    SrcBitmapSource = source;

                    LogMessage($"大小({buffer.Length}Byte)", "加载图片");
                }
            }
        }
        public ViewModel()
        {
            ScaleCommand = new Command(Scale);
            PickImageCommand = new Command(PickImage);

            Init();
        }
        public void Init()
        {
            ScaleMethods = new List<ScaleMethod>();
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

        private StringBuilder logStringBuilder = new StringBuilder();
        public string Log
        {
            get { return logStringBuilder.ToString(); }
        }

        public void LogMessage(string message, string title=null)
        {
            if (string.IsNullOrEmpty(title))
            {
                logStringBuilder.AppendLine($"[{DateTime.Now}]{message}");
            }
            else
            {

                logStringBuilder.AppendLine($"[{DateTime.Now}]{title}:{message}");
            }
            OnPropertyChanged("Log");
        }
    }
    public class ScaleMethod
    {
        public string Name { get; set; }
        public Type Method { get; set; }
    }

    public class Command : ICommand
    {
        public Action commandAction;
        public Command(Action action)
        {
            commandAction = action;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            commandAction?.Invoke();
        }
    }

    public class BitmapHelper
    {
        public static byte[] GetPictureData(string imagepath)
        {
            /**/
            ////根据图片文件的路径使用文件流打开，并保存为byte[]   
            FileStream fs = new FileStream(imagepath, FileMode.Open);//可以是其他重载方法   
            byte[] byData = new byte[fs.Length];
            fs.Read(byData, 0, byData.Length);
            fs.Close();
            return byData;
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bmp = null;

            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }

            return bmp;
        }
    }
}
