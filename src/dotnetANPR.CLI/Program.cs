using DotNetANPR;

const string helpText = """
    -----------------------------------------------------------
    dotnetANPR - Automatic Number Plate Recognition System
    Based on JavaANPR by Ondrej Martinsky (2006-2007)

    Usage: dotnet dotnetANPR.CLI.dll [options]

    Options:

        -help                          Display this help
        -recognize -i <snapshot>       Recognize a single snapshot
        -newconfig -o <file>           Generate default config file (JSON)
        -newnetwork -i <learndir> -o <file>
                                       Train neural network and save
        -newalphabet -i <srcdir> -o <dstdir>
                                       Normalize alphabet images
    -----------------------------------------------------------
    """;

if (args.Length == 0)
{
    Console.WriteLine(helpText);
    return;
}

try
{
    switch (args[0])
    {
        case "-help":
            Console.WriteLine(helpText);
            break;

        case "-recognize" when args.Length >= 3 && args[1] == "-i":
            var result = ANPR.Recognize(args[2]);
            Console.WriteLine(result ?? "(no plate recognized)");
            break;

        case "-newconfig" when args.Length >= 3 && args[1] == "-o":
            ANPR.ExportDefaultConfig(args[2]);
            Console.WriteLine("Default configuration saved to " + args[2]);
            break;

        case "-newnetwork" when args.Length >= 5 && args[1] == "-i" && args[3] == "-o":
            ANPR.TrainNetworkAndExport(args[2], args[4]);
            Console.WriteLine("Neural network trained and saved to " + args[4]);
            break;

        case "-newalphabet" when args.Length >= 5 && args[1] == "-i" && args[3] == "-o":
            ANPR.NormalizeAlphabets(args[2], args[4]);
            Console.WriteLine("Alphabet normalized from " + args[2] + " to " + args[4]);
            break;

        default:
            Console.WriteLine(helpText);
            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
    Environment.Exit(1);
}
