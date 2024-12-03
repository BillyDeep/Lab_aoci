using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using static Lab_2_aoci_.ImageConverter;


namespace Lab_2_aoci_
{
    internal class ImageConverter
    {
        public enum OperationType { Addition, Multiplication, Subtraction }
        public enum ChannelsHsv { Hue, Saturation, Value }
        
        private enum ChannelsBgr { Blue, Green, Red }

        private Image<Bgr, byte> _sourceImage;

        public Image<Bgr, byte> LoadImage()
        {
            
            _sourceImage = LoadImageFromFile();
            if (_sourceImage == null) return null;

            return _sourceImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Bgr, byte> SingleChannelOutput(int channelIndex)
        {
            if (_sourceImage == null) return null;

            var channel = _sourceImage.Split()[channelIndex];
            Image<Bgr, byte> destImage = _sourceImage.CopyBlank();
            VectorOfMat vectorOfMat = new VectorOfMat();

            var copy = channel.CopyBlank();

            switch (channelIndex)
            {
                case (int)ChannelsBgr.Blue:
                    vectorOfMat.Push(channel);
                    vectorOfMat.Push(copy);
                    vectorOfMat.Push(copy);
                    break;
                case (int)ChannelsBgr.Green:
                    vectorOfMat.Push(copy);
                    vectorOfMat.Push(channel);
                    vectorOfMat.Push(copy);
                    break;
                case (int)ChannelsBgr.Red:
                    vectorOfMat.Push(copy);
                    vectorOfMat.Push(copy);
                    vectorOfMat.Push(channel);
                    break;
            }

            CvInvoke.Merge(vectorOfMat, destImage);

            return destImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Gray, byte> GrayVersionOutput()
        {
            if (_sourceImage == null) return null;

            var grayImage = new Image<Gray, byte>(_sourceImage.Size);

            for (int x = 0; x < grayImage.Width; x++)
                for (int y = 0; y < grayImage.Height; y++)
                {
                    grayImage.Data[y, x, (int)ChannelsBgr.Blue] = Convert.ToByte(0.299 * _sourceImage.Data[y, x, (int)ChannelsBgr.Red] + 0.587
                        * _sourceImage.Data[y, x, (int)ChannelsBgr.Green] + 0.114 * _sourceImage.Data[y, x, (int)ChannelsBgr.Blue]);
                }

            return grayImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Bgr, byte> SepiaVersionOutput()
        {
            if (_sourceImage == null) return null;

            var resultImage = new Image<Bgr, byte>(_sourceImage.Size);

            for (int x = 0; x < resultImage.Width; x++)
                for (int y = 0; y < resultImage.Height; y++)
                {
                    var sourceBlue = _sourceImage.Data[y, x, (int)ChannelsBgr.Blue];
                    var sourceGreen = _sourceImage.Data[y, x, (int)ChannelsBgr.Green];
                    var sourceRed = _sourceImage.Data[y, x, (int)ChannelsBgr.Red];

                    var blue = 0.272 * sourceRed + 0.534 * sourceGreen + 0.131 * sourceBlue;
                    var green = 0.349 * sourceRed + 0.686 * sourceGreen + 0.168 * sourceBlue;
                    var red = 0.393 * sourceRed + 0.769 * sourceGreen + 0.189 * sourceBlue;

                    resultImage.Data[y, x, (int)ChannelsBgr.Blue] = ColorToByte(blue);

                    resultImage.Data[y, x, (int)ChannelsBgr.Green] = ColorToByte(green);

                    resultImage.Data[y, x, (int)ChannelsBgr.Red] = ColorToByte(red);
                }

            return resultImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Bgr, byte> ContrastVersionOutput(double brightness, double contrast)
        {
            if (_sourceImage == null) return null;

            var resultImage = new Image<Bgr, byte>(_sourceImage.Size);

            for (int x = 0; x < resultImage.Width; x++)
                for (int y = 0; y < resultImage.Height; y++)
                {
                    var blue = (_sourceImage.Data[y, x, (int)ChannelsBgr.Blue] + brightness) * contrast;
                    var green = (_sourceImage.Data[y, x, (int)ChannelsBgr.Green] + brightness) * contrast;
                    var red = (_sourceImage.Data[y, x, (int)ChannelsBgr.Red] + brightness) * contrast;
                    
                    resultImage.Data[y, x, (int)ChannelsBgr.Blue] = ColorToByte(blue);
                    resultImage.Data[y, x, (int)ChannelsBgr.Green] = ColorToByte(green);
                    resultImage.Data[y, x, (int)ChannelsBgr.Red] = ColorToByte(red); 
                }

            return resultImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Bgr, byte> LogicalOperationOutput(OperationType operationType)
        {
            if (_sourceImage == null) return null;

            var secondImage = LoadImageFromFile();

            if (secondImage == null) return null;

            return UseLogicalOperation(_sourceImage.Resize(640, 480, Inter.Linear), secondImage, operationType);

        }

        private Image<Bgr, byte> UseLogicalOperation(Image<Bgr, byte> firstImage, Image<Bgr, byte> secondImage, OperationType operationType, double coefficient = 1)
        {
            var resultImage = new Image<Bgr, byte>(firstImage.Size);
            
            secondImage = secondImage.Resize(640, 480, Inter.Linear);

            var left = coefficient == 1 ? 1 : (1 - coefficient);
            var right = coefficient;

            for (int x = 0; x < resultImage.Width; x++)
                for (int y = 0; y < resultImage.Height; y++)
                {
                    var blue = firstImage.Data[y, x, (int)ChannelsBgr.Blue] * left;
                    var green = firstImage.Data[y, x, (int)ChannelsBgr.Green] * left;
                    var red = firstImage.Data[y, x, (int)ChannelsBgr.Red] * left;
                    
                    switch ((int)operationType)
                    {
                        case (int)OperationType.Addition:
                            blue += right * secondImage.Data[y, x, (int)ChannelsBgr.Blue];
                            green += right * secondImage.Data[y, x, (int)ChannelsBgr.Green];
                            red += right * secondImage.Data[y, x, (int)ChannelsBgr.Red];
                            break; 
                        case (int)OperationType.Multiplication:
                            blue *= right * secondImage.Data[y, x, (int)ChannelsBgr.Blue];
                            green *= right * secondImage.Data[y, x, (int)ChannelsBgr.Green];
                            red *= right * secondImage.Data[y, x, (int)ChannelsBgr.Red];
                            break;
                        case (int)OperationType.Subtraction:
                            blue -= right * secondImage.Data[y, x, (int)ChannelsBgr.Blue];
                            green -= right * secondImage.Data[y, x, (int)ChannelsBgr.Green];
                            red -= right * secondImage.Data[y, x, (int)ChannelsBgr.Red];
                            break;
                    } 

                    resultImage.Data[y, x, (int)ChannelsBgr.Blue] = ColorToByte(blue);
                    resultImage.Data[y, x, (int)ChannelsBgr.Green] = ColorToByte(green);
                    resultImage.Data[y, x, (int)ChannelsBgr.Red] = ColorToByte(red);
                }

            return resultImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Hsv, byte> HsvImageOutput(ChannelsHsv channelsHsv, double value)
        {
            if (_sourceImage == null) return null;

            var hsvImage = _sourceImage.Convert<Hsv, byte>();

            for (int x = 0; x < hsvImage.Width; x++)
                for (int y = 0; y < hsvImage.Height; y++)
                {
                    hsvImage.Data[y, x, (int)channelsHsv] = ColorToByte(value);
                }

            return hsvImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Gray, byte> MedianBlurVersionOutput()
        {
            if (_sourceImage == null) return null;

            int[,] matrix = new int[0, 0];
            var grayImage = GrayVersionOutput();
            var resultImage = grayImage.CopyBlank();
            int sh = 4;

            for (int x = sh; x < grayImage.Width - sh; x++)
                for (int y = sh; y < grayImage.Height - sh; y++)
                {
                    List<int> list = GetAverageValue(x, y, grayImage, matrix);
                    resultImage.Data[y,x, (int)ChannelsBgr.Blue] = ColorToByte(list[list.Count / 2]);
                }
            return resultImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Gray, byte> WindowFilterVersionOutput(int[,] matrix)
        {
            if (_sourceImage == null) return null;

            var grayImage = GrayVersionOutput();
            var resultImage = grayImage.CopyBlank();
            int sh = 1;

            for (int x = sh; x < grayImage.Width - sh; x++)
                for (int y = sh; y < grayImage.Height - sh; y++)
                {
                    List<int> list = GetAverageValue(x, y, grayImage, matrix);
                    int sum = list.ToArray().Sum();
                    resultImage.Data[y, x, (int)ChannelsBgr.Blue] = ColorToByte(sum);
                }
            return resultImage.Resize(640, 480, Inter.Linear);

        }

        public Image<Bgr, byte>CartoonFilterVersionOutput(int coefficient)
        {
            if (_sourceImage == null) return null;

            var sourceImage = _sourceImage.Resize(640, 480, Inter.Linear);
            var grayImage = sourceImage.Convert<Gray,byte>().Resize(640, 480, Inter.Linear);
            grayImage = grayImage.SmoothBlur(5, 5);

            grayImage = grayImage.ThresholdAdaptive(new Gray(coefficient), AdaptiveThresholdType.MeanC,ThresholdType.Binary, 3, new Gray(0.03));
            
            var resultImage = sourceImage.CopyBlank();

            for (int x = 0; x < resultImage.Width; x++)
                for (int y = 0; y < resultImage.Height; y++)
                {
                    var blue = sourceImage.Data[y, x, (int)ChannelsBgr.Blue];
                    var green = sourceImage.Data[y, x, (int)ChannelsBgr.Green];
                    var red = sourceImage.Data[y, x, (int)ChannelsBgr.Red];

                    
                    blue *= grayImage.Data[y, x, (int)ChannelsBgr.Blue];
                    green *= grayImage.Data[y, x, (int)ChannelsBgr.Blue];
                    red *= grayImage.Data[y, x, (int)ChannelsBgr.Blue];

                    resultImage.Data[y, x, (int)ChannelsBgr.Blue] = ColorToByte(blue);
                    resultImage.Data[y, x, (int)ChannelsBgr.Green] = ColorToByte(green);
                    resultImage.Data[y, x, (int)ChannelsBgr.Red] = ColorToByte(red);
                }
            
            return resultImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Bgr, byte> WatercolorFilterVersionOutput(double brightness, double contrast, double coefficient)
        {
            if (_sourceImage == null) return null;
            var contrastImage = ContrastVersionOutput(brightness, contrast);
            var blurImage = contrastImage.SmoothMedian(1);

            var mask = LoadImageFromFile();
            if (mask == null) return null;

            var result = UseLogicalOperation(blurImage, mask, OperationType.Addition, coefficient);

            return result.Resize(640, 480, Inter.Linear);

        }

        private List<int> GetAverageValue(int x, int y, Image<Gray, byte> grayImage, int[,] matrix)
        {
            List<int> list = new List<int>();

            for(int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    int res = grayImage.Data[i + y, j + x, (int)ChannelsBgr.Blue];

                    if (matrix.Length != 0) 
                        res = res * matrix[i + 1, j + 1];

                    list.Add(res);
                }

            list.Sort();

            return list;
        }
        private byte ColorToByte(double color)
        {
            if (color > 255) color = 255;
            else if (color < 0) color = 0;

            return (byte)color;
        }

        private Image<Bgr, byte> LoadImageFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ("*.png | *.jpg; *.jpeg; *.png");
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var fileName = openFileDialog.FileName;
                return new Image<Bgr, byte>(fileName);
            }

            return null;
        }


    }
}
