using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class FileImageHelper
    {
        bool IsImage(byte[] bytes)
        {
            if (bytes.Length < 12)
                return false;

            // JPEG (FF D8 FF)
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return true;

            // PNG (89 50 4E 47 0D 0A 1A 0A)
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E &&
                bytes[3] == 0x47 && bytes[4] == 0x0D && bytes[5] == 0x0A &&
                bytes[6] == 0x1A && bytes[7] == 0x0A)
                return true;

            // GIF (47 49 46 38)
            if (bytes[0] == 0x47 && bytes[1] == 0x49 &&
                bytes[2] == 0x46 && bytes[3] == 0x38)
                return true;

            // BMP (42 4D)
            if (bytes[0] == 0x42 && bytes[1] == 0x4D)
                return true;

            // TIFF (little endian: 49 49 2A 00)
            if (bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00)
                return true;

            // TIFF (big endian: 4D 4D 00 2A)
            if (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A)
                return true;

            // WEBP (52 49 46 46 .... 57 45 42 50)
            if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                return true;

            // HEIC/HEIF (ftypheic, ftypheix, ftypmif1, ftypmsf1)
            if (bytes.Length >= 12)
            {
                string ftyp = System.Text.Encoding.ASCII.GetString(bytes, 4, 8);
                if (ftyp.Contains("heic") ||
                    ftyp.Contains("heix") ||
                    ftyp.Contains("mif1") ||
                    ftyp.Contains("msf1"))
                    return true;
            }

            // ICO (00 00 01 00)
            if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0x01 && bytes[3] == 0x00)
                return true;

            return false;
        }

        public byte[] CompressImageByteQuality(byte[] imageBytes, long quality = 75L)
        {
            try
            {
                if (IsImage(imageBytes) == false)
                {
                    return imageBytes;
                }
                using (var inputStream = new MemoryStream(imageBytes))
                using (var originalImage = System.Drawing.Image.FromStream(inputStream))
                {
                    // Get encoder (use JPEG or PNG depending on what you want)
                    var encoder = GetEncoderInfo("image/jpeg");
                    if (encoder == null)
                        throw new Exception("Encoder not found.");

                    var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                    encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, quality);

                    // Resize (same logic as your code)
                    double scaleFactor = 0.8;
                    int newWidth = (int)(originalImage.Width * scaleFactor);
                    int newHeight = (int)(originalImage.Height * scaleFactor);
                    var resizedBitmap = new Bitmap(newWidth, newHeight);

                    using (var g = Graphics.FromImage(resizedBitmap))
                    {
                        // Fill background white to prevent black transparency issue
                        g.Clear(Color.White);

                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, new Rectangle(0, 0, newWidth, newHeight));
                    }

                    // Return compressed bytes
                    using (var ms = new MemoryStream())
                    {
                        resizedBitmap.Save(ms, encoder, encoderParams);
                        return ms.ToArray();
                    }
                }
            }
            catch
            {
                return imageBytes;
            }
        }

        public void CompressImageQuality(System.Drawing.Image image, string targetPath, long quality = 75L)
        {
            var encoder = GetEncoderInfo("image/png");
            if (encoder == null)
            {
                // Handle the case where the encoder is not found.
                throw new Exception("PNG encoder not found.");
            }

            var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
            encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            double scaleFactor = 0.8; // Scale factor for width
            var newWidth = (int)(image.Width * scaleFactor);
            var newHeight = (int)(image.Height * scaleFactor);
            var thumbnailImg = new System.Drawing.Bitmap(newWidth, newHeight);

            using (var thumbGraph = System.Drawing.Graphics.FromImage(thumbnailImg))
            {
                thumbGraph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                var imageRectangle = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
            }

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            thumbnailImg.Save(targetPath, encoder, encoderParameters);
        }

        public byte[] ScaleImageToBytes(byte[] imageBytes, int maxHeight, bool sameHeight = false)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                using (var img = System.Drawing.Image.FromStream(ms))
                {
                    using (var scaled = ScaleImage(img, maxHeight, sameHeight))
                    {
                        using (var outStream = new MemoryStream())
                        {
                            // Use PNG (recommended) or img.RawFormat if you want original format
                            scaled.Save(outStream, img.RawFormat);
                            return outStream.ToArray();
                        }
                    }
                }
            }
        }

        public System.Drawing.Image ScaleImage(byte[] imageBytes, int maxHeight, bool sameHeight = false)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                using (var img = System.Drawing.Image.FromStream(ms))
                {
                    return ScaleImage(img, maxHeight, sameHeight);
                }
            }
        }
        public System.Drawing.Image ScaleImage(System.Drawing.Image image, int maxHeight, bool sameHeight = false)
        {
            var ratio = (double)maxHeight / image.Height;
            var newWidth = (int)(image.Width * ratio);
            if (sameHeight)
            {
                newWidth = maxHeight;
            }



            var newHeight = (int)(image.Height * ratio);
            var newImage = new System.Drawing.Bitmap(newWidth, newHeight);
            using (var g = System.Drawing.Graphics.FromImage(newImage))
            {
                g.Clear(System.Drawing.Color.White);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get all codecs that match the mime type
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the codec with the specified mime type
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }
            return null;
        }


        public Bitmap ConvertToDarkMode(Bitmap original)
        {
            Bitmap newImage = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixelColor = original.GetPixel(x, y);

                    // Invert color for a basic "dark mode" effect
                    Color invertedColor = Color.FromArgb(pixelColor.A,
                        255 - pixelColor.R,
                        255 - pixelColor.G,
                        255 - pixelColor.B);

                    newImage.SetPixel(x, y, invertedColor);
                }
            }

            return newImage;
        }
        public Bitmap ConvertBlackWhiteOnly(Bitmap original, int blackThreshold = 100, int whiteThreshold = 150)
        {
            Bitmap newImage = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixelColor = original.GetPixel(x, y);
                    int r = pixelColor.R;
                    int g = pixelColor.G;
                    int b = pixelColor.B;

                    bool isBlack = (r <= blackThreshold && g <= blackThreshold && b <= blackThreshold);
                    bool isWhite = (r >= whiteThreshold && g >= whiteThreshold && b >= whiteThreshold);

                    if (isWhite)
                    {
                        newImage.SetPixel(x, y, Color.Transparent);
                    }
                    else if (isBlack)
                    {
                        // The darker the pixel, the closer to off-white (not pure 255)
                        // Map: 0 (pure black) → 230 (off-white), blackThreshold → stays near its brightness
                        float darkness = 1f - ((r + g + b) / 3f / blackThreshold); // 0.0 = near threshold, 1.0 = pure black
                        int softWhite = (int)(200 + darkness * 30); // Range: 200 (less dark) → 230 (pure black)

                        newImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, softWhite, softWhite, softWhite));
                    }
                    else
                    {
                        newImage.SetPixel(x, y, pixelColor);
                    }
                }
            }
            return newImage;
        }
    }
}