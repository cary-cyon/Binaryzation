using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    internal abstract class ImageController
    {
        protected int[,] pic;
        protected abstract Task Binaryzation();

        public virtual async Task<BitmapSource> CreateBitmapSourceFromGdiBitmap()
        {

            Bitmap bitmap = new Bitmap(pic.GetLength(0), pic.GetLength(1));
            await Binaryzation();
            for (int i = 0; i < pic.GetLength(0); i++)
            {
                for (int j = 0; j < pic.GetLength(1); j++)
                {
                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(pic[i, j], pic[i, j], pic[i, j]));
                }
            }
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
        }// end CreateBitmapSourceFromGdiBitmap
    } //end ImageController

    internal class ImageControllerGav : ImageController
    {
        private double Level=125;
        public ImageControllerGav(double Lv, int[,] image )
        {
            Level = Lv;
            pic = (int[,])image.Clone();
            
        }
        protected override async Task Binaryzation()
        {
            for (int i = 0; i < pic.GetLength(0); i++)
            {
                for (int j = 0; j < pic.GetLength(1); j++)
                {
                    pic[i, j] = pic[i, j] < Level ? 0:255;
                }
            }
            
        } // end Binaryzation

    } // end ImageControllerGav

    internal class ImageControllerOtsu : ImageController
    {
        private double Level;
        public ImageControllerOtsu(int[,] image)
        {
            pic = (int[,])image.Clone();
            double[] gist = new double[256];
            double size = pic.GetLength(0) * pic.GetLength(1);
            
            for(int i = 0; i < pic.GetLength(0); i++)
            {
                for(int j = 0; j < pic.GetLength(1); j++)
                {
                    double Br = pic[i, j];
                    gist[(int)Br] += Br / size; 
                }
            }
            double sigma = double.MinValue;

            for(int i =0; i < 256; i++)
            {
                double omega1 = 0;
                double omega2 = 0;
                double mu1 = 0;
                double muT = 0;
                double mu2 = 0;
                double new_sigma = 0;
                for (int j = 0; j <= i; j++)
                {
                    omega1 += gist[j];
                }
                omega2 = 1 - omega1;
                for (int j = 0; j <= i; j++)
                {
                    mu1 += j*gist[j]/omega1;
                }
                for (int j = 0; j <= i; j++)
                {
                    muT += j * gist[j];
                }
                mu2 = (muT - mu1 * omega1) / omega2;
                new_sigma = omega1 * omega2 * Math.Pow(mu1 - mu2, 2);
                if (sigma < new_sigma)
                {

                    sigma = new_sigma;
                    Level = i;
                }
            }


        }
        protected override async Task Binaryzation()
        {
            for (int i = 0; i < pic.GetLength(0); i++)
            {
                for (int j = 0; j < pic.GetLength(1); j++)
                {
                    pic[i, j] = pic[i, j] < Level ? 0 : 255;
                }
            }

        } // end Binaryzation
    } //end Otsu
    class ImageControllerLocal : ImageController
    {
        protected int size;
        protected override Task Binaryzation()
        {
            throw new NotImplementedException();
        }
        protected List<double> power(List<double> val)
        {
            List<double> ret = new List<double>();
            foreach(double v in val)
                ret.Add(v * v);
            return ret;

        }
        protected List<double> GetValuesLocal(int i, int j)

        {
            List<double> values = new List<double>();
            int start_i = i - (size - 1) / 2;
            int start_j = j - (size - 1) / 2;
            int end_i = start_i + size;
            int end_j = start_j + size;

            if (start_i < 0)
                start_i = 0;
            if (end_i > pic.GetLength(0))
                end_i = pic.GetLength(0);
            if (start_j < 0)
                start_j = 0;
            if (end_j > pic.GetLength(1))
                end_j = pic.GetLength(1);

            for (int i1 = start_i; i1 < end_i; i1++)
                for(int j1 = start_j; j1 < end_j; j1++)
                {
                    try
                    {
                        values.Add(pic[i1, j1]);
                    }
                    catch
                    {

                    }
                    
                }

            return values;
        }
    }
    class ImageControllerNiblec : ImageControllerLocal
    {
        protected double k;
        protected int[,] new_pic;
        public ImageControllerNiblec(int[,] image, int Size, double K = -0.2)
        {
            pic = (int[,])image.Clone();
            new_pic = new int[pic.GetLength(0), pic.GetLength(1)];
            size = Size;
            k = K;
        }
        

        protected override async Task Binaryzation()
        {

            for(int i = 0; i < pic.GetLength(0); i++)
            {
                for(int j = 0; j<pic.GetLength(1); j++)
                {
                    double math_ex = 0;
                    double math_ex2 = 0;
                    double dispersion = 0;
                    List<double> values = GetValuesLocal(i, j);
                    math_ex = values.Sum() / values.Count;
                    math_ex2 = power(values).Sum() / values.Count;
                    dispersion = math_ex2 - math_ex*math_ex;
                    double locLevel = math_ex + k*Math.Sqrt(dispersion);
                    new_pic[i,j] = pic[i, j] <= locLevel? 0 : 255;

                }
            }
            pic = new_pic;
        }
    }
    class ImageControllerSouvol : ImageControllerNiblec
    {
        public ImageControllerSouvol(int[,] image, int Size, double K = 0.3):base(image, Size, K)
        {
            pic = (int[,])image.Clone();
            new_pic = new int[pic.GetLength(0), pic.GetLength(1)];
            size = Size;
            k = K;
        }
        protected override async Task Binaryzation()
        {

            for (int i = 0; i < pic.GetLength(0); i++)
            {
                for (int j = 0; j < pic.GetLength(1); j++)
                {
                    double math_ex = 0;
                    double math_ex2 = 0;
                    double dispersion = 0;
                    List<double> values = GetValuesLocal(i, j);
                    math_ex = values.Sum() / values.Count;
                    math_ex2 = power(values).Sum() / values.Count;
                    dispersion = math_ex2 - math_ex * math_ex;
                    double locLevel = math_ex * (1+ k*(Math.Sqrt(dispersion) / 128 - 1));
                    new_pic[i, j] = pic[i, j] <= locLevel ? 0 : 255;

                }
            }
            pic = new_pic;
        }

    }


}
