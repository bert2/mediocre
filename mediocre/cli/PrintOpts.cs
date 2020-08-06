namespace Mediocre.Prototype {
    using CommandLine;

    [Verb("print")]
    public readonly struct PrintOpts {
        [Option(Default = "primary")]
        public string Screen { get; }

        [Option(Default = 30)]
        public int Fps { get; }

        [Option(Default = 2)]
        public int SampleStep { get; }

        public PrintOpts(string screen, int fps, int sampleStep) {
            Screen = screen;
            Fps = fps;
            SampleStep = sampleStep;
        }
    }
}
