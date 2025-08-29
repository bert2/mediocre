namespace Mediocre.CLI;

using CommandLine;
using CommandLine.Text;

using System;
using System.Threading.Tasks;

public partial class Commands {
    public static Task<int> Help(NotParsed<object> result) {
        var help = HelpText.AutoBuild(result, helpText => helpText
            .With(ht => ht.Copyright = "")
            .With(ht => ht.AdditionalNewLineAfterOption = false)
            .With(ht => ht.AddEnumValuesToHelpText = true));
        Console.WriteLine(help);
        return Task.FromResult(1);
    }
}
