namespace Mediocre {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using CommandLine;
    using CommandLine.Text;

    using Mediocre.Prototype;

    public static class Program {
        public static async Task<int> Main(string[] args) => await new Parser()
            .ParseArguments<SyncOpts, PrintOpts, ReadOpts, ListOpts>(args)
            .MapResult(
                parsedSync:  Sync,
                parsedPrint: Print,
                parsedRead:  Read,
                parsedList:  List,
                notParsed:   ShowHelp);

        private static async Task<int> Sync(SyncOpts opts) {
            Log.Verbose = opts.Verbose;
            Console.WriteLine(HeadingInfo.Default);
            Console.WriteLine();

            var screenshot = Screenshot.FromScreenName(opts.Screen);
            Log.Dbg($"selected screen {screenshot.ScreenName}.");

            var device = await Device.InitFirst(opts.Port);
            Log.Msg($"syncing average color of {screenshot.ScreenName} with {device}.");

            Color? prevColor = null;
            var prevBright = 0;
            var delay = 1000 / opts.Fps;

            while (true) {
                screenshot.Refresh();

                var color = screenshot.GetAverageColor(opts.SampleStep);
                var bright = color.GetBrightness().Scale(1, 100);

                if (color != prevColor) await device
                    .SetRGBColor(color.R, color.G, color.B, opts.Smooth)
                    .Log($"setting {color}.");

                if (bright != prevBright) await device
                    .SetBrightness(bright, opts.Smooth)
                    .Log($"setting brightness {bright}.");

                prevColor = color;
                prevBright = bright;

                await Task.Delay(delay);
            }
        }

        private static async Task<int> Print(PrintOpts opts) {
            var screen = Screenshot.FromScreenName(opts.Screen);

            Color? prevColor = null;
            var delay = 1000 / opts.Fps;

            while (true) {
                screen.Refresh();

                var color = screen.GetAverageColor(opts.SampleStep);
                if (color != prevColor)
                    Console.WriteLine(color.ToRgb());

                prevColor = color;

                await Task.Delay(delay);
            }
        }

        private static Task<int> Read(ReadOpts opts) {
            return Task.FromResult(0);
        }

        private static Task<int> List(ListOpts opts) => opts.What switch {
            ListType.screens => ListScreens(opts.Filter),
            ListType.devices => ListScreens(opts.Filter),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Task<int> ListScreens(string? filter) {
            Console.WriteLine(HeadingInfo.Default);
            Console.WriteLine();

            foreach (var screen in Screenshot.ListAll(filter)) {
                var isPrimary = screen.Screen?.Primary == true;
                if (isPrimary) Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.WriteLine($"{screen.ScreenName} {(isPrimary ? "(primary)" : "")}");
                Console.WriteLine();
                Console.WriteLine($"  upper left:           ({screen.Bounds.Top}, {screen.Bounds.Left})");
                Console.WriteLine($"  width:                {screen.Bounds.Width} px");
                Console.WriteLine($"  height:               {screen.Bounds.Height} px");
                Console.WriteLine();

                Console.ResetColor();
            }

            return Task.FromResult(0);
        }

        private static Task<int> ShowHelp(NotParsed<object> result) {
            Console.WriteLine(HelpText.AutoBuild(
                result,
                helpText => helpText
                    .With(ht => ht.Copyright = "")
                    .With(ht => ht.AdditionalNewLineAfterOption = false)
                    .With(ht => ht.AddEnumValuesToHelpText = true)));
            return Task.FromResult(1);
        }

        private static async Task<int> MapResult(
            this ParserResult<object> result,
            Func<SyncOpts, Task<int>> parsedSync,
            Func<PrintOpts, Task<int>> parsedPrint,
            Func<ReadOpts, Task<int>> parsedRead,
            Func<ListOpts, Task<int>> parsedList,
            Func<NotParsed<object>, Task<int>> notParsed)
            => await result.MapResult(
                parsedSync,
                parsedPrint,
                parsedRead,
                parsedList,
                notParsedFunc: _ => notParsed((NotParsed<object>)result));

        private static int Scale(this float x, int min, int max, int? factor = null)
            => (int)Math.Round(Math.Clamp(x * (factor ?? max), min, max));

        private static int ToRgb(this Color c) => (c.R << 16) | (c.G << 8) | c.B;

        private static string Print(this Color c) => $"({c.R}, {c.G}, {c.B})";

        private static string Print(this bool? b) => b == true ? "yes" : "no";

        private static void ForEach<T>(this IEnumerable<T> xs, Action<T> effect) {
            foreach (var x in xs)
                effect(x);
        }

        private static T With<T>(this T x, Action<T> effect) {
            effect(x);
            return x;
        }
    }
}