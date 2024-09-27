using System.Net;
using System.Net.Sockets;
using RSH.Global.Util.Cons;
using RSH.Node.Servers.Netbase;
using RSH.Node.Servers.Netbase.WWW;
using RSH.Node.Servers.Session;
using WatsonTcp;

namespace RSH.Node.Servers.Servidor;

public class UserTcpListener(IPAddress address, int port, Guid creatorGuid, WatsonTcpServer watsonTcpServer)
    : TcpServer(address, port)
{
    public override bool Start()
    {
        OptionAcceptorBacklog = int.MaxValue;
        OptionNoDelay = true;

        var r = base.Start();

        Logger.Log(Logger.Prefixes.Start,
            $"New UserTcpListener started! Listening endpoint: {Endpoint}.");
        return r;
    }

    protected override TcpSession CreateSession()
    {
        return new UserTcpSession(this, creatorGuid, watsonTcpServer);
    }

    protected override void OnError(SocketError error)
    {
        base.OnError(error);

        Logger.Log(Logger.Prefixes.Error,
            $"New error handled in UserTcpListener-({Endpoint})! Error: {error}.");
    }
}