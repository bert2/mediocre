namespace Mediocre.CLI;
using CommandLine;

[Verb("sync", HelpText = "Sync one or more Yeelight devices with the average color of a screen.")]
public readonly struct SyncOpts(string screen, int brightness, int smooth, int fps, int sampleStep, int port, bool verbose) {
    [Option(
        longName: "screen",
        shortName: 's',
        Default = "primary",
        MetaValue = "STRING",
        HelpText = "Identifies the screen to grab the average color from. STRING must uniquely match a screen by name. Partial matches are allowed. Use 'primary' to select the primary screen. Use 'virtual' to select the virtual screen (the bounding box around all screens).")]
    public string Screen { get; } = screen;

    [Option(
        longName: "brightness",
        shortName: 'b',
        Default = 100,
        MetaValue = "INTEGER",
        HelpText = "Controls the brightness of the device. INTEGER is a percentage value between 1 and 100.")]
    public int Brightness { get; } = brightness;

    [Option(Default = 300)]
    public int Smooth { get; } = smooth;

    [Option(
        longName: "fps",
        shortName: 'f',
        Default = 30,
        MetaValue = "INTEGER",
        HelpText = "Maximum update rate in frames per second.")]
    public int Fps { get; } = fps;

    [Option(
        longName: "samplestep",
        shortName: 'k',
        Default = 2,
        MetaValue = "INTEGER",
        HelpText = "Determines the number of screen pixels to analyze (1 = every pixel, 2 = every 2nd, 3 = every 3rd, ...). Increase to improve performance at higher resolutions and/or update rates.")]
    public int SampleStep { get; } = sampleStep;

    [Option(Default = 12345)]
    public int Port { get; } = port;

    [Option(
        longName: "verbose",
        shortName: 'v',
        Default = false,
        HelpText = "Enables verbose output for debugging.")]
    public bool Verbose { get; } = verbose;
}
