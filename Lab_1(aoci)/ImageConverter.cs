using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace Lab_1_aoci_
{
    internal class ImageConverter
    {
        public double CannyThreshold { get; private set; } = 80;
        public double CannyThresholdLinking { get; private set; } = 40;
        public List<int> ThresholdColorValues { get; private set; } = new List<int>(5) {0,25,180,210,255};

        private Image<Bgr, byte> _sourceImage;

        public void LoadNewImage(ImageBox imageBox)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ("*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif *.png");
            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                _sourceImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);
                imageBox.Image = _sourceImage;
            }
        }

        public void ApplyCannyFilter(ImageBox imageBox)
        {
            if (_sourceImage != null) imageBox.Image = UseCannyFilter(_sourceImage);
        }

        public void UseThresholdFilter(ImageBox imageBox, Image<Bgr, byte> image)
        {
            if (image == null) return;

            var cannyEdges = UseCannyFilter(image);
            var cannyEdgesBgr = cannyEdges.Convert<Bgr, byte>();

            var resultImage = image.Sub(cannyEdgesBgr);

            for (int channel = 0; channel < resultImage.NumberOfChannels; channel++)
                for (int x = 0; x < resultImage.Width; x++)
                    for (int y = 0; y < resultImage.Height; y++)
                    {
                        byte color = resultImage.Data[y, x, channel];
                    
                        if (color <= 50) color = (byte)ThresholdColorValues[0];
                        else if (color <= 100) color = (byte)ThresholdColorValues[1];
                        else if (color <= 150) color = (byte)ThresholdColorValues[2];
                        else if (color <= 200) color = (byte)ThresholdColorValues[3];
                        else color = (byte)ThresholdColorValues[4];

                        resultImage.Data[y, x, channel] = color;
                    }

            imageBox.Image = resultImage;
        }

        public void SetCannySettings(double cannyThresholdValue, double cannyThresholdLinkingValue, List<int> list)
        {
            CannyThreshold = cannyThresholdValue;
            CannyThresholdLinking = cannyThresholdLinkingValue;

            for (int i = 0; i < list.Count; i++)
            {
                ThresholdColorValues[i] = list[i];
            }
        }

        public Image<Gray, byte> UseCannyFilter(Image<Bgr, byte> image)
        {
            try
            {
                Image<Gray, byte> grayImage = image.Convert<Gray, byte>();
                var tempImage = grayImage.PyrDown();
                var destImage = tempImage.PyrUp();

                Image<Gray, byte> cannyEdges = destImage.Canny(CannyThreshold, CannyThresholdLinking);

                return cannyEdges;
            }
            catch (Exception) { return null;}
        }

        public void ApplyThresholdFilter(ImageBox imageBox)
        {
            UseThresholdFilter(imageBox, _sourceImage);
        }

    }
}
