using Newtonsoft.Json;
using RSH.Global.Util.Cons;
using RSH.Global.Util.Help;
using RSH.Node.Servers.Netbase;
using RSH.Node.Servers.Netbase.WWW;
using WatsonTcp;
using Timer = System.Timers.Timer;

namespace RSH.Node.Servers.Session;

public class UserTcpSession(TcpServer tcpServer, Guid creatorGuid, WatsonTcpServer watsonTcpServer)
    : TcpSession(tcpServer)
{
    private Timer _toDiscTimer = null!;
    private bool _toDiscTimerCanBeEliminatedByNewMessage;

    protected override void OnConnected()
    {
        base.OnConnected();

        try
        {
            Logger.Log(Logger.Prefixes.Tcp, $"New UserTcpSession created! " +
                                            $"Information: Id = {Id}; Ip = {Helper.GetIpBySocket(Socket)}. " +
                                            $"TcpServer info: Ip = {Server.Address}; Port = {Server.Port}.");
        }
        catch
        {
            Logger.Log(Logger.Prefixes.Cmd,
                $"DOS warning on server: Ip = {Server.Address}; Port = {Server.Port}.");
        }

        DisconnectAfterTime(10, true);
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        try
        {
            Logger.Log(Logger.Prefixes.Tcp, $"UserTcpSession closed! " +
                                            $"Information: Id = {Id}. " +
                                            $"TcpServer info: Ip = {Server.Address}; Port = {Server.Port}.");
        }
        catch
        {
            Logger.Log(Logger.Prefixes.Cmd,
                $"DDOS warning on server: Ip = {Server.Address}; Port = {Server.Port}.");
        }
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        base.OnReceived(buffer, offset, size);

        if (size + offset >= 2048)
        {
            Disconnect();
            return;
        }

        if (_toDiscTimer != null! && _toDiscTimerCanBeEliminatedByNewMessage)
        {
            _toDiscTimer.Stop();
            _toDiscTimer.Dispose();
            _toDiscTimer = null!;
        }

        watsonTcpServer.SendAsync(creatorGuid, "server", new Dictionary<string, object>
        {
            { "buffer", JsonConvert.SerializeObject(buffer.Take((int)size)) },
            { "server_id", Server.Port },
            { "client_id", Id }
        });
    }

    public void DisconnectAfterTime(int secs, bool canBeEliminatedByNewMessage = false)
    {
        _toDiscTimer = new Timer(1000 * secs) { Enabled = true, AutoReset = false };
        _toDiscTimerCanBeEliminatedByNewMessage = canBeEliminatedByNewMessage;

        _toDiscTimer.Elapsed += (_, _) =>
        {
            Disconnect();

            if (_toDiscTimer == null!) return;
            _toDiscTimer.Dispose();
            _toDiscTimer = null!;
        };
    }
}