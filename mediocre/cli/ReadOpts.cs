namespace Mediocre.CLI;
using CommandLine;

[Verb("read", HelpText = "Read a stream of colors from stdin and use it to change the color of one or more Yeelight devices.")]
public readonly struct ReadOpts(string[] devices, int smooth, int port, bool verbose) {
    [Option]
    public string[] Devices { get; } = devices;

    [Option(Default = 300)]
    public int Smooth { get; } = smooth;

    [Option(Default = 12345)]
    public int Port { get; } = port;

    [Option('v', Default = false)]
    public bool Verbose { get; } = verbose;
}
