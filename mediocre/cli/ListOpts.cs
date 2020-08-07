namespace Mediocre.CLI {
    using CommandLine;

    [Verb("list")]
    public readonly struct ListOpts {
        [Value(0,
            Required = true,
            MetaName = "what",
            MetaValue = "ENUM",
            HelpText = "What should be listed.")]
        public ListWhat What { get; }

        [Option(
            Default = null,
            MetaValue = "STRING",
            HelpText = "Optional filter. Only list items with STRING in their name.")]
        public string? Filter { get; }

        public ListOpts(ListWhat what, string? filter) {
            What = what;
            Filter = filter;
        }
    }

    public enum ListWhat {
        devices,
        screens
    }
}
