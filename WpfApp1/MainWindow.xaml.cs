using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImageController con = null; 
        
        int[,] pixels = null;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        //возвращает матрицу пикселей от pic
        private int[,] GetMatOfPixels(Bitmap pic)
        {
            int[,] res = new int[pic.Width, pic.Height];
            for (int i = 0; i < pic.Width; i++)
                for (int j = 0; j < pic.Height; j++)
                {
                    System.Drawing.Color col = pic.GetPixel(i, j);
                    double R = col.R;
                    double G = col.G;
                    double B = col.B;
                    int monochromValue = (int)(0.213 * R + 0.715 * G + 0.072 * B);
                    res[i, j] = monochromValue;
                }
            return res;
        }
        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
        //private void setImage(object argument)
        //{
        //    System.Windows.Controls.Image image = argument as System.Windows.Controls.Image;
        //    con = new ImageControllerGav(75, pixels);

        //    imageBin1.Source = con.CreateBitmapSourceFromGdiBitmap();
        //}

        private void imageBoxInput_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            Bitmap bitmap = new Bitmap("in1.jpg");
            pixels = GetMatOfPixels(bitmap);
            image.Source = CreateBitmapSourceFromGdiBitmap(bitmap);
        }

        private async void imageBoxInput_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog();
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            ofd.Title = "Open Image";
            ofd.Filter = "jpg files (*.jpg; *.jpeg)|*.jpg;*.jpeg";
            //ParameterizedThreadStart setImageThread = new ParameterizedThreadStart(setImage);
            //Thread thread1 = new Thread(setImageThread);
            if ((bool)ofd.ShowDialog())
            { 
                Bitmap bitmap = new Bitmap(ofd.FileName);
                pixels = GetMatOfPixels(bitmap);
                image.Source = CreateBitmapSourceFromGdiBitmap(bitmap);
            }

            //con = new ImageControllerOtsu(pixels);
            //imageBin1.Source = await con.CreateBitmapSourceFromGdiBitmap();
            //con = new ImageControllerGav(125, pixels);
            //imageBin2.Source = await con.CreateBitmapSourceFromGdiBitmap();
            con = new ImageControllerNiblec(pixels, 9, -0.01);
            imageBin3.Source = await con.CreateBitmapSourceFromGdiBitmap();
            con = new ImageControllerSouvol(pixels, 9, 0.01);
            imageBin4.Source = await con.CreateBitmapSourceFromGdiBitmap();
        }
    }
}
