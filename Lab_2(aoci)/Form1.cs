using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_2_aoci_
{
    public partial class Form1 : Form
    {
        private ImageConverter _imageConverter = new ImageConverter();

        public Form1()
        {
            InitializeComponent();
            UpdateForm();
        }

        private void UseOnlyNumbers(KeyPressEventArgs e, char key = ' ')
        {
            char number = e.KeyChar;
            
            if (!Char.IsDigit(number) && number != 8 && number != (int)key)
            {
                e.Handled = true;
            }
        }

        private void TextBoxLimit(TextBox textBox, double minValue, double maxValue)
        {
            try
            {
                double currentValue = (textBox.Text == "") ? 0 : Convert.ToDouble(textBox.Text);

                if (currentValue > maxValue) currentValue = maxValue;
                if (currentValue < minValue) currentValue = minValue;

                textBox.Text = Convert.ToString(currentValue);
            }
            catch { }
        }

        private int[,] DataGridviewToMatrix(DataGridView dataGridView)
        {
            int row = dataGridView.Rows.Count;
            int columns = dataGridView.Columns.Count;
            int[,] matrix = new int[row, columns];

            for (int i = 0; i < row; i++)
                for (int j = 0; j < columns; j++)
                    matrix[i,j] = Convert.ToInt32(dataGridView.Rows[i].Cells[j].Value);
            
            return matrix;

        }

        private void UpdateForm()
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.AddRange(Enum.GetNames(typeof(ImageConverter.OperationType)));
            comboBox2.SelectedIndex = 0;
            comboBox3.Items.AddRange(Enum.GetNames(typeof(ImageConverter.ChannelsHsv)));
            comboBox3.SelectedIndex = 0;

            dataGridView1.Rows.Clear();
            dataGridView1.CellValueChanged -= dataGridView1_CellValueChanged;
            for (int i = 0; i < 3; i++)
            {
                dataGridView1.Rows.Add();
               
            }

            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    dataGridView1.Rows[j].Cells[i].Value = "1";
                }
            }

            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;

            double value = trackBar1.Value;
            label6.Text = $"Contrast: {value / 10}";

            double value2 = trackBar2.Value;
            label9.Text = $"Coefficient: {value2 / 100}";

            textBox4.Text = "50";
            textBox2.Text = "1";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            imageBox1.Image = _imageConverter.LoadImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            imageBox2.Image = _imageConverter.SingleChannelOutput(comboBox1.SelectedIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            imageBox2.Image = _imageConverter.GrayVersionOutput();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            imageBox2.Image = _imageConverter.SepiaVersionOutput();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            double brightness = Convert.ToDouble(textBox4.Text);
            double contrast = Convert.ToDouble(trackBar1.Value);
            imageBox2.Image = _imageConverter.ContrastVersionOutput(brightness, contrast / 10);
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            imageBox2.Image = _imageConverter.LogicalOperationOutput((ImageConverter.OperationType)comboBox2.SelectedIndex);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int currentValue = (textBox1.Text == "") ? 0 : Convert.ToInt32(textBox1.Text);
            imageBox2.Image = _imageConverter.HsvImageOutput((ImageConverter.ChannelsHsv)comboBox3.SelectedIndex, currentValue);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            UseOnlyNumbers(e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool isHue = comboBox3.SelectedIndex == (int)ImageConverter.ChannelsHsv.Hue;
            bool isSaturation = comboBox3.SelectedIndex == (int)ImageConverter.ChannelsHsv.Saturation;
            bool isValue = comboBox3.SelectedIndex == (int)ImageConverter.ChannelsHsv.Value;

            int currentValue = (textBox1.Text == "")? 0 : Convert.ToInt32(textBox1.Text);

            if (isHue && currentValue > 360)
                textBox1.Text = "360";
            else if ((isSaturation || isValue) && currentValue > 100)
                textBox1.Text = "100";
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = "0";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            imageBox2.Image = _imageConverter.MedianBlurVersionOutput();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var matrix = DataGridviewToMatrix(dataGridView1);
            imageBox2.Image = _imageConverter.WindowFilterVersionOutput(matrix);
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox tb = (TextBox)e.Control;
            tb.KeyPress += new KeyPressEventHandler(tb_KeyPress);
        }

        private void tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            UseOnlyNumbers(e, '-');
        }

        private void Contrast_KeyPress(object sender, KeyPressEventArgs e)
        {
            UseOnlyNumbers(e, '.');
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(dataGridView1.CurrentCell.Value == null) return;
            int currentValue = dataGridView1.CurrentCell.Value == "" ? 0 : Convert.ToInt32(dataGridView1.CurrentCell.Value);

            int maxValue = 255;
            int minValue = -255;

            if (currentValue < minValue) currentValue = minValue;
            if (currentValue > maxValue) currentValue = maxValue;

            dataGridView1.CurrentCell.Value = currentValue;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            imageBox2.Image = _imageConverter.CartoonFilterVersionOutput(Convert.ToInt32(textBox2.Text));
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

            TextBoxLimit(textBox2, 0, 255);

        }

        private void button11_Click(object sender, EventArgs e)
        {
            double brightness = Convert.ToDouble(textBox4.Text);
            double contrast = Convert.ToDouble(trackBar1.Value);
            double coefficient = Convert.ToDouble(trackBar2.Value);
            imageBox2.Image = _imageConverter.WatercolorFilterVersionOutput(brightness, contrast/ 10, coefficient / 100);
        }


        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            TextBoxLimit(textBox4, 0, 255);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            double value = trackBar1.Value;
            label6.Text = $"Contrast: {value / 10}";
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            double value = trackBar2.Value;
            label9.Text = $"Coefficient: {value / 100}";
        }
    }
}
