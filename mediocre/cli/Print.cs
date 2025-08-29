namespace Mediocre.CLI;

using System;
using System.Drawing;
using System.Threading.Tasks;

public partial class Commands {
    /**
     * TODO:
     * - subtract avg calc time from delay for more accurate fps
     * - configurable color format
     */
    public static async Task<int> Print(PrintOpts opts) {
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
}
