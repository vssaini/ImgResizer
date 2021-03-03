using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ImgResizer.Properties;

namespace ImgResizer.Imaging
{
    /// <summary>
    /// This class contains helper functions for image resizing and manipulation
    /// </summary>
    public sealed class ImageUtilities
    {
        /// <summary>
        /// Resizes an image given its maximum size
        /// </summary>
        /// <param name="image">The image bytes</param>
        /// <param name="size">The maximum image size</param>
        /// <param name="maintainAspectRatio">Indicates whether to maintain the aspect ratio</param>
        /// <returns>The resized Image object</returns>
        public static Image ResizeImage(byte[] image, Size size, bool maintainAspectRatio)
        {
            // Create a memory stream from the image bytes
            using (MemoryStream ms = new MemoryStream(image))
            {
                // Read the stream into the image
                using (Image img = Image.FromStream(ms))
                {
                    // Resize and return the image
                    return ResizeImage(img, size, true);
                }
            }
        }

        /// <summary>
        /// Resizes an image given its size match with picturebox
        /// </summary>
        /// <param name="image">The image bytes</param>
        /// <param name="size">The maximum image size</param>
        /// <returns>The resized Image object</returns>
        public static Image ResizeImage(byte[] image, Size size)
        {
            // Create a memory stream from the image bytes
            using (MemoryStream ms = new MemoryStream(image))
            {
                // Read the stream into the image
                using (Image img = Image.FromStream(ms))
                {
                    return ResizeImage(img, size, img.Width < size.Width || img.Height < size.Height);
                }
            }
        }

        /// <summary>
        /// Resizes an image given its maximum size
        /// </summary>
        /// <param name="image">The image bytes</param>
        /// <param name="size">The maximum image size</param>
        /// <param name="maintainAspectRatio">Indicates whether to maintain the aspect ratio</param>
        /// <returns>The resized Image object</returns>
        public static Image ResizeImage(Image image, Size size, bool maintainAspectRatio)
        {
            // This size will hold the resized image size
            Size destinationSize = size;

            // If we should maintain the aspect ratio
            if (maintainAspectRatio)
            {
                // Calculate the new image size maintaining the aspect ratio
                int sourceWidth = image.Width;
                int sourceHeight = image.Height;

                if (sourceWidth < size.Width)
                    size.Width = sourceWidth;

                if (sourceHeight < size.Height)
                    size.Height = sourceHeight;

                var nPercentW = (size.Width / (float)sourceWidth);
                var nPercentH = (size.Height / (float)sourceHeight);

                var nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

                destinationSize.Width = (int)(sourceWidth * nPercent);
                destinationSize.Height = (int)(sourceHeight * nPercent);
            }

            // Create a new bitmap with the destination size
            Bitmap b = new Bitmap(destinationSize.Width, destinationSize.Height);
            var pen = new Pen(Brushes.DimGray, 2);

            // Create graphics from the image and draw the image
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, destinationSize.Width, destinationSize.Height);

            }

            pen.Dispose();

