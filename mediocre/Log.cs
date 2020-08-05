namespace Mediocre {
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using YeelightAPI;

    public static class Log {
        public static bool Verbose;

        public static void Dbg(FormattableString msg) {
            if (!Verbose) return;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Err(FormattableString msg) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Dbg(object sender, NotificationReceivedEventArgs e) {
            if (Verbose) Dbg($"device received: {JsonConvert.SerializeObject(e.Result)}");
        }

        public static void Err(object sender, UnhandledExceptionEventArgs e)
            => Err($"device error: {e}");
    }

    public static class CmdResultExt {
        public static async Task Log(this Task<bool> cmdResult, FormattableString msg) {
            if (await cmdResult)
                Mediocre.Log.Dbg(msg);
            else
                Mediocre.Log.Err($"{msg} failed.");
        }
    }
}
