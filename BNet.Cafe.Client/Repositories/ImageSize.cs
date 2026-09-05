using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Repositories
{
    public class ImageSize
    {

        public async Task<MemoryStream> ReduceSize(MemoryStream ms, double imageScale)
        {
            if (imageScale >= 1.0)
            {
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }

            ms.Seek(0, SeekOrigin.Begin);

            using (var image = Image.FromStream(ms))
            {
                int newWidth = Math.Max(1, (int)(image.Width * imageScale));
                int newHeight = Math.Max(1, (int)(image.Height * imageScale));

                using (var thumbnail = new Bitmap(newWidth, newHeight))
                {
                    using (var g = Graphics.FromImage(thumbnail))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                        // HighQualityBilinear is visually fine and much faster than
                        // HighQualityBicubic for real-time remote desktop use.
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

                        g.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
                    }

                    // Allocate the output stream OUTSIDE the using blocks so it
                    // is NOT disposed before it reaches the caller.
                    var output = new MemoryStream();
                    thumbnail.Save(output, ImageFormat.Jpeg);
                    output.Position = 0;

                    await Task.CompletedTask;
                    return output;
                }
            }
        }
    }
}