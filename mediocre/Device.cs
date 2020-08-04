namespace Mediocre {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using YeelightAPI;

    public static class Device {
        public static async Task<IDeviceController> InitFirst(int port) {
            var devices = await DeviceLocator.DiscoverAsync();

            var device = devices.FirstOrDefault() ?? throw new InvalidOperationException("No device found.");
            device.OnError += Log.Err;
            device.OnNotificationReceived += Log.Dbg;

            await device.Connect().LogErr("connect to", device);
            await device.TurnOn().LogErr("turn on", device);
            await device.StartMusicMode(port: port).LogErr("activate music mode on", device);

            return device;
        }
    }
}
