namespace Mediocre;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class Screenshot {
    public readonly string ScreenName;
    public readonly Rectangle Bounds;
    public readonly Screen? Screen;
    public readonly Bitmap Image;
    public readonly Graphics Gfx;

    [MemberNotNullWhen(true, nameof(Screen))]
    public bool IsPrimary => Screen?.Primary == true;

    [MemberNotNullWhen(false, nameof(Screen))]
    public bool IsVirtual { get; }

    public static Screenshot FromPrimaryScreen() => new();

    public static Screenshot FromVirtualScreen() => new(
        screenName: "(virtual screen)",
        top:    GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN),
        left:   GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN),
        width:  GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN),
        height: GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN));

    public static Screenshot FromScreenName(string name) {
        if (name.EqualsI("primary"))
            return FromPrimaryScreen();
        else if (name.EqualsI("virtual"))
            return FromVirtualScreen();

        var screens = Screen.AllScreens
            .Where(s => s.DeviceName.ContainsI(name))
            .ToArray();

        return screens switch {
            [var s] => new(s),
            [] => throw new InvalidOperationException($"No screen matching '{name}' found. Available screens: {Screen.AllScreens.Print()}"),
            _ => throw new InvalidOperationException($"Multiple screens found for '{name}': {screens.Print()}")
        };
    }

    public static IEnumerable<Screenshot> All(string? filter) {
        if (filter.EqualsI("primary"))
            return FromPrimaryScreen().AsSingleton();
        else if (filter.EqualsI("virtual"))
            return FromVirtualScreen().AsSingleton();

        return filter == null
            ? Screen.AllScreens
                .Select(s => new Screenshot(s))
                .Append(FromVirtualScreen())
            : Screen.AllScreens
                .Where(s => s.DeviceName.ContainsI(filter))
                .Select(s => new Screenshot(s));
    }

    public Screenshot()
        : this(Screen.PrimaryScreen ?? throw new InvalidOperationException("No primary screen found.")) { }

    public Screenshot(Screen screen) : this(screen.DeviceName, screen.Bounds, screen) { }

    public Screenshot(string screenName, int top, int left, int width, int height)
        : this(screenName, new Rectangle(x: left, y: top, width, height)) { }

    public Screenshot(string screenName, Rectangle bounds, Screen? screen = null) {
        ScreenName = screenName + (screen?.Primary == true ? " (primary)" : "");
        Bounds = bounds;
        Screen = screen;
        IsVirtual = screen is null;
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
