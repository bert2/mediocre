namespace Mediocre.CLI;
using CommandLine;

[Verb("print", HelpText = "Continously print the average color of a screen to the standard output.")]
public readonly struct PrintOpts(string screen, int fps, int sampleStep) {
    [Option(
        longName: "screen",
        shortName: 's',
        Default = "primary",
        MetaValue = "STRING",
        HelpText = "Identifies the screen to grab the average color from. STRING must uniquely match a screen by name. Partial matches are allowed. Use 'primary' to select the primary screen. Use 'virtual' to select the virtual screen (the bounding box around all screens).")]
    public string Screen { get; } = screen;

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
}
