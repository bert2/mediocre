namespace basic_test {
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
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
            var left = 0; // GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN);
            var top = 0; // GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN);
            var width = 1920; // GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN);
            var height = 1080; // GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN);

            using var screen = new Bitmap(width, height);
            using var screenGfx = Graphics.FromImage(screen);

            using var avg = new Bitmap(1, 1);
            using var avgGfx = Graphics.FromImage(avg);
            avgGfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var device = await InitYeelight();

            const int frequency = 24;
            const int delay = 1000 / frequency;
            const int smooth = 300;

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
    }
}
