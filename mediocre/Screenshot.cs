namespace Mediocre {
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class Screenshot {
        public readonly string Name;
        public readonly Rectangle Bounds;
        public readonly Screen? Screen;
        public readonly Bitmap Image;
        public readonly Graphics Gfx;

        public static Screenshot FromPrimaryScreen() => new Screenshot();

        public static Screenshot FromVirtualScreen() => new Screenshot(
            name:   "(virtual screen)",
            top:    GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN),
            left:   GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN),
            width:  GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN),
            height: GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN));

        public static IEnumerable<Screenshot> FromAll() => Screen
            .AllScreens
            .Select(s => new Screenshot(s))
            .Append(FromVirtualScreen());

        public Screenshot() : this(Screen.PrimaryScreen) { }

        public Screenshot(Screen screen) : this(screen.DeviceName, screen.Bounds, screen) { }

        public Screenshot(string name, int top, int left, int width, int height)
            : this(name, new Point(x: left, y: top), new Size(width, height)) { }

        public Screenshot(string name, Point upperLeft, Size size) : this(name, new Rectangle(upperLeft, size)) { }

        public Screenshot(string name, Rectangle bounds, Screen? screen = null) {
            Name = name;
            Bounds = bounds;
            Screen = screen;
            Image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            Gfx = Graphics.FromImage(Image);
            Refresh();
        }

        public void Refresh() => Gfx.CopyFromScreen(Bounds.Location, new Point(0, 0), Bounds.Size);

        public unsafe Color GetAverageColor(int sampleStep) {
            var data = Image.LockBits(
                new Rectangle(Point.Empty, Image.Size),
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

            Image.UnlockBits(data);

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
