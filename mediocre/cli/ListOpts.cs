namespace Mediocre.Prototype {
    using System.Collections.Generic;

    using CommandLine;
    using CommandLine.Text;

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

        [Usage(ApplicationAlias = "yourapp")]
        public static IEnumerable<Example> Examples {
            get {
                yield return new Example("Normal scenario", new ListOpts(ListType.screens, null));
            }
        }
    }

    public enum ListType {
        devices,
        screens
    }
}
