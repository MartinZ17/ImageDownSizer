using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;


namespace ImageDownSizer1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedImagePath = openFileDialog.FileName;
                Image originalImage = Image.FromFile(selectedImagePath);
                pictureBoxOriginal.Image = originalImage;
            }
        }
        private void DownSize_Click(object sender, EventArgs e)
        {
            if (pictureBoxOriginal.Image != null)
            {
                if (!float.TryParse(downScallingFactor.Text, out float percentage) || percentage <= 0 || percentage > 100)
                {
                    MessageBox.Show("Please enter a valid downscaling percentage (1-100).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); 

                Image originalImage = pictureBoxOriginal.Image;
                int newWidth = (int)(originalImage.Width * percentage / 100);
                int newHeight = (int)(originalImage.Height * percentage / 100);

                Image downscaledImage = ResizeImage(originalImage, newWidth, newHeight);
                pictureBoxDownscaled.Image = downscaledImage;
                currentSize.Text = originalImage.Width.ToString() + "x" + originalImage.Height.ToString();
                newSize.Text = newWidth.ToString() + "x" + newHeight.ToString();

                stopwatch.Stop();
                MessageBox.Show($"Non-threaded resizing took {stopwatch.ElapsedMilliseconds} ms.", "Time Measurement");
            }


        }

        private Image ResizeImage(Image originalImage, int newWidth, int newHeight)
        {
            byte[,,] originalImageData = ConvertToColor(originalImage);
            byte[,,] resizedImageData = ImageResizer.ResizeImage(originalImageData, originalImage.Width, originalImage.Height, newWidth, newHeight);
            return CreateBitmapFrom3DArray(resizedImageData, newWidth, newHeight);
        }

        private byte[,,] ConvertToColor(Image image)
        {
            Bitmap bitmap = new Bitmap(image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[,,] colorImage = new byte[width, height, 3];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    colorImage[x, y, 0] = pixel.R; 
                    colorImage[x, y, 1] = pixel.G; 
                    colorImage[x, y, 2] = pixel.B; 
                }
            }

            return colorImage;
        }

        private Image CreateBitmapFrom3DArray(byte[,,] array, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = Color.FromArgb(array[x, y, 0], array[x, y, 1], array[x, y, 2]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }
        private void DownscaleThreads_Click_1(object sender, EventArgs e)
        {
            if (pictureBoxOriginal.Image != null)
            {
                if (!float.TryParse(downScallingFactor.Text, out float percentage) || percentage <= 0 || percentage > 100)
                {
                    MessageBox.Show("Please enter a valid downscaling percentage (1-100).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Perform resizing in a separate thread
                Thread resizingThread = new Thread(() =>
                {
                    Image originalImage = pictureBoxOriginal.Image;
                    int newWidth = (int)(originalImage.Width * percentage / 100);
                    int newHeight = (int)(originalImage.Height * percentage / 100);

                    Image downscaledImage = ResizeImage(originalImage, newWidth, newHeight);
                    SetImageOnMainThread(pictureBoxDownscaled, downscaledImage);

                    stopwatch.Stop(); 
                    MessageBox.Show($"Threaded resizing took {stopwatch.ElapsedMilliseconds} ms.", "Time Measurement");
                
                });
                resizingThread.Start();
            }
        }
        private void SetImageOnMainThread(PictureBox pictureBox, Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => pictureBox.Image = image));
            }
            else
            {
                pictureBox.Image = image;
            }
        }
    }
}