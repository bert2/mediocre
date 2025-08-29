namespace Mediocre.CLI;

using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

[Verb("list", HelpText = "List the available screens or Yeelight devices.")]
public readonly struct ListOpts(ListWhat what, string? filter = null) {
    [Value(
        index: 0,
        Required = true,
        MetaName = "what",
        MetaValue = "ENUM",
        HelpText = "What should be listed.")]
    public ListWhat What { get; } = what;

    [Value(
        index: 1,
        Required = false,
        MetaName = "filter",
        MetaValue = "STRING",
        Default = null,
        HelpText = "Optional filter. Only list items with STRING in their name.")]
    public string? Filter { get; } = filter;

    [Usage(ApplicationAlias = "mediocre")]
    public static IEnumerable<Example> Examples {
        get {
            yield return new Example("List all screens", new ListOpts(ListWhat.screens));
            yield return new Example("Only list screens with 'foo' in their name", new ListOpts(ListWhat.screens, "foo"));
            yield return new Example("Only list the primary screen", new ListOpts(ListWhat.screens, "primary"));
            yield return new Example("Only list the virtual screen (the bounding box around all screens)", new ListOpts(ListWhat.screens, "virtual"));

            yield return new Example("List all devices", new ListOpts(ListWhat.devices));
            yield return new Example("Only list devices with 'foo' in their name", new ListOpts(ListWhat.devices, "foo"));
        }
    }
}

public enum ListWhat {
    devices = 1,
    screens = 2
}
