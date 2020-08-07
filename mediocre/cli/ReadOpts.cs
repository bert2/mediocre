namespace Mediocre.CLI {
    using CommandLine;

    [Verb("read")]
    public readonly struct ReadOpts {
        [Option]
        public string[] Devices { get; }

        public ReadOpts(string[] devices) {
            Devices = devices;
        }
    }
}
