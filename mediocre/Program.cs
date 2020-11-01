namespace Mediocre {
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;

    using CommandLine;
    using CommandLine.Text;

    using Mediocre.CLI;

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

                if (color.R < 10 && color.G < 10 && color.B < 10)
                    color = Color.Black;

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
            ListWhat.screens => ListScreens(opts.Filter),
            ListWhat.devices => ListDevices(opts.Filter),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Task<int> ListScreens(string? filter) {
            Console.WriteLine(HeadingInfo.Default);
            Console.WriteLine();

            foreach (var screen in Screenshot.All(filter)) {
                var isPrimary = screen.Screen?.Primary == true;

                using (ConsoleWith.FG(ConsoleColor.DarkYellow).When(isPrimary)) {
                    Console.WriteLine(screen.ScreenName);
                    Console.WriteLine();
                    Console.WriteLine($"  upper left:           ({screen.Bounds.Top}, {screen.Bounds.Left})");
                    Console.WriteLine($"  width:                {screen.Bounds.Width} px");
                    Console.WriteLine($"  height:               {screen.Bounds.Height} px");
                    Console.WriteLine();
                }
            }

            return Task.FromResult(0);
        }

        private static async Task<int> ListDevices(string? filter) {
            Console.WriteLine(HeadingInfo.Default);
            Console.WriteLine();

            foreach (var device in await Device.All(filter)) {
                var props = device.Properties.Select(x => $"{x.Key} = {x.Value}").OrderBy(x => x);
                var ops = device.SupportedOperations.Select(x => x.GetRealName()).OrderBy(x => x);

                Console.WriteLine(device);
                Console.WriteLine();
                Console.WriteLine($"  id:                   {device.Id}");
                Console.WriteLine($"  name:                 {device.Name}");
                Console.WriteLine($"  model:                {device.Model}");
                Console.WriteLine($"  firmware:             {device.FirmwareVersion}");
                Console.WriteLine($"  hostname:             {device.Hostname}");
                Console.WriteLine($"  port:                 {device.Port}");
                Console.WriteLine($"  properties:           {props.Join("\n                        ")}");
                Console.WriteLine($"  supported commands:   {ops.Join("\n                        ")}");
                Console.WriteLine();
            }

            return await Task.FromResult(0);
        }

        private static Task<int> ShowHelp(NotParsed<object> result) {
            var help = HelpText.AutoBuild(result, helpText => helpText
                .With(ht => ht.Copyright = "")
                .With(ht => ht.AdditionalNewLineAfterOption = false)
                .With(ht => ht.AddEnumValuesToHelpText = true));
            Console.WriteLine(help);
            return Task.FromResult(1);
        }
    }
}