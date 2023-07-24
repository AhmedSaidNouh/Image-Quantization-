using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            Console.WriteLine("time at start : " + DateTime.Now.Minute.ToString() + " :" + DateTime.Now.Second.ToString());
            string message = ImageOperations.DistinctColors(ImageMatrix).Count.ToString();
            MessageBox.Show( message,"# of distinct colors: ");
            
            ImageOperations.MST();
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageOperations.Cluster(maskSize);
            MessageBox.Show(ImageOperations.clusters.Count.ToString(), "# of clusters colors: ");
            ImageOperations.PalletGereration();
            ImageOperations.ImageQuantization(ref ImageMatrix);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);

            Console.WriteLine("time at end : " + DateTime.Now.Minute.ToString() + " :" + DateTime.Now.Second.ToString());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void nudMaskSize_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}