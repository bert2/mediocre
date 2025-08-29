namespace Mediocre.CLI;

using CommandLine.Text;

using System;
using System.Drawing;
using System.Threading.Tasks;

public partial class Commands {
    /**
     * TODO:
     * - select sepcified device instead of first
     * - multi-device support
     * - select all devices by default
     * - subtract avg calc time from delay for more accurate fps
     */
    public static async Task<int> Sync(SyncOpts opts) {
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
            var brightFactor = Math.Clamp(opts.Brightness, 1, 100) / 100f;
            var bright = (color.GetBrightness() * brightFactor).Scale(1, 100);

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
}
