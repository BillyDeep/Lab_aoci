using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace Lab_1_aoci_
{
    internal class VideoConverter
    {
        public ImageBox ImageBox1;
        public ImageBox ImageBox2;

        private VideoCapture _videoCapture;
        private ImageConverter _imageConverter = new ImageConverter();
        private string _fileName = "";

        public VideoConverter(ImageConverter imageConverter)
        {
            _imageConverter = imageConverter;
        }
        public void StartCaptureWebCam(bool needCanny, bool needThreshold)
        {
            if (_videoCapture != null) _videoCapture.Stop();
            _videoCapture = new VideoCapture();
            UnsubscribeFromEvents();
            if (needCanny) _videoCapture.ImageGrabbed += ProcessFrameCannyFilter;
            else if (needThreshold) _videoCapture.ImageGrabbed += ProcessFrameThresholdFilter;
            else _videoCapture.ImageGrabbed += ProcessFrame;

            _videoCapture.Start();
        }

        public void LoadFileVideo()
        {

            LoadNewVideoFile();
            if(_videoCapture != null)_videoCapture.Stop();

            _videoCapture = new VideoCapture(_fileName);

        }

        public void StartCaptureFileVideo(bool needCanny = false, bool needThreshold = false)
        {
            UnsubscribeFromEvents();
            var frame = _videoCapture.QueryFrame();
            
            Image<Bgr, byte> imageFrame = frame.ToImage<Bgr, byte>();
            
            if (needThreshold) _imageConverter.UseThresholdFilter(ImageBox2, imageFrame);
            else if (needCanny) ImageBox2.Image = _imageConverter.UseCannyFilter(imageFrame);
            else ImageBox1.Image = frame;

        }

        public void StopVideoCapture()
        {
            if (_videoCapture != null) _videoCapture.Stop();
        }

        private void UnsubscribeFromEvents()
        {
            
            _videoCapture.ImageGrabbed -= ProcessFrame;
            _videoCapture.ImageGrabbed -= ProcessFrameCannyFilter;
            _videoCapture.ImageGrabbed -= ProcessFrameThresholdFilter;
        }

        private void LoadNewVideoFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ("*.mp4 | *.mp4;");
            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                _fileName = fileName;
            }

        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            var frame = new Mat();
            _videoCapture.Retrieve(frame);
            ImageBox1.Image = frame;
        }

        private void ProcessFrameCannyFilter(object sender, EventArgs e)
        {
            var frame = new Mat();
            _videoCapture.Retrieve(frame);
            Image<Bgr, byte> imageFrame = frame.ToImage<Bgr, byte>();
            ImageBox2.Image = _imageConverter.UseCannyFilter(imageFrame);
        }

        private void ProcessFrameThresholdFilter(object sender, EventArgs e)
        {
            
            var frame = new Mat();
            _videoCapture.Retrieve(frame);
            Image<Bgr, byte> imageFrame = frame.ToImage<Bgr, byte>();
            _imageConverter.UseThresholdFilter(ImageBox2,imageFrame);
        }

    }
}
