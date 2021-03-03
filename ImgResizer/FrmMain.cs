using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ImgResizer.Imaging;
using ImgResizer.Properties;

namespace ImgResizer
{
    public partial class FrmMain : Form
    {
        private readonly string _tempPath;

        public FrmMain()
        {
            InitializeComponent();

            txtImagePath.Text = @"E:\Users\V.S. Saini\Documents\AD Photos\Photos Samples\Obama.jpg";
            _tempPath = new StringBuilder(Path.GetTempPath()).Append(@"AD Photos\").ToString();
            var dirInfo = new DirectoryInfo(_tempPath);

            if (!dirInfo.Exists)
                dirInfo.Create();
            else
            {
                foreach (var file in dirInfo.GetFiles())
                    File.Delete(file.FullName);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (!openFileDialog.ShowDialog().Equals(DialogResult.OK)) return;

            var imgPath = txtImagePath.Text = openFileDialog.FileName;
            var originalImg = Image.FromFile(imgPath);
            pbUserSelected.Image = originalImg;

            //ResizeImage(imgPath, newImgPath, pbPreview.Width, pbPreview.Height, false);

            using (var ms = new MemoryStream(ResizeImage(imgPath)))
            {
                pbPreview.Image = Image.FromStream(ms);
            }
        }

        /// <summary>
        /// Return file path.
        /// </summary>
        public string GetFileName()
        {
            var date = DateTime.Now;
            var builder = new StringBuilder(_tempPath);

            builder.Append(Guid.NewGuid().ToString("N")).Append(date.Day).Append(date.Month).Append(date.Year).Append(date.Hour).Append(date.Minute).Append(date.Second).Append(date.Millisecond).Append(".png");

            return builder.ToString();
        }

        //private static void ResizeImage(string originalFile, string newFile, int newWidth, int maxHeight, bool onlyResizeIfWider)
        //{
        //    var fullsizeImage = Image.FromFile(originalFile);

        //    // Prevent using images internal thumbnail
        //    fullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
        //    fullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

        //    if (onlyResizeIfWider && fullsizeImage.Width <= newWidth)
        //        newWidth = fullsizeImage.Width;

        //    var newHeight = fullsizeImage.Height * newWidth / fullsizeImage.Width;
        //    if (newHeight > maxHeight)
        //    {
        //        // Resize with height instead
        //        //newWidth = fullsizeImage.Width * maxHeight / fullsizeImage.Height;
        //        newHeight = maxHeight;
        //    }

        //    var newImage = fullsizeImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);

        //    // Clear handle to original file so that we can overwrite it if necessary
        //    fullsizeImage.Dispose();

        //    // Save resized picture
        //    newImage.Save(newFile);
        //}

        private byte[] ResizeImage(string fileName)
        {
            byte[] rImgBytes = { };

            try
            {
                using (var originalImg = Image.FromFile(fileName))
                {
                    // Dispose the previous image
                    if (pbPreview.Image != null)
                    {
                        pbPreview.Image.Dispose();
                        pbPreview.Image = null;
                    }

                    // Resize the image and set it in the picture box
                    var pbSize = new Size(pbPreview.Width, pbPreview.Height);

                    var resizedImg = ImageUtilities.ResizeImage(originalImg, pbSize, true);
                    
                    using (var g = Graphics.FromImage(resizedImg))
                    {
                        using (var pen = new Pen(Brushes.DimGray, 2))
                        {
                            g.DrawRectangle(pen, new Rectangle(0, 0, resizedImg.Width, resizedImg.Height));
                        }
                    }

                    if (Settings.Default.IsTrialVersion)
                        resizedImg = ImageUtilities.SetTrialWatermark(resizedImg);

                    var imgBytes = ImageUtilities.GetImageBytes(originalImg, PhotoFormat.Jpeg, Settings.Default.ImageQuality);
                    rImgBytes = ImageUtilities.GetImageBytes(resizedImg, PhotoFormat.Jpeg, 100);

                    lblStatus.Text = string.Format("File size {0}Kb ({2}x{3}px), On saving {1}Kb ({4}x{5}px)",
                        new FileInfo(fileName).Length / 1024, imgBytes.Length / 1024, originalImg.Width, originalImg.Height, resizedImg.Width, resizedImg.Height);
                }
            }
            catch
            {
                // Show error message
                MessageBox.Show(this, "Could not load image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return rImgBytes;
        }
    }
}