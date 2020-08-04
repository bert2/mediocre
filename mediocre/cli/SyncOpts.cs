namespace Mediocre.Prototype {
    using CommandLine;

    [Verb("sync")]
    public readonly struct SyncOpts {
        [Option(Default = 300)]
        public int Smooth { get; }

        [Option(Default = 30)]
        public int Fps { get; }

        [Option(Default = 2)]
        public int SampleStep { get; }

        [Option(Default = 12345)]
        public int Port { get; }

        [Option('v', Default = false)]
        public bool Verbose { get; }

        public SyncOpts(int smooth, int fps, int sampleStep, int port, bool verbose) {
            Smooth = smooth;
            Fps = fps;
            SampleStep = sampleStep;
            Verbose = verbose;
            Port = port;
        }
    }
}
