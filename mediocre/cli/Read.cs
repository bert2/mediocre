namespace Mediocre.CLI;

using CommandLine.Text;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

public partial class Commands {
    /**
     * TODO:
     * - everything
     */
    public static async Task<int> Read(ReadOpts opts) {
        Log.Verbose = opts.Verbose;
        Console.WriteLine(HeadingInfo.Default);
        Console.WriteLine();

        var device = await Device.InitFirst(opts.Port);
        Log.Msg($"syncing stdin color stream with {device}.");

        while (true) {
            var text = await Console.In.ReadLineAsync();
            Debug.Assert(text != null);
            var num = int.Parse(text);
            var color = num.ToRgb();
            var bright = color.GetBrightness().Scale(1, 100);

            if (color.R < 10 && color.G < 10 && color.B < 10)
                color = Color.Black;

            await device
                .SetRGBColor(color.R, color.G, color.B, opts.Smooth)
                .Log($"setting {color}.");

            await device
                .SetBrightness(bright, opts.Smooth)
                .Log($"setting brightness {bright}.");
        }
    }

}
