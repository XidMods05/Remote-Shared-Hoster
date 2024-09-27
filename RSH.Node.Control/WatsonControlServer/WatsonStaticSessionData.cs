using System.Collections.Concurrent;

namespace RSH.Node.Control.WatsonControlServer;

public static class WatsonStaticSessionData
{
    public static readonly ConcurrentDictionary<string, WatsonSessionData> SessionNames = new();
    public static readonly ConcurrentDictionary<int, WatsonSessionData> SessionLinkedServers = new();
}