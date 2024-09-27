using System.Net;
using RSH.Node.Servers.Servidor;
using WatsonTcp;

namespace RSH.Node.Control.WatsonControlServer;

public class WatsonSessionData(WatsonTcpServer watsonTcpServer)
{
    public required Guid Id { get; init; }
    public required string IpPort { get; init; }

    public string AdminName { get; private set; } = string.Empty;
    public bool AdminIsGod { get; private set; }

    public Dictionary<int, UserTcpListener> LinkedServers { get; } = [];

    public bool ExecuteMeta(string key, object value, out string rCode)
    {
        key = key.ToLower().Trim();
        {
            key = key.Replace(" ", "_");
        }

        switch (key)
        {
            case "auth":
            {
                var name = value.ToString()!.ToLower().Trim();
                {
                    name = name.Replace(" ", "");
                }

                if (WatsonStaticSessionData.SessionNames.TryGetValue(name, out var sessionData))
                {
                    rCode = $"This name ({name}) already taken by {sessionData.IpPort}!";
                    return false;
                }

                if (name.Length is <= 3 or > 15)
                {
                    rCode = $"This name is incorrect ({(name.Length <= 3 ? "short name" : "long name")})!";
                    return false;
                }

                WatsonStaticSessionData.SessionNames.TryAdd(name, this);
                AdminName = name;

                goto returner;
            }

            case "auth_wgr124585":
            {
                var name = value.ToString()!.Trim();

                if (WatsonStaticSessionData.SessionNames.TryGetValue(name, out var sessionData))
                {
                    rCode = $"This name ({name}) already taken by {sessionData.Id} (admsecdat)!";
                    return false;
                }

                WatsonStaticSessionData.SessionNames.TryAdd(name, this);

                AdminName = name;
                AdminIsGod = true;

                goto returner;
            }
        }

        if (AdminName == string.Empty)
        {
            rCode = "You are not authenticated yet!";
            return false;
        }

        switch (key)
        {
            case "link_server":
            {
                var pn = Convert.ToInt32(value.ToString());

                if (WatsonStaticSessionData.SessionLinkedServers.TryGetValue(pn, out var sessionData))
                {
                    rCode = $"This server ({pn}) already linked by {sessionData.AdminName}!";
                    return false;
                }

                WatsonStaticSessionData.SessionLinkedServers.TryAdd(pn, this);

                LinkedServers.Add(pn, new UserTcpListener(IPAddress.Any, pn, Id, watsonTcpServer));
                LinkedServers[pn].Start();

                break;
            }

            case "unlink_server":
            {
                var pn = Convert.ToInt32(value.ToString());

                if (!WatsonStaticSessionData.SessionLinkedServers.TryGetValue(pn, out _))
                {
                    rCode = $"This server ({pn}) not linked!";
                    return false;
                }

                var linker = WatsonStaticSessionData.SessionLinkedServers[pn];

                if (!linker.AdminName.Equals(AdminName))
                {
                    if (AdminIsGod)
                    {
                        WatsonStaticSessionData.SessionLinkedServers.TryRemove(pn, out var creatorSessionData);

                        creatorSessionData!.LinkedServers[pn].Stop(true);
                        creatorSessionData.LinkedServers.Remove(pn);

                        break;
                    }

                    rCode = $"The server is not linked by you! He is linked by {linker.AdminName}.";
                    return false;
                }

                WatsonStaticSessionData.SessionLinkedServers.TryRemove(pn, out _);

                LinkedServers[pn].Stop(true);
                LinkedServers.Remove(pn);

                break;
            }
            default:
                rCode = $"Unknown command: {key}.";
                return false;
        }

        returner:
        rCode = string.Empty;
        return true;
    }
    
    public void ExecuteServer(byte[] buffer, int port, Guid guid)
    {
        if (!LinkedServers.TryGetValue(port, out var listener))
            return;
        
        listener.FindSession(guid)?.SendAsync(buffer);
    }
}