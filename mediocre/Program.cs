using CommandLine;

using Mediocre;
using Mediocre.CLI;

await new Parser()
    .ParseArguments<SyncOpts, PrintOpts, ReadOpts, ListOpts>(args)
    .MapResult(
        parsedSync:  Commands.Sync,
        parsedPrint: Commands.Print,
        parsedRead:  Commands.Read,
        parsedList:  Commands.List,
        notParsed:   Commands.Help);
