namespace Yeelight.Ambient.Prototype {
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
    using System.Numerics;
    using System.Runtime.CompilerServices;
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
            const bool benchmark = false;
            const bool projector = false;
            const bool virtScreen = false;

            var left = projector || virtScreen ? GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN) : 0;
            var top = projector || virtScreen ? GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN) : 0;
            var width = virtScreen ? GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN) : 1920;
            var height = virtScreen ? GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN) : 1080;
            Console.WriteLine($"t/l: {top}/{left}");
            Console.WriteLine($"w/h: {width}/{height}");

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

            var prevColor = Color.Black;
            var prevBright = 0;
            while (true) {
                screenGfx.CopyFromScreen(left, top, 0, 0, screen.Size);
                var color = GetAvgBase(screen, step: 2);

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

            var avgLevels = Enumerable.Range(1, int.MaxValue)
                .Select(x => 1 << x)
                .Select(x => screen.Size / x)
                .TakeWhile(x => x.Width > 0 || x.Height > 0)
                .Select(x => new Size(Math.Max(x.Width, 1), Math.Max(x.Height, 1)))
                .Select(s => {
                    var bmp = new Bitmap(s.Width, s.Height);
                    var gfx = Graphics.FromImage(bmp);
                    return (bmp, gfx);
                })
                .ToArray();

            var avgsBase = new StringBuilder(20 * iterations);
            var avgsStep2 = new StringBuilder(20 * iterations);
            var avgsStep4 = new StringBuilder(20 * iterations);
            var avgsTpl = new StringBuilder(20 * iterations);
            var avgsSimd = new StringBuilder(20 * iterations);
            var avgsBicubic = new StringBuilder(20 * iterations);
            var avgsBilinear = new StringBuilder(20 * iterations);

            Console.WriteLine("warming up...");
            for (var i = 0; i < 10; i++) {
                screenGfx.CopyFromScreen(left, top, 0, 0, screen.Size);
                Console.WriteLine(GetAvgTpl(screen));
            }

            var sw = new Stopwatch();
            for (var i = 0; i < iterations; i++) {
                Console.WriteLine(i);
                screenGfx.CopyFromScreen(left, top, 0, 0, screen.Size);
                Color avg;

                sw.Restart();
                avg = GetAvgBase(screen, step: 1);
                sw.Stop();
                _ = avgsBase.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();
#if DEBUG
                Console.WriteLine($"avg: {avg}");
#endif

                sw.Restart();
                avg = GetAvgBase(screen, step: 2);
                sw.Stop();
                _ = avgsStep2.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();

                sw.Restart();
                avg = GetAvgBase(screen, step: 4);
                sw.Stop();
                _ = avgsStep4.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();

                //sw.Restart();
                //avg = GetAvgTpl(screen);
                //sw.Stop();
                //_ = avgsTpl.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();

                //sw.Restart();
                //avg = GetAvgSimd(screen);
                //sw.Stop();
                //_ = avgsSimd.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();

                //sw.Restart();
                //avg = GetAvgInterp(screen, avgLevels, InterpolationMode.Bicubic);
                //sw.Stop();
                //_ = avgsBicubic.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();

                //sw.Restart();
                //avg = GetAvgInterp(screen, avgLevels, InterpolationMode.Bilinear);
                //sw.Stop();
                //_ = avgsBilinear.AppendColor(avg).Append(sw.ElapsedMilliseconds).AppendLine();
            }

            if (avgsBase.Length > 0)     File.WriteAllText("avgs-baseline.csv", avgsBase    .ToString());
            if (avgsStep2.Length > 0)    File.WriteAllText("avgs-step-2.csv",   avgsStep2   .ToString());
            if (avgsStep4.Length > 0)    File.WriteAllText("avgs-step-4.csv",   avgsStep4   .ToString());
            if (avgsTpl.Length > 0)      File.WriteAllText("avgs-tpl.csv",      avgsTpl     .ToString());
            if (avgsSimd.Length > 0)     File.WriteAllText("avgs-simd.csv",     avgsSimd    .ToString());
            if (avgsBicubic.Length > 0)  File.WriteAllText("avgs-bicubic.csv",  avgsBicubic .ToString());
            if (avgsBilinear.Length > 0) File.WriteAllText("avgs-bilinear.csv", avgsBilinear.ToString());
        }

        private static Color GetAvgInterp(Bitmap screen, (Bitmap bmp, Graphics gfx)[] avgLevels, InterpolationMode mode) {
            var previous = screen;
            for (var l = 0; l < avgLevels.Length; l++) {
                avgLevels[l].gfx.InterpolationMode = mode;
                avgLevels[l].gfx.DrawImage(previous, 0, 0, avgLevels[l].bmp.Width, avgLevels[l].bmp.Height);
                previous = avgLevels[l].bmp;
            }

            return avgLevels.Last().bmp.GetPixel(0, 0);
        }

        private static unsafe Color GetAvgTpl(Bitmap screen) {
            // LockBits() is too slow, because it converts to a different PixelFormat.
            // Fixing this would require changes to the TPL implementation.
            // Not worth the trouble though, because GetAvg() with `step` is already fast enough.
            var data = screen.LockBits(
                new Rectangle(0, 0, screen.Width, screen.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            const int bytesPerPixel = 3;
            var raw = (byte*)data.Scan0.ToPointer();
            var rawLineLen = data.Width * bytesPerPixel; // 5760
            if (data.Stride != rawLineLen) throw new InvalidOperationException("Padded images are not supported.");

            var numChunks = Environment.ProcessorCount; // 12
            var linesPerChunk = data.Height / numChunks; // 90
            var chunkLen = rawLineLen * linesPerChunk; // 518400

            var chunkSumsR = new long[numChunks];
            var chunkSumsG = new long[numChunks];
            var chunkSumsB = new long[numChunks];

            var loop = Parallel.For(0, numChunks, chunkIdx => {
                var chunk = raw + chunkIdx * chunkLen;
                for (var i = 0; i < chunkLen;) {
                    chunkSumsB[chunkIdx] += chunk[i++];
                    chunkSumsG[chunkIdx] += chunk[i++];
                    chunkSumsR[chunkIdx] += chunk[i++];
                }
            });

            screen.UnlockBits(data);

            if (!loop.IsCompleted) throw new InvalidOperationException("Parallel loop didn't finish.");
            var (r, g, b) = (chunkSumsR.Sum(), chunkSumsG.Sum(), chunkSumsB.Sum());
            var n = data.Width * data.Height;

            return Color.FromArgb((int)(r / n), (int)(g / n), (int)(b / n));
        }

        private static unsafe Color GetAvgSimd(Bitmap screen) {
            // LockBits() is too slow, because it converts to a different PixelFormat.
            // Fixing this would require changes to the SIMD implementation.
            // Not worth the trouble though, because GetAvg() with `step` is already fast enough.
            var data = screen.LockBits(
                new Rectangle(0, 0, screen.Width, screen.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            const int bytesPerPixel = 3;
            var raw = (byte*)data.Scan0.ToPointer();
            var rowLen = data.Width * bytesPerPixel; // 5760
            if (data.Stride != rowLen) throw new InvalidOperationException("Padded images are not supported.");

            var vecsPerRow = rowLen / Vector<uint>.Count; // 5760 / 8 = 720
            var colSums = new Vector<uint>[vecsPerRow];

            for (var y = 0; y < data.Height; y++) {
                for (var i = 0; i < vecsPerRow;) {
                    var bytes = Unsafe.Read<Vector<byte>>(raw);
                    Vector.Widen(bytes, out var ushorts1, out var ushorts2);
                    Vector.Widen(ushorts1, out var uints1, out var uints2);
                    Vector.Widen(ushorts2, out var uints3, out var uints4);
                    colSums[i++] += uints1;
                    colSums[i++] += uints2;
                    colSums[i++] += uints3;
                    colSums[i++] += uints4;
                    raw += Vector<byte>.Count;
                }
            }

            var (r, g, b) = (0L, 0L, 0L);

            for (var i = 0; i < vecsPerRow;) {
                b += colSums[i][0];
                g += colSums[i][1];
                r += colSums[i][2];

                b += colSums[i][3];
                g += colSums[i][4];
                r += colSums[i][5];

                b += colSums[i][6];
                g += colSums[i][7];

                i++;

                r += colSums[i][0];

                b += colSums[i][1];
                g += colSums[i][2];
                r += colSums[i][3];

                b += colSums[i][4];
                g += colSums[i][5];
                r += colSums[i][6];

                b += colSums[i][7];

                i++;

                g += colSums[i][0];
                r += colSums[i][1];

                b += colSums[i][2];
                g += colSums[i][3];
                r += colSums[i][4];

                b += colSums[i][5];
                g += colSums[i][6];
                r += colSums[i][7];

                i++;
            }

            screen.UnlockBits(data);

            var n = data.Width * data.Height;

            return Color.FromArgb((int)(r / n), (int)(g / n), (int)(b / n));
        }

        private static unsafe Color GetAvgBase(Bitmap screen, int step) {
            var data = screen.LockBits(
                new Rectangle(0, 0, screen.Width, screen.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var row = (int*)data.Scan0.ToPointer();
            var (sumR, sumG, sumB) = (0L, 0L, 0L);
            var stride = data.Stride / sizeof(int) * step;

            for (var y = 0; y < data.Height; y += step) {
                for (var x = 0; x < data.Width; x += step) {
                    var argb = row[x];
                    sumR += (argb & 0x00FF0000) >> 16;
                    sumG += (argb & 0x0000FF00) >> 8;
                    sumB += argb & 0x000000FF;
                }
                row += stride;
            }

            screen.UnlockBits(data);

            var numSamples = data.Width / step * data.Height / step;
            var avgR = sumR / numSamples;
            var avgG = sumG / numSamples;
            var avgB = sumB / numSamples;
            return Color.FromArgb((int)avgR, (int)avgG, (int)avgB);
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

            Console.WriteLine($"listening at {localAddr}:{port}...");

            Debug.Assert(await device.StartMusicMode(hostName: localAddr.ToString(), port));
            Console.WriteLine("accepting...");
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

        private static IEnumerable<int> Range(int start, int count) => Enumerable.Range(start, count);
    }
}
