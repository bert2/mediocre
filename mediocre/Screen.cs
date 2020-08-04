namespace Mediocre {
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    public class Screen {
        private readonly Bitmap screen;
        private readonly Graphics screenGfx;

        public Rectangle Bounds { get; }

        public Screen() : this(
            top:    GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN),
            left:   GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN),
            width:  GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN),
            height: GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN))
        { }

        public Screen(int top, int left, int width, int height)
            : this(new Point(x: top, y: left), new Size(width, height))
        { }

        public Screen(Point upperLeft, Size size) : this(new Rectangle(upperLeft, size)) { }

        public Screen(Rectangle bounds) {
            Bounds = bounds;
            screen = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            screenGfx = Graphics.FromImage(screen);
            Refresh();
        }

        public static Screen FromVirtualScreen() => new Screen();

        public void Refresh() => screenGfx.CopyFromScreen(Bounds.Location, new Point(0, 0), Bounds.Size);

        public unsafe Color GetAverageColor(int sampleStep) {
            var data = screen.LockBits(
                new Rectangle(Point.Empty, screen.Size),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var row = (int*)data.Scan0.ToPointer();
            var (sumR, sumG, sumB) = (0L, 0L, 0L);
            var stride = data.Stride / sizeof(int) * sampleStep;

            for (var y = 0; y < data.Height; y += sampleStep) {
                for (var x = 0; x < data.Width; x += sampleStep) {
                    var argb = row[x];
                    sumR += (argb & 0x00FF0000) >> 16;
                    sumG += (argb & 0x0000FF00) >> 8;
                    sumB += argb & 0x000000FF;
                }
                row += stride;
            }

            screen.UnlockBits(data);

            var numSamples = data.Width / sampleStep * data.Height / sampleStep;
            var avgR = sumR / numSamples;
            var avgG = sumG / numSamples;
            var avgB = sumB / numSamples;
            return Color.FromArgb((int)avgR, (int)avgG, (int)avgB);
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric metric);

        private enum SystemMetric {
            SM_XVIRTUALSCREEN = 76,
            SM_YVIRTUALSCREEN = 77,
            SM_CXVIRTUALSCREEN = 78,
            SM_CYVIRTUALSCREEN = 79
        }
    }
}
