namespace Mediocre.Prototype {
    using CommandLine;

    [Verb("print")]
    public readonly struct PrintOpts {
        [Option]
        public int Fps { get; }

        [Option(Default = 2)]
        public int SampleStep { get; }

        public PrintOpts(int fps, int sampleStep) {
            Fps = fps;
            SampleStep = sampleStep;
        }
    }
}
