namespace Mediocre.Prototype {

    using CommandLine;

    [Verb("list")]
    public readonly struct ListOpts {
        [Value(0,
            Required = true,
            MetaName = "what",
            MetaValue = "ENUM",
            HelpText = "What should be listed.")]
        public ListType What { get; }

        [Option(
            Default = null,
            MetaValue = "STRING",
            HelpText = "Optional filter. Only list items with STRING in their name.")]
        public string? Filter { get; }

        public ListOpts(ListType what, string? filter) {
            What = what;
            Filter = filter;
        }
    }

    public enum ListType {
        devices,
        screens
    }
}
