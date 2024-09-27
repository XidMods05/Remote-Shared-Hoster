using RSH.Global.Conf;
using RSH.Node.Control.WatsonControlServer;

namespace RemoteSharedHoster;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("You need to pass arguments to the console.");
            return;
        }

        if (!args.Any(arg => arg.StartsWith("-l=")))
        {
            Console.WriteLine("Argument -l is missing.");
            return;
        }

        if (!int.TryParse(args.First(arg => arg.StartsWith("-l="))[3..], out var l) || l < 0 || l > 100)
        {
            Console.WriteLine("Invalid argument -l. Must be an integer from 0 to 100.");
            return;
        }

        if (!args.Any(arg => arg.StartsWith("-p=")))
        {
            Console.WriteLine("Argument -p is missing.");
            return;
        }

        AppConfig.LogSensitive = l;

        foreach (var port in args.First(arg => arg.StartsWith("-p="))[3..].Split(',')
                     .Select(int.Parse).ToArray().Select(port => new WatsonController(port))) port.Start();

        for (;;) ;
    }
}