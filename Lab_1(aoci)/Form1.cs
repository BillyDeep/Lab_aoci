using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lab_1_aoci_
{
    public partial class Form1 : Form
    {
        private ImageConverter _imageConverter = new ImageConverter();
        private VideoConverter _videoConverter;
        private bool _videoFilNeedCannyFilter = false;
        private bool _videoFilNeedThresholdFilter = false;


        public Form1()
        {
            InitializeComponent();
            UpdateFormText();
            _videoConverter = new VideoConverter(_imageConverter);
        }

        private void UseOnlyNumbers(KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8)
            {
                e.Handled = true;
            }
        }

        private void UpdateFormText()
        {
            trackBar1.Value = (int)_imageConverter.CannyThreshold;
            trackBar2.Value = (int)_imageConverter.CannyThresholdLinking;
            label2.Text = $"Canny threshold = {trackBar1.Value}";
            label3.Text = $"Canny threshold linking = {trackBar2.Value}";
            textBox1.Text = Convert.ToString(_imageConverter.ThresholdColorValues[4]);
            textBox2.Text = Convert.ToString(_imageConverter.ThresholdColorValues[3]);
            textBox3.Text = Convert.ToString(_imageConverter.ThresholdColorValues[2]);
            textBox4.Text = Convert.ToString(_imageConverter.ThresholdColorValues[1]);
            textBox5.Text = Convert.ToString(_imageConverter.ThresholdColorValues[0]);
        }

        private void SetFiltersSettings()
        {
            List<int> list = new List<int>();
            foreach (Control textBox in Controls)
            {
                if (textBox is System.Windows.Forms.TextBox) list.Add(Convert.ToInt32(textBox.Text));
            }
            _imageConverter.SetCannySettings(trackBar1.Value, trackBar2.Value, list);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _videoConverter.StopVideoCapture();
            _imageConverter.LoadNewImage(imageBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _videoConverter.StopVideoCapture();
            SetFiltersSettings();
            _imageConverter.ApplyCannyFilter(imageBox2);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            _videoConverter.StopVideoCapture();
            SetFiltersSettings();
            _imageConverter.ApplyThresholdFilter(imageBox2);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = $"Canny threshold = {trackBar1.Value}";
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label3.Text = $"Canny threshold linking = {trackBar2.Value}";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            UseOnlyNumbers(e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            _videoConverter.ImageBox1 = imageBox1;
            try{ _videoConverter.StartCaptureWebCam(false,false);} catch (Exception) { }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            SetFiltersSettings();
            _videoConverter.ImageBox2 = imageBox2;
            try { _videoConverter.StartCaptureWebCam(true, false); } catch (Exception) { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            SetFiltersSettings();
            _videoConverter.ImageBox2 = imageBox2;
            try { _videoConverter.StartCaptureWebCam(false,true); } catch (Exception) { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _videoConverter.ImageBox1 = imageBox1;
            _videoConverter.LoadFileVideo();
            _videoFilNeedCannyFilter = false;
            _videoFilNeedThresholdFilter = false;
            timer1.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetFiltersSettings();
            _videoConverter.ImageBox2 = imageBox2;
            _videoFilNeedCannyFilter = true;
            _videoFilNeedThresholdFilter = false;
            timer1.Enabled = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SetFiltersSettings();
            _videoConverter.ImageBox2 = imageBox2;
            _videoFilNeedCannyFilter = false;
            _videoFilNeedThresholdFilter = true;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try { _videoConverter.StartCaptureFileVideo(_videoFilNeedCannyFilter, _videoFilNeedThresholdFilter); } catch (Exception) { }

        }
    }
}
