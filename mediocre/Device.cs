namespace Mediocre {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using YeelightAPI;

    public static class Device {
        public static async Task<IDeviceController> InitFirst(int port) {
            Log.Dbg($"looking for Yeelight devices...");
            DeviceLocator.MaxRetryCount = 3;
            var devices = await DeviceLocator.DiscoverAsync();
            Log.Dbg($"found {devices.Count()} devices.");

            var device = devices.FirstOrDefault() ?? throw new InvalidOperationException("No device found.");
            device.OnError += Log.Err;
            device.OnNotificationReceived += Log.Dbg;
            Log.Dbg($"selected device {device}.");

            await device.Connect().Log($"connecting to {device}.");
            await device.TurnOn().Log($"turning on {device}.");
            await device.StartMusicMode(port: port).Log($"activating music mode on {device}.");

            return device;
        }
    }
}
