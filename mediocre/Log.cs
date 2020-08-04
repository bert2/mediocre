namespace Mediocre {
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using YeelightAPI;

    public static class Log {
        public static bool Verbose;

        public static void Dbg(string msg) {
            if (!Verbose) return;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Dbg(object sender, NotificationReceivedEventArgs e) {
            if (!Verbose) return;
            Dbg($"device received: {JsonConvert.SerializeObject(e.Result)}");
        }

        public static void Err(string msg) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Err(object sender, UnhandledExceptionEventArgs e)
            => Err($"device error: {e}");

        public static async Task LogErr(this Task<bool> cmdResult, string action, IDeviceController device) {
            if (!await cmdResult) Err($"failed to {action} {device}");
        }
    }
}
