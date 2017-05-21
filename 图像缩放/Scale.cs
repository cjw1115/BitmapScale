using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BitmapScale
{
    interface IBitmapScale
    {
        Task<BitmapSource> ScaleAsync(BitmapSource source,int scaleRate);
    }

    public class NearestScale : IBitmapScale
    {
        public async Task<BitmapSource> ScaleAsync(BitmapSource bitmapSource, int scaleRate)
        {
            var oldPixelWidth = bitmapSource.PixelWidth;
            var oldPixelHeight = bitmapSource.PixelHeight;

            var bytesCount = oldPixelWidth * oldPixelHeight * bitmapSource.Format.BitsPerPixel / 8;

            var pixels = new byte[bytesCount];

            bitmapSource.CopyPixels(pixels, bitmapSource.Format.BitsPerPixel / 8 * oldPixelWidth, 0);
            var newPixls = new byte[(int)(pixels.Length * scaleRate * scaleRate)];
            await Task.Run(()=> 
            {
                

                var oldStride = oldPixelWidth * 4;
                var newStride = oldPixelWidth * 4 * scaleRate;

                var newPixelHeiht = oldPixelHeight * scaleRate;
                var newPixelWidth = oldPixelWidth * scaleRate;
                for (int i = 0; i < newPixelHeiht; i++)
                {
                    for (int j = 0; j < newPixelWidth; j++)
                    {
                        var srcX = j * oldPixelWidth / (double)(oldPixelWidth * scaleRate);
                        srcX = Math.Round(srcX);
                        if (srcX >= oldPixelWidth)
                            srcX = oldPixelWidth - 1;

                        var srcY = i * oldPixelHeight / (double)(oldPixelHeight * scaleRate);
                        srcY = Math.Round(srcY);
                        if (srcY >= oldPixelHeight)
                            srcY = oldPixelHeight - 1;

                        var newPos = 4 * (i * newPixelWidth + j);
                        var oldPos = (int)(4 * (srcY * oldPixelWidth + srcX));
                        newPixls[newPos + 0] = pixels[oldPos + 0];
                        newPixls[newPos + 1] = pixels[oldPos + 1];
                        newPixls[newPos + 2] = pixels[oldPos + 2];
                        newPixls[newPos + 3] = pixels[oldPos + 3];

                    }
                }
            });

            var newSource = BitmapSource.Create((int)(oldPixelWidth * scaleRate), (int)(oldPixelHeight * scaleRate), bitmapSource.DpiX, bitmapSource.DpiY, PixelFormats.Bgr32, null, newPixls, (int)(4 * oldPixelWidth * scaleRate));

            return newSource;
        }
    }

    public class BilinearScale: IBitmapScale
    {
        public async Task<BitmapSource> ScaleAsync(BitmapSource bitmapSource, int scaleRate)
        {
            var oldPixelWidth = bitmapSource.PixelWidth;
            var oldPixelHeight = bitmapSource.PixelHeight;

            var bytesCount = oldPixelWidth * oldPixelHeight * bitmapSource.Format.BitsPerPixel / 8;
            //var width = bytesCount * oldPixelWidth / (oldPixelWidth + oldPixelHeight);
            //var height= bytesCount * oldPixelHeight / (oldPixelWidth + oldPixelHeight);

            var pixels = new byte[bytesCount];

            bitmapSource.CopyPixels(pixels, bitmapSource.Format.BitsPerPixel / 8 * oldPixelWidth, 0);

            var newPixls = new byte[(int)(pixels.Length * scaleRate * scaleRate)];

            await Task.Run(() =>
            {
                var oldStride = oldPixelWidth * 4;
                var newStride = oldPixelWidth * 4 * scaleRate;

                var newPixelHeiht = oldPixelHeight * scaleRate;
                var newPixelWidth = oldPixelWidth * scaleRate;
                for (int i = 0; i < newPixelHeiht; i++)
                {
                    for (int j = 0; j < newPixelWidth; j++)
                    {
                        var srcX = j * oldPixelWidth / (double)(oldPixelWidth * scaleRate);
                        var srcY = i * oldPixelHeight / (double)(oldPixelHeight * scaleRate);
                        var x = (int)srcX;
                        if (x + 1 == oldPixelWidth)
                        {
                            x -= 1;
                        }
                        var u = srcX - x;

                        var y = (int)srcY;
                        if (y + 1 == oldPixelHeight)
                        {
                            y -= 1;
                        }
                        var v = srcY - y;



                        var oldPos0 = (int)(4 * (y * oldPixelWidth + x));
                        var oldPos1 = (int)(4 * (y * oldPixelWidth + x + 1));
                        var oldPos2 = (int)(4 * ((y + 1) * oldPixelWidth + x));
                        var oldPos3 = (int)(4 * ((y + 1) * oldPixelWidth + x + 1));

                        var pos0Weight = CalcPixel(oldPos0, (1 - u) * (1 - v));
                        var pos1Weight = CalcPixel(oldPos1, u * (1 - v));
                        var pos2Weight = CalcPixel(oldPos2, (1 - u) * v);
                        var pos3Weight = CalcPixel(oldPos3, u * v);

                        var value = AddWeight(pos0Weight, pos1Weight, pos2Weight, pos3Weight);


                        var newPos = 4 * (i * newPixelWidth + j);

                        newPixls[newPos + 0] = value[0];
                        newPixls[newPos + 1] = value[1];
                        newPixls[newPos + 2] = value[2];
                        newPixls[newPos + 3] = 255;

                    }
                }
            });


            var newSource = BitmapSource.Create((int)(oldPixelWidth * scaleRate), (int)(oldPixelHeight * scaleRate), bitmapSource.DpiX, bitmapSource.DpiY, PixelFormats.Bgr32, null, newPixls, (int)(4 * oldPixelWidth * scaleRate));

            return newSource;

            byte[] CalcPixel(int pos, double rate)
            {
                byte[] newPixel = new byte[4];
                for (int i = 0; i < newPixel.Length; i++)
                {
                    newPixel[i] = (byte)(pixels[pos + i] * rate);
                }
                return newPixel;
            }
            byte[] AddWeight(byte[] weight0, byte[] weight1, byte[] weight2, byte[] weight3)
            {
                byte[] value = new byte[4];
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = (byte)(weight0[i] + weight1[i] + weight2[i] + weight3[i]);

                }
                return value;
            }
        }
        
    }
    public class BicubicScale : IBitmapScale
    {
        public async Task<BitmapSource> ScaleAsync(BitmapSource bitmapSource, int scaleRate)
        {
            var oldPixelWidth = bitmapSource.PixelWidth;
            var oldPixelHeight = bitmapSource.PixelHeight;

            var bytesCount = oldPixelWidth * oldPixelHeight * bitmapSource.Format.BitsPerPixel / 8;

            var pixels = new byte[bytesCount];

            bitmapSource.CopyPixels(pixels, bitmapSource.Format.BitsPerPixel / 8 * oldPixelWidth, 0);

            var newPixls = new byte[(int)(pixels.Length * scaleRate * scaleRate)];

            await Task.Run(() =>
            {
                var oldStride = oldPixelWidth * 4;
                var newStride = oldPixelWidth * 4 * scaleRate;

                var newPixelHeiht = oldPixelHeight * scaleRate;
                var newPixelWidth = oldPixelWidth * scaleRate;
                for (int i = 0; i < newPixelHeiht; i++)
                {
                    for (int j = 0; j < newPixelWidth; j++)
                    {
                        var srcX = j * oldPixelWidth / (double)(oldPixelWidth * scaleRate);
                        var srcY = i * oldPixelHeight / (double)(oldPixelHeight * scaleRate);
                        var x = (int)srcX;
                        var u = srcX - x;
                        if (x + 2 >= oldPixelWidth)
                        {
                            x -= 2;
                        }
                        if (x - 1 < 0)
                        {
                            x += 1;
                        }


                        var y = (int)srcY;
                        var v = srcY - y;
                        if (y + 1 >= oldPixelHeight)
                        {
                            y -= 1;
                        }
                        if (y - 2 < 0)
                        {
                            y += 2;
                        }


                        var A = Matrix<double>.Build;
                        double[] uInter = new double[4] { CalcInterpolation(1 + u), CalcInterpolation(u), CalcInterpolation(1 - u), CalcInterpolation(2 - u) };
                        double[] vInter = new double[4] { CalcInterpolation(1 + v), CalcInterpolation(v), CalcInterpolation(1 - v), CalcInterpolation(2 - v) };

                        double[] pixelValue = new double[16];


                        var matrixA = A.Dense(1, 4, uInter);

                        var matrixC = A.Dense(4, 1, vInter);


                        byte[] newPiexl = new byte[4];

                        for (int index = 0; index < 3; index++)
                        {
                            int count = 0;
                            for (int r = y - 2; r < y + 2; r++)
                            {
                                for (int l = x - 1; l < x + 3; l++)
                                {
                                    pixelValue[count++] = GetPixel(4 * (r * oldPixelWidth + l), index);
                                }
                            }
                            var matrixB = A.Dense(4, 4, pixelValue);

                            var newPiexlMatric = matrixA * matrixB * matrixC;

                            newPiexl[index] = (byte)newPiexlMatric[0, 0];

                        }

                        var newPos = 4 * (i * newPixelWidth + j);

                        newPixls[newPos + 0] = newPiexl[0];
                        newPixls[newPos + 1] = newPiexl[1];
                        newPixls[newPos + 2] = newPiexl[2];
                        newPixls[newPos + 3] = 255;


                    }
                }
            });
            var newSource = BitmapSource.Create((int)(oldPixelWidth * scaleRate), (int)(oldPixelHeight * scaleRate), bitmapSource.DpiX, bitmapSource.DpiY, PixelFormats.Bgr32, null, newPixls, (int)(4 * oldPixelWidth * scaleRate));

            return newSource;

            byte GetPixel(int pos, int index)
            {
                return (byte)(pixels[pos + index]);
            }
            ///双三次内插值函数
            double CalcInterpolation(double value)
            {
                if (value < 0)
                {
                    value = -value;
                }
                if (value >= 2)
                    return 0;
                if (value >= 1)
                {
                    return 4 - 8 * value + 5 * value * value - value * value * value;
                }
                return 1 - 2 * value * value + value * value * value;
            }
        }
    }
}
