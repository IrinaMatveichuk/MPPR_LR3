using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace NeuralNetwork
{
    public partial class Form1 : Form
    {
        int[,] pixels;
        string pixelFile = @"C:\Users\User\source\repos\NeuralNetwork\NeuralNetwork\train-images.idx3-ubyte";
        string labelFile = @"C:\Users\User\source\repos\NeuralNetwork\NeuralNetwork\train-labels.idx1-ubyte";
        string testpixelFile = @"C:\Users\User\source\repos\NeuralNetwork\NeuralNetwork\t10k-images.idx3-ubyte";
        string testlabelFile = @"C:\Users\User\source\repos\NeuralNetwork\NeuralNetwork\t10k-labels.idx1-ubyte";
        string testpixelFile1 = @"C:\Users\User\source\repos\NeuralNetwork\NeuralNetwork\t10k-images-copy.idx3-ubyte";
        string testlabelFile1 = @"C:\Users\User\source\repos\NeuralNetwork\NeuralNetwork\t10k-labels-copy.idx1-ubyte";
        FileStream ifsPixelst;
        FileStream ifsLabelst;
        BinaryReader brImagest;
        BinaryReader brLabelst;
        NN network;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Visible = false;
            network = new NN();
            Bitmap bmp = new Bitmap(168, 168);
            pictureBox1.Image = bmp;
            ifsPixelst = new FileStream(testpixelFile, FileMode.Open);
            ifsLabelst = new FileStream(testlabelFile, FileMode.Open);
            brImagest = new BinaryReader(ifsPixelst);
            brLabelst = new BinaryReader(ifsLabelst);
            for (int i=0; i<4; i++)
                brImagest.ReadInt32();
            brLabelst.ReadInt32();
            brLabelst.ReadInt32();
        }
        
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                g.FillEllipse(new SolidBrush(Color.Black), e.X, e.Y, 15, 15);
                g.Dispose();

                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void buttonClean_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(168, 168);
            pictureBox1.Image = bmp;
            textBoxSymbol.Text = "";
        }
        
        public int[,] ConvertBitMapToIntArray(Bitmap bitmap)
        {
            byte[] pixels = null;
            int[,] pic = new int[28, 28];
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                pixels = stream.ToArray();
                for (int i = 0; i < 28; i++)
                    for (int j = 0; j < 28; j++)
                        pic[i, j] = Convert.ToInt32(pixels[i * j]);
            }
            return pic;
        }

        public Image ResizeImg(Image b, int nWidth, int nHeight)
        {
            Image result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
                g.Dispose();
            }
            return result;
        }

        private void buttonRecognize_Click(object sender, EventArgs e)
        {
            int[,] pic = new int[28,28];
            Bitmap bmpSave = (Bitmap)ResizeImg(pictureBox1.Image, 28, 28);
            Bitmap output = new Bitmap(28, 28);
            for (int j = 0; j < 28; j++)
            {
                for (int i = 0; i < 28; i++)
                {
                    Color color = bmpSave.GetPixel(i, j);
                    int K = color.A;
                    pic[j, i] = color.A;
                    output.SetPixel(i, j, (K <= 50 ? Color.White : Color.Black));
                }
            }
            pictureBox1.Image = output;
            double symbol = network.GetAnswer(pic);
            textBoxSymbol.Text = Convert.ToString(symbol);
        }

        private void buttonLearn_Click(object sender, EventArgs e)
        {
            buttonTest.Enabled = false;
            buttonClean.Enabled = false;
            buttonRecognize.Enabled = false;
            buttonStatistics.Enabled = false;
            double speed = Convert.ToDouble(textBoxSpeed.Text);
            int numImages = 30000;
            pixels = new int[28, 28];
            FileStream ifsPixels = new FileStream(pixelFile, FileMode.Open);
            FileStream ifsLabels = new FileStream(labelFile, FileMode.Open);
            BinaryReader brImages = new BinaryReader(ifsPixels);
            BinaryReader brLabels = new BinaryReader(ifsLabels);
            for (int i=0; i<4; i++)
                brImages.ReadInt32();
            brLabels.ReadInt32();
            brLabels.ReadInt32();
            for (int k = 0; k < numImages; ++k)
            {
                for (int i = 0; i < 28; ++i) 
                {
                    for (int j = 0; j < 28; ++j)
                    {
                        byte b = brImages.ReadByte();
                        pixels[i, j] = Convert.ToInt32(b);
                    }
                }
                int lbl = Convert.ToInt32(brLabels.ReadByte());
                network.study(pixels, lbl, speed);
            }
            ifsPixels.Close(); brImages.Close();
            ifsLabels.Close(); brLabels.Close();
            buttonTest.Enabled = true;
            buttonClean.Enabled = true;
            buttonRecognize.Enabled = true;
            buttonStatistics.Enabled = true;
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            pixels = new int[28, 28];
            Bitmap bmp = new Bitmap(28, 28);
            for (int i = 0; i < 28; ++i)
            {
                for (int j = 0; j < 28; ++j)
                {
                    byte b = brImagest.ReadByte();
                    pixels[i, j] = Convert.ToInt32(b);
                    bmp.SetPixel(j, i, (pixels[i, j] <= 50 ? Color.White : Color.Black));
                }
            }
            int symbol = network.GetAnswer(pixels);
            pictureBox1.Image = bmp;
            textBoxSymbol.Text = Convert.ToString(symbol);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            brImagest.Close();
            brLabelst.Close();
            ifsPixelst.Close();
            ifsLabelst.Close();
        }

        private void buttonStatistics_Click(object sender, EventArgs e)
        {
            int numImages = 10000;
            int correct = 0;
            double prob;
            FileStream ifsPixelsTest = new FileStream(testpixelFile1, FileMode.Open);
            FileStream ifsLabelsTest = new FileStream(testlabelFile1, FileMode.Open);
            BinaryReader brImages = new BinaryReader(ifsPixelsTest);
            BinaryReader brLabels = new BinaryReader(ifsLabelsTest);
            for (int i = 0; i < 4; i++)
                brImages.ReadInt32();
            brLabels.ReadInt32();
            brLabels.ReadInt32();
            for (int k = 0; k < numImages; ++k)
            {
                for (int i = 0; i < 28; ++i)
                {
                    for (int j = 0; j < 28; ++j)
                    {
                        byte b = brImages.ReadByte();
                        pixels[i, j] = Convert.ToInt32(b);
                    }
                }
                int lbl = Convert.ToInt32(brLabels.ReadByte());
                int symbol = network.GetAnswer(pixels);
                if (symbol == lbl) correct++;
            } // по каждому изображению
            brImages.Close();
            brLabels.Close();
            ifsPixelsTest.Close();
            ifsLabelsTest.Close();
            prob = ((double)correct / (double)numImages) * 100;
            label2.Text = "Вероятность распознавания " + Convert.ToString(Math.Round(prob,2)) + "%";
            label2.Visible = true;
        }
    }
}

