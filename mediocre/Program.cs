namespace Mediocre {
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    using CommandLine;
    using CommandLine.Text;

    using Mediocre.Prototype;

    public static class Program {
        public static async Task<int> Main(string[] args) => await Parser.Default
            .ParseArguments<SyncOpts, PrintOpts, ScreensOpts>(args)
            .MapResult(
                (SyncOpts opts) => Sync(opts),
                (PrintOpts opts) => Print(opts),
                (ScreensOpts opts) => Screens(opts),
                notParsedFunc: _ => Task.FromResult(1));

        private static async Task<int> Sync(SyncOpts opts) {
            Log.Verbose = opts.Verbose;
            Console.WriteLine(HeadingInfo.Default);

            var device = await Device.InitFirst(opts.Port);
            var screen = Screen.FromVirtualScreen();

            var prevColor = Color.Black;
            var prevBright = 0;
            var delay = 1000 / opts.Fps;

            while (true) {
                screen.Refresh();

                var color = screen.GetAverageColor(opts.SampleStep);
                var bright = (int)Math.Round(Math.Clamp(color.GetBrightness() * 100, 1, 100));

                if (color != prevColor) {
                    Log.Dbg($"setting color {color}");
                    await device.SetRGBColor(color.R, color.G, color.B, opts.Smooth).LogErr("set color on", device);
                }

                if (bright != prevBright) {
                    Log.Dbg($"setting brightness {bright}");
                    await device.SetBrightness(bright, opts.Smooth).LogErr("set brightness on", device);
                }

                prevColor = color;
                prevBright = bright;

                await Task.Delay(delay);
            }
        }

        private static Task<int> Print(PrintOpts opts) {
            var screen = Screen.FromVirtualScreen();

            while (true) {
                screen.Refresh();
                var c = screen.GetAverageColor(opts.SampleStep);
                Console.WriteLine(c.ToRgb());
            }
        }

        private static Task<int> Screens(ScreensOpts opts) {
            var virtScreen = Screen.FromVirtualScreen();
            Console.WriteLine("virtual screen:");
            Console.WriteLine($"  top/left\t{virtScreen.Bounds.Top}/{virtScreen.Bounds.Left}");
            Console.WriteLine($"  width\t\t{virtScreen.Bounds.Width} px");
            Console.WriteLine($"  height\t{virtScreen.Bounds.Height} px");
            return Task.FromResult(0);
        }

        private static int ToRgb(this Color c) => (c.R << 16) | (c.G << 8) | c.B;
    }
}