            // Return the resized image
            return b;
        }

        /// <summary>
        /// Applies a text watermark to an image
        /// </summary>
        /// <param name="image">The source image</param>
        /// <param name="watermarkText">The watermark text</param>
        /// <param name="textFont">The text's font</param>
        /// <param name="textColor">The text's color</param>
        /// <param name="alignment">The alignment of the watermark</param>
        /// <param name="offsetX">The X axis offset relative to the current position in pixels</param>
        /// <param name="offsetY">The Y axis offset relative to the current position in pixels</param>
        /// <param name="rotationDegree">The rotation in degrees</param>
        /// <param name="opacity">The watermark opacity (percentage)</param>
        /// <returns>The watermarked image</returns>
        public static Image ApplyWatermark(Image image, string watermarkText, Font textFont, Color textColor, Alignment alignment, int offsetX, int offsetY, float rotationDegree, int opacity)
        {            
            // Create a graphics object on the image
            using (Graphics g = Graphics.FromImage(image))
            {                
                // Set the color and brush
                Color color = Color.FromArgb((opacity * 255) / 100, textColor);
                Brush brush = new SolidBrush(color);                    

                // Mesure the text's size
                SizeF textSize = g.MeasureString(watermarkText, textFont);

                // Calculate the anchor point
                PointF anchorPoint = CalculateWatermarkAnchorPoint(image, alignment, offsetX, offsetY, textSize);

                // Set graphics options
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Rotate the graphics by the specified rotation degrees
                RotateGraphics(g, rotationDegree, textSize, anchorPoint);

                // Draw the string on the image
                g.DrawString(watermarkText, textFont, brush, anchorPoint);                
            }

            // Return the watermarked image
            return image;
        }

        /// <summary>
        /// Applies an image watermark to another image
        /// </summary>
        /// <param name="image">The source image</param>
        /// <param name="watermarkImage">The watermark image</param>
        /// <param name="alignment">The alignment of the watermark</param>
        /// <param name="offsetX">The X axis offset relative to the current position in pixels</param>
        /// <param name="offsetY">The Y axis offset relative to the current position in pixels</param>
        /// <param name="rotationDegree">The rotation in degrees</param>
        /// <param name="opacity">The watermark opacity (percentage)</param>
        /// <returns>The watermarked image</returns>
        public static Image ApplyWatermark(Image image, Image watermarkImage, Alignment alignment, int offsetX, int offsetY, float rotationDegree, int opacity)
        {
            // Create a graphics object on the image
            using (Graphics g = Graphics.FromImage(image))
            {
                // Calculate the anchor point
                PointF anchorPoint = CalculateWatermarkAnchorPoint(image, alignment, offsetX, offsetY, watermarkImage.Size);         

                // Apply opacity to the watermark image
                ImageAttributes attributes = new ImageAttributes();
                ColorMap colorMap = new ColorMap();

                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                ColorMap[] remapTable = { colorMap };

                attributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                float[][] colorMatrixElements = { 
                   new[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                   new[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                   new[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                   new[] {0.0f,  0.0f,  0.0f,  (opacity/100.0f)*1.0f, 0.0f},
                   new[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                };

                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                attributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // Set graphics options
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Rotate the graphics by the specified rotation degrees
                RotateGraphics(g, rotationDegree, watermarkImage.Size, anchorPoint);

                // Draw the watermark image on the source image
                g.DrawImage(watermarkImage,
                    new Rectangle((int)anchorPoint.X, (int)anchorPoint.Y, watermarkImage.Width, watermarkImage.Height),
                    0, 
                    0,
                    watermarkImage.Width,
                    watermarkImage.Height,
                    GraphicsUnit.Pixel, 
                    attributes);
            }

            // Return the watermarked photo
            return image;
        }

        /// <summary>
        /// Calculates the point on which the watermark image should be anchored within the source image
        /// </summary>
        /// <param name="image">The source image</param>
        /// <param name="alignment">The image alignment</param>
        /// <param name="offsetX">The X axis offset relative to the current position in pixels</param>
        /// <param name="offsetY">The Y axis offset relative to the current position in pixels</param>        
        /// <param name="size">The size of the watermark (text or image)</param>
        /// <returns>The point on which the watermark image should be anchored within the source image</returns>
        private static PointF CalculateWatermarkAnchorPoint(Image image, Alignment alignment, int offsetX, int offsetY, SizeF size)
        {
            // Init anchor point
            PointF anchorPoint = new Point(0, 0);

            // Set the X coordinate of the anchor point based on the horizontal alignment
            switch (alignment.HorizontalAlignment)
            {
                case HorizontalAlignment.Left: anchorPoint.X = 0; break;
                case HorizontalAlignment.Center: anchorPoint.X = (image.Width - size.Width) / 2.0f; break;
                case HorizontalAlignment.Right: anchorPoint.X = image.Width - size.Width; break;
            }

            // Set the Y coordinate of the anchor point based on the vertical alignment
            switch (alignment.VerticalAlignment)
            {
                case VerticalAlignment.Top: anchorPoint.Y = 0; break;
                case VerticalAlignment.Middle: anchorPoint.Y = (image.Height - size.Height) / 2.0f; break;
                case VerticalAlignment.Bottom: anchorPoint.Y = image.Height - size.Height; break;
            }

            // Apply the X and Y offsets
            anchorPoint.X += offsetX;
            anchorPoint.Y += offsetY;

            return anchorPoint;
        }

        /// <summary>
        /// Rotates the graphics by the specified rotation degrees.
        /// <remarks>This will rotate the watermark around itself from a point in its middle</remarks>
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="rotationDegree">The roation in degrees</param>
        /// <param name="size">The size of the watermark</param>
        /// <param name="anchorPoint">The anchor point of the watermark</param>
        private static void RotateGraphics(Graphics g, float rotationDegree, SizeF size, PointF anchorPoint)
        {
            // Create a new matrix
            Matrix m = new Matrix();

            // Rotate the matrix around a point in the middle of the watermark box
            m.RotateAt(rotationDegree, new PointF(anchorPoint.X + (size.Width / 2.0f), anchorPoint.Y + (size.Height / 2.0f)));

            // Apply matrix transformation
            g.Transform = m;
        }

        /// <summary>
        /// Saves the image in the desired format and quality and returns the image bytes
        /// </summary>
        /// <param name="image">The image to be saved</param>
        /// <param name="format">The photo format</param>
        /// <param name="quality">The photo quality</param>        
        /// <returns>The saved image bytes</returns>
        public static byte[] GetImageBytes(Image image, PhotoFormat format, long quality)
        {
            if (image == null)
                return null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the image in the desired format and quality
                SaveImageToStream(memoryStream, image, format, quality);             
                
                // Convert the memory stream to an array and return it
                return memoryStream.ToArray();                
            }
        }

        /// <summary>
        /// Saves the image in the desired format and quality to a file
        /// </summary>
        /// <param name="image">The image to be saved</param>
        /// <param name="filename">The destination filename</param>
        /// <param name="format">The photo format</param>
        /// <param name="quality">The photo quality</param>                
        public static void SaveImageToFile(Image image, string filename, PhotoFormat format, long quality)
        {
            // Create the file and get the file stream
            using (FileStream fileStream = new FileStream(filename, FileMode.Create))
            {
                // Save the image in the desired format and quality to the stream
                SaveImageToStream(fileStream, image, format, quality);
            }
        }

        /// <summary>
        /// Saves an image to a stream based on the desired format and quality
        /// </summary>
        /// <param name="stream">The stream to write the image to</param>
        /// <param name="image">The image to be saved</param>
        /// <param name="format">The photo format</param>
        /// <param name="quality">The photo quality</param>                
        private static void SaveImageToStream(Stream stream, Image image, PhotoFormat format, long quality)
        {
            // Get the image codec info based on the format
            ImageCodecInfo imageCodecInfo = GetImageCodecInfo(format);

            // Get the encoder parameters for the quality parameter
            EncoderParameters encoderParams = GetEncoderParameters(quality);

            // Create a memory stream and save the image to it            
            image.Save(stream, imageCodecInfo, encoderParams);
        }

        /// <summary>
        /// Returns an array of encoder parameters containing the quality encoder parameter set
        /// </summary>
        /// <param name="quality">The image quality</param>
        /// <returns>The encoder parameters array</returns>
        private static EncoderParameters GetEncoderParameters(long quality)
        {
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            return encoderParams;
        }

        /// <summary>
        /// Gets the image codec info related to the passed photo format
        /// </summary>
        /// <param name="photoFormat">The photo format</param>
        /// <returns>The image codec info related to the passed photo format</returns>
        private static ImageCodecInfo GetImageCodecInfo(PhotoFormat photoFormat)
        {
            // Get the corresponding image format based on the photo format
            ImageFormat imageFormat = GetImageFormat(photoFormat);

            // Loop within all image encoders
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                // Return the one with the same GUID
                if (codec.FormatID.Equals(imageFormat.Guid))
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an ImageFormat object corresponding to the passed photo format
        /// </summary>
        /// <param name="format">The photo format</param>
        /// <returns>The corresponding ImageFormat object</returns>
        private static ImageFormat GetImageFormat(PhotoFormat format)
        {
            switch (format)
            {
                case PhotoFormat.Bmp:
                    return ImageFormat.Bmp;
                case PhotoFormat.Emf:
                    return ImageFormat.Emf;
                case PhotoFormat.Exif:
                    return ImageFormat.Exif;
                case PhotoFormat.Gif:
                    return ImageFormat.Gif;
                case PhotoFormat.Icon:
                    return ImageFormat.Icon;
                case PhotoFormat.Jpeg:
                    return ImageFormat.Jpeg;
                case PhotoFormat.Png:
                    return ImageFormat.Png;
                case PhotoFormat.Tiff:
                    return ImageFormat.Tiff;
                case PhotoFormat.Wmf:
                    return ImageFormat.Wmf;
                default:
                    throw new Exception("Unsupported photo format");
            }
        }

        /// <summary>
        /// Set trial watermark on image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image SetTrialWatermark(Image image)
        {
            // Apply text watermark to photo
            return ApplyWatermark(image,
                    "TRIAL - VERSION",
                    Settings.Default.WatermarkFont,
                    Color.White,
                    new Alignment(HorizontalAlignment.Center, VerticalAlignment.Middle), 
                    Settings.Default.WatermarkOffsetX,
                    Settings.Default.WatermarkOffsetY,
                    Settings.Default.WatermarkRotation,
                    Settings.Default.WatermarkOpacity);
        }
    }
}
