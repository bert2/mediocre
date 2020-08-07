namespace Mediocre.CLI {
    using CommandLine;

    [Verb("sync")]
    public readonly struct SyncOpts {
        [Option]
        public string[] Devices { get; }

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

        public SyncOpts(string[] devices, string screen, int smooth, int fps, int sampleStep, int port, bool verbose) {
            Devices = devices;
            Screen = screen;
            Smooth = smooth;
            Fps = fps;
            SampleStep = sampleStep;
            Verbose = verbose;
            Port = port;
        }
    }
}
