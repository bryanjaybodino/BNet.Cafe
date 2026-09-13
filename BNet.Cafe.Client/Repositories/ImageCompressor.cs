using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Repositories
{
    /// <summary>
    /// Optimized JPEG compressor for remote desktop streaming.
    ///
    /// Recommended defaults:
    /// - Quality: 30
    /// - Scale:   0.8
    ///
    /// These settings preserve readable text while significantly reducing bandwidth.
    ///
    /// PERFORMANCE CHANGES:
    ///  • CompressInto(Bitmap, Stream, long, double) — writes the JPEG directly
    ///    into the caller-supplied stream with zero intermediate byte[] allocation.
    ///    Used by CaptureLoop to build the full WebSocket payload in a single pass.
    ///  • CompressImage() is kept for call sites that still need a MemoryStream.
    /// </summary>
    public class ImageCompressor
    {
        // Cache JPEG codec once for the lifetime of the app.
        private static readonly ImageCodecInfo JpegCodec = FindJpegCodec();

        private static ImageCodecInfo FindJpegCodec()
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                if (codec.MimeType == "image/jpeg") return codec;
            return null;
        }

        // ── Shared EncoderParameters factory ─────────────────────────────────

        private static EncoderParameters MakeEncoderParams(long quality)
        {
            var ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            return ep;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Compress and resize a bitmap, writing the resulting JPEG bytes
        /// directly into <paramref name="destination"/>.
        ///
        /// This is the preferred method in CaptureLoop: the caller pre-writes
        /// the frame header into the stream, then calls CompressInto so the
        /// JPEG lands in the same buffer — no intermediate byte[] copy needed.
        /// </summary>
        /// <param name="bmp">Source screen bitmap (not disposed here).</param>
        /// <param name="destination">Stream to write JPEG into.</param>
        /// <param name="quality">JPEG quality 1-100. Recommended: 25-40.</param>
        /// <param name="scale">Resize scale 0.1-1.0. Recommended: 0.7-0.9.</param>
        public void CompressInto(
            Bitmap bmp,
            Stream destination,
            long quality = 30L,
            double scale = 0.8)
        {
            quality = Math.Max(1L, Math.Min(100L, quality));
            scale = Math.Max(0.1, Math.Min(1.0, scale));

            int w = (int)(bmp.Width * scale);
            int h = (int)(bmp.Height * scale);

            using (Bitmap resized = ResizeImage(bmp, w, h))
            using (EncoderParameters ep = MakeEncoderParams(quality))
            {
                resized.Save(destination, JpegCodec, ep);
            }
        }

        /// <summary>
        /// Compress bitmap for streaming and return a MemoryStream.
        /// Kept for any call site that needs a standalone stream.
        /// </summary>
        public Task<MemoryStream> CompressImage(
            Bitmap bmp,
            long quality = 30L,
            double scale = 0.8)
        {
            quality = Math.Max(1L, Math.Min(100L, quality));
            scale = Math.Max(0.1, Math.Min(1.0, scale));

            int w = (int)(bmp.Width * scale);
            int h = (int)(bmp.Height * scale);

            using (Bitmap resized = ResizeImage(bmp, w, h))
            {
                var ms = new MemoryStream(w * h / 4);
                using (EncoderParameters ep = MakeEncoderParams(quality))
                    resized.Save(ms, JpegCodec, ep);
                ms.Position = 0;
                return Task.FromResult(ms);
            }
        }

        // ── Resize helper ─────────────────────────────────────────────────────

        /// <summary>
        /// Resize image with a balance of quality and speed suitable for
        /// real-time remote desktop streaming.
        /// </summary>
        private static Bitmap ResizeImage(Bitmap source, int width, int height)
        {
            var destination = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(destination))
            {
                // Low/Bilinear mode significantly cuts CPU usage during real-time scaling
                g.InterpolationMode = InterpolationMode.Low;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.DrawImage(source, 0, 0, width, height);
            }
            return destination;
        }
    }
}