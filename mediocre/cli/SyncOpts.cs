namespace Mediocre.Prototype {
    using CommandLine;

    [Verb("sync")]
    public readonly struct SyncOpts {
        [Option(Default = null)]
        public string? Device { get; }

        [Option(Default = "primary")]
        public string Screen { get; }

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

        public SyncOpts(string? device, string screen, int smooth, int fps, int sampleStep, int port, bool verbose) {
            Device = device;
            Screen = screen;
            Smooth = smooth;
            Fps = fps;
            SampleStep = sampleStep;
            Verbose = verbose;
            Port = port;
        }
    }
}
