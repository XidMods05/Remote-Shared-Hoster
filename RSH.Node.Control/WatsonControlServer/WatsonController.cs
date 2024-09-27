using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using RSH.Global.Util.Cons;
using WatsonTcp;

namespace RSH.Node.Control.WatsonControlServer;

public class WatsonController
{
    private readonly int _port;

    private readonly WatsonTcpServer _watsonTcpServer;
    private readonly ConcurrentDictionary<Guid, WatsonSessionData> _watsonTcpSessions;

    public WatsonController(int port)
    {
        _port = port;

        _watsonTcpServer = new WatsonTcpServer("0.0.0.0", port);
        {
            _watsonTcpServer.Events.ClientConnected += Cc!;
            _watsonTcpServer.Events.ClientDisconnected += Cd!;
            _watsonTcpServer.Events.MessageReceived += Mr!;
        }

        _watsonTcpSessions = new ConcurrentDictionary<Guid, WatsonSessionData>();
    }

    public void Start()
    {
        _watsonTcpServer.Start();

        Logger.Log(Logger.Prefixes.Start, $"Watson controller({_port}) launched! {_watsonTcpServer.Settings.Guid}.");
    }

    public void Stop()
    {
        _watsonTcpServer.Stop();

        Logger.Log(Logger.Prefixes.Stop, $"Watson controller({_port}) stopped! {_watsonTcpServer.Settings.Guid}.");
    }

    private void Cc(object sender, ConnectionEventArgs args)
    {
        Logger.Log(Logger.Prefixes.Tcp, $"New client connection detected! {args.Client.IpPort}.");

        _watsonTcpSessions.TryAdd(args.Client.Guid,
            new WatsonSessionData(_watsonTcpServer) { Id = args.Client.Guid, IpPort = args.Client.IpPort });
    }

    private void Cd(object sender, DisconnectionEventArgs args)
    {
        Logger.Log(Logger.Prefixes.Tcp, $"New client disconnection detected ({args.Reason})! {args.Client.IpPort}.");

        _watsonTcpSessions.TryRemove(args.Client.Guid, out var sessionData);
        if (sessionData == null) return;

        WatsonStaticSessionData.SessionNames.TryRemove(sessionData.AdminName, out _);
    }

    private void Mr(object sender, MessageReceivedEventArgs args)
    {
        try
        {
            Logger.Log(Logger.Prefixes.Tcp, $"New message received from {args.Client.IpPort}!" +
                                            $"\n  -> Message data length: {args.Data.Length}." +
                                            $"\n  -> Message metadata: {_watsonTcpServer.SerializationHelper.SerializeJson(args.Metadata)}.");

            var session = _watsonTcpSessions[args.Client.Guid];

            switch (Encoding.UTF8.GetString(args.Data))
            {
                case "error":
                    Logger.Log(Logger.Prefixes.Error, $"Error received from {args.Client.IpPort}! " +
                                                      $"JData: {_watsonTcpServer.SerializationHelper.SerializeJson(args.Metadata)}.");
                    break;
                case "server":
                    session.ExecuteServer(
                        JsonConvert.DeserializeObject<byte[]>(args.Metadata.GetValueOrDefault("buffer")!.ToString()!)!,
                        Convert.ToInt32(args.Metadata.GetValueOrDefault("server_id")!.ToString()),
                        Guid.Parse(args.Metadata.GetValueOrDefault("client_id")!.ToString()!));
                    break;
                case "meta":
                    foreach (var meta in args.Metadata)
                        if (!session.ExecuteMeta(meta.Key, meta.Value, out var rCode))
                            _watsonTcpServer.SendAsync(args.Client.Guid, "error", new Dictionary<string, object>
                            {
                                { "key", meta.Key },
                                { "value", meta.Value },
                                { "r_code", rCode }
                            });
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.Log(Logger.Prefixes.Error, e);
        }
    }
}