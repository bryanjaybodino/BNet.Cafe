using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace BNet.Cafe.Server.Services
{
    public class CodeGenerator
    {
        public string Ellipsis(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }
        public void Barcode(string Data, System.Web.UI.WebControls.Image image)
        {
            BarcodeWriter writer = new BarcodeWriter()
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 80,
                    Width = 800,
                    PureBarcode = false,
                    Margin = 8,
                },
            };
            var bitmap = writer.Write(Data);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            string base64ImageRepresentation = "data:image/png;base64," + Convert.ToBase64String(byteImage);
            image.ImageUrl = base64ImageRepresentation;
        }

        public void Barcode(string Data, string imageName)
        {
            try
            {
                string SavePath = HttpContext.Current.Server.MapPath("~/temp_files/Barcodes");
                BarcodeWriter writer = new BarcodeWriter()
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 80,
                        Width = 800,
                        PureBarcode = false,
                        Margin = 8,

                    },
                };
                var bitmap = writer.Write(Data);
                bitmap.Save(SavePath + "/" + imageName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch { }
        }



        public void QRCode(string Data, string imageName, string logoPath = null)
        {
            try
            {
                string SavePath = HttpContext.Current.Server.MapPath("~/temp_files/QRCodes");

                BarcodeWriter writer = new BarcodeWriter()
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = 300,
                        Width = 300,
                        Margin = 0,
                        ErrorCorrection = string.IsNullOrEmpty(logoPath)
                            ? ErrorCorrectionLevel.L
                            : ErrorCorrectionLevel.H
                    },
                };

                using (Bitmap bitmap = writer.Write(Data))
                {
                    // ✅ Add logo if provided
                    if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        using (Bitmap logo = (Bitmap)Bitmap.FromFile(logoPath))
                        {
                            int logoSize = bitmap.Width / 5; // 20%
                            int x = (bitmap.Width - logoSize) / 2;
                            int y = (bitmap.Height - logoSize) / 2;

                            int padding = 5;

                            // White background for better scan
                            g.FillRectangle(Brushes.White,
                                x - padding,
                                y - padding,
                                logoSize + padding * 2,
                                logoSize + padding * 2);

                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            g.DrawImage(logo, new Rectangle(x, y, logoSize, logoSize));
                        }
                    }

                    // ✅ Save final image
                    bitmap.Save(
                        Path.Combine(SavePath, imageName + ".png"),
                        ImageFormat.Png
                    );
                }
            }
            catch
            {
                // silent catch (same as your style)
            }
        }


        public void QRCode(string data, System.Web.UI.WebControls.Image image, string logoPath = null)
        {
            var options = new QrCodeEncodingOptions
            {
                Height = 300,
                Width = 300,
                Margin = 0,
                ErrorCorrection = string.IsNullOrEmpty(logoPath)
                    ? ErrorCorrectionLevel.L
                    : ErrorCorrectionLevel.H
            };

            BarcodeWriter writer = new BarcodeWriter()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            using (Bitmap qrBitmap = writer.Write(data))
            {
                // ✅ Draw logo if provided
                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    using (Graphics g = Graphics.FromImage(qrBitmap))
                    using (Bitmap logo = (Bitmap)Bitmap.FromFile(logoPath))
                    {
                        int logoSize = qrBitmap.Width / 5;
                        int x = (qrBitmap.Width - logoSize) / 2;
                        int y = (qrBitmap.Height - logoSize) / 2;

                        int padding = 5;
                        g.FillRectangle(Brushes.White,
                            x - padding,
                            y - padding,
                            logoSize + padding * 2,
                            logoSize + padding * 2);

                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(logo, new Rectangle(x, y, logoSize, logoSize));
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    qrBitmap.Save(ms, ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    string base64 = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                    image.ImageUrl = base64;
                }
            }
        }


        public string GetQRCode(string Id)
        {
            return HttpContext.Current.Server.MapPath("temp_files/QRCodes/" + Id + ".png");
        }
        public string GetBarcode(string Id)
        {
            return HttpContext.Current.Server.MapPath("temp_files/Barcodes/" + Id + ".png");
        }
    }
}