namespace basic_test {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using YeelightAPI;

    public enum SystemMetric {
        SM_XVIRTUALSCREEN = 76,
        SM_YVIRTUALSCREEN = 77,
        SM_CXVIRTUALSCREEN = 78,
        SM_CYVIRTUALSCREEN = 79
    }

    public static class Program {
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric metric);

        public static async Task Main() {
            const bool benchmark = true;
            const bool projector = false;

            var left = projector ? GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN) : 0;
            var top = projector ? GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN) : 0;
            var width = 1920; // GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN);
            var height = 1080; // GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN);

            if (benchmark)
                Benchmark(left, top, width, height);
            else
                await MainLoop(left, top, width, height, frequency: 30);
        }

        private static async Task MainLoop(int left, int top, int width, int height, int frequency) {
            var delay = 1000 / frequency;

            var device = await InitYeelight();
            const int smooth = 300;

            using var screen = new Bitmap(width, height);
            using var screenGfx = Graphics.FromImage(screen);

            using var avg = new Bitmap(1, 1);
            using var avgGfx = Graphics.FromImage(avg);
            avgGfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var prevColor = Color.Black;
            var prevBright = 0;
            while (true) {
                screenGfx.CopyFromScreen(left, top, 0, 0, screen.Size);
                avgGfx.DrawImage(screen, 0, 0, avg.Width, avg.Height);

                var color = avg.GetPixel(0, 0);
                if (color != prevColor) {
                    LogDbg($"set_rgb {color}");
                    var set_rgb = $"{{\"id\": {DateTime.Now.Ticks}, \"method\": \"set_rgb\", \"params\":[{color.ToRgb()}, \"smooth\", {smooth}]}}\r\n";
                    _ = device.Client.Send(Encoding.UTF8.GetBytes(set_rgb));
                }
                prevColor = color;

                var bright = (int)Math.Round(Math.Clamp(color.GetBrightness() * 100, 1, 100));
                if (bright != prevBright) {
                    LogDbg($"set_bright {bright}");
                    var set_bright = $"{{\"id\": {DateTime.Now.Ticks}, \"method\": \"set_bright\", \"params\":[{bright}, \"smooth\", {smooth}]}}\r\n";
                    _ = device.Client.Send(Encoding.UTF8.GetBytes(set_bright));
                }
                prevBright = bright;

                await Task.Delay(delay);
            }
        }

        private static void Benchmark(int left, int top, int width, int height) {
            const int iterations = 1000;

            using var screen = new Bitmap(width, height);
            using var screenGfx = Graphics.FromImage(screen);

            using var avgBicubic = new Bitmap(1, 1);
            using var avgBicubicGfx = Graphics.FromImage(avgBicubic);

            var avgsBase = new StringBuilder(20 * iterations);
            var avgsBicubic = new StringBuilder(20 * iterations);
            var avgsBicubicHq = new StringBuilder(20 * iterations);

            Console.WriteLine("warming up...");
            for (var i = 0; i < 100; i++) {
                screenGfx.CopyFromScreen(left, top, 0, 0, screen.Size);
                Console.WriteLine(GetAvgBicubicHq(screen, avgBicubic, avgBicubicGfx));
            }

            var sw = new Stopwatch();
            for (var i = 0; i < iterations; i++) {
                Console.WriteLine(i);
                screenGfx.CopyFromScreen(left, top, 0, 0, screen.Size);

                sw.Restart();
                var currAvgTruth = GetAvgTruth(screen);
                sw.Stop();
                _ = avgsBase.AppendColor(currAvgTruth).Append(sw.ElapsedMilliseconds).AppendLine();

                sw.Restart();
                var currAvgBicubic = GetAvgBicubic(screen, avgBicubic, avgBicubicGfx);
                sw.Stop();
                _ = avgsBicubic.AppendColor(currAvgBicubic).Append(sw.ElapsedMilliseconds).AppendLine();

                sw.Restart();
                var currAvgBicubicHq = GetAvgBicubicHq(screen, avgBicubic, avgBicubicGfx);
                sw.Stop();
                _ = avgsBicubicHq.AppendColor(currAvgBicubicHq).Append(sw.ElapsedMilliseconds).AppendLine();
            }

            File.WriteAllText("avgs-baseline.csv", avgsBase.ToString());
            File.WriteAllText("avgs-bicubic.csv", avgsBicubic.ToString());
            File.WriteAllText("avgs-bicubic-hq.csv", avgsBicubicHq.ToString());
        }

        private static Color GetAvgBicubicHq(Bitmap screen, Bitmap avg, Graphics avgGfx) {
            avgGfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            avgGfx.DrawImage(screen, 0, 0, avg.Width, avg.Height);
            return avg.GetPixel(0, 0);
        }

        private static Color GetAvgBicubic(Bitmap screen, Bitmap avg, Graphics avgGfx) {
            avgGfx.InterpolationMode = InterpolationMode.Bicubic;
            avgGfx.DrawImage(screen, 0, 0, avg.Width, avg.Height);
            return avg.GetPixel(0, 0);
        }

        private static unsafe Color GetAvgTruth(Bitmap screen) {
            var data = screen.LockBits(
                new Rectangle(0, 0, screen.Width, screen.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            if (data.Stride != data.Width * 3) throw new InvalidOperationException("Padded images not supported");

            var raw = (byte*)data.Scan0.ToPointer();
            var (r, g, b) = (0L, 0L, 0L);
            var rawLength = data.Width * data.Height * 3;

            for (var i = 0; i < rawLength;) {
                b += raw[i++];
                g += raw[i++];
                r += raw[i++];
            }

            screen.UnlockBits(data);

            var n = data.Width * data.Height;
            return Color.FromArgb((int)(r / n), (int)(g / n), (int)(b / n));
        }

        private static async Task<TcpClient> InitYeelight() {
            DeviceLocator.MaxRetryCount = 2;
            var devices = await DeviceLocator.DiscoverAsync(new Progress<Device>(d => Log($"discovered device: {d}")));
            var device = devices.FirstOrDefault() ?? throw new InvalidOperationException("No device discovered.");
            device.OnNotificationReceived += (_, e) => LogDbg($"dev recvd: {JsonConvert.SerializeObject(e.Result)}");
            device.OnError += (_, e) => Log($"dev err: {e}");

            Debug.Assert(await device.Connect());
            Debug.Assert(await device.TurnOn());

            var localAddr = NetworkInterface
                .GetAllNetworkInterfaces()
                .Single(ifc => ifc.Name == "Ethernet")
                .GetIPProperties()
                .UnicastAddresses
                .Single(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Address;
            const int port = 12345;

            var listener = new TcpListener(localAddr, port);
            listener.Start();

            Debug.Assert(await device.StartMusicMode(hostName: localAddr.ToString(), port));

            return await listener.AcceptTcpClientAsync();
        }

        private static int ToRgb(this Color c) => (c.R << 16) | (c.G << 8) | c.B;

        private static void Log(string msg) => Console.WriteLine(msg);

        private static void LogDbg(string msg) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void LogErr(string msg) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void Pause(string? msg = null) {
            Log($"ready to {msg ?? "continue"}? press any key to continue...");
            _ = Console.ReadKey(true);
        }

        private static string Join(this IEnumerable<string> strs, string sep) => string.Join(sep, strs);

        private static StringBuilder AppendColor(this StringBuilder sb, Color c) => sb
            .Append(c.R).Append(", ")
            .Append(c.G).Append(", ")
            .Append(c.B).Append(", ");
    }
}
