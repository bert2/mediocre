namespace Mediocre.CLI;

using CommandLine.Text;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class Commands {
    public static Task<int> List(ListOpts opts) => opts.What switch {
        ListWhat.screens => ListScreens(opts.Filter),
        ListWhat.devices => ListDevices(opts.Filter),
        _ => throw new SwitchExpressionException(opts.What)
    };

    private static Task<int> ListScreens(string? filter) {
        Console.WriteLine(HeadingInfo.Default);
        Console.WriteLine();

        foreach (var screen in Screenshot.All(filter)) {
            using (ConsoleWith.FG(ConsoleColor.DarkYellow).When(screen.IsPrimary)) {
                Console.WriteLine(
                    $"""
                    {screen.ScreenName}

                      upper left:           ({screen.Bounds.Top}, {screen.Bounds.Left})
                      width:                {screen.Bounds.Width} px
                      height:               {screen.Bounds.Height} px

                    """);
            }
        }

        return Task.FromResult(0);
    }

    private static async Task<int> ListDevices(string? filter) {
        Console.WriteLine(HeadingInfo.Default);
        Console.WriteLine();

        foreach (var device in await Device.All(filter)) {
            var props = device.Properties.Select(x => $"{x.Key} = {x.Value}").Order();
            var ops = device.SupportedOperations.Select(x => x.GetRealName()).Order();

            Console.WriteLine(
                $"""
                {device}

                  id:                   {device.Id}
                  name:                 {device.Name}
                  model:                {device.Model}
                  firmware:             {device.FirmwareVersion}
                  hostname:             {device.Hostname}
                  port:                 {device.Port}
                  properties:           {props.Join("\n                        ")}
                  supported commands:   {ops.Join("\n                        ")}
                
                """);
        }

        return await Task.FromResult(0);
    }
}
