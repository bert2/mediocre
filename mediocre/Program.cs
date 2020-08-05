namespace Mediocre {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
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
            var screen = Screenshot.FromPrimaryScreen();

            var prevColor = Color.Black;
            var prevBright = 0;
            var delay = 1000 / opts.Fps;

            while (true) {
                screen.Refresh();

                var color = screen.GetAverageColor(opts.SampleStep);
                var bright = (int)Math.Round(Math.Clamp(color.GetBrightness() * 100, 1, 100));

                if (color != prevColor) await device
                    .SetRGBColor(color.R, color.G, color.B, opts.Smooth)
                    .Log($"setting {color}");

                if (bright != prevBright) await device
                    .SetBrightness(bright, opts.Smooth)
                    .Log($"setting brightness {bright}");

                prevColor = color;
                prevBright = bright;

                await Task.Delay(delay);
            }
        }

        private static Task<int> Print(PrintOpts opts) {
            var screen = Screenshot.FromPrimaryScreen();

            while (true) {
                screen.Refresh();
                var c = screen.GetAverageColor(opts.SampleStep);
                Console.WriteLine(c.ToRgb());
            }
        }

        private static Task<int> Screens(ScreensOpts opts) {
            Console.WriteLine(HeadingInfo.Default);
            Console.WriteLine();

            Screenshot.FromAll().ForEach(screen => {
                var isPrimary = screen.Screen?.Primary == true;
                if (isPrimary) Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.WriteLine($"{screen.Name} {(isPrimary ? "(primary)" : "")}");
                Console.WriteLine();
                Console.WriteLine($"  upper left    ({screen.Bounds.Top}, {screen.Bounds.Left})");
                Console.WriteLine($"  width         {screen.Bounds.Width} px");
                Console.WriteLine($"  height        {screen.Bounds.Height} px");
                Console.WriteLine();

                Console.ResetColor();
            });

            return Task.FromResult(0);
        }

        private static int ToRgb(this Color c) => (c.R << 16) | (c.G << 8) | c.B;

        private static string Print(this Color c) => $"({c.R}, {c.G}, {c.B})";

        private static string Print(this bool? b) => b == true ? "yes" : "no";

        private static void ForEach<T>(this IEnumerable<T> xs, Action<T> effect) {
            foreach (var x in xs)
                effect(x);
        }
    }
}