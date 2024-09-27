using System.Text;
using Newtonsoft.Json;
using RSH.Global.Util.Cons;
using WatsonTcp;

namespace RSH.Test.Project2;

internal class Program
{
    private static WatsonTcpClient _client;

    private static void Main(string[] args)
    {
        _client = new WatsonTcpClient("127.0.0.1", 5000);
        _client.Events.ServerConnected += ServerConnected;
        _client.Events.ServerDisconnected += ServerDisconnected;
        _client.Events.MessageReceived += MessageReceived;
        _client.Callbacks.SyncRequestReceivedAsync = SyncRequestReceived;
        _client.Connect();

        _client.SendAsync("meta", new Dictionary<string, object>
        {
            { "auth", "77dxzdd" },
            { "link_server", 9339 }
        });

        for (;;) ;
    }

    private static void MessageReceived(object sender, MessageReceivedEventArgs args)
    {
        Logger.Log(Logger.Prefixes.Tcp, $"New message received!" +
                                        $"\n  -> Message data length: {args.Data.Length}." +
                                        $"\n  -> Message metadata: {_client.SerializationHelper.SerializeJson(args.Metadata)}.");

        var cKey = Encoding.UTF8.GetString(args.Data).ToLower();

        switch (cKey)
        {
            case "error":
                Logger.Log(Logger.Prefixes.Error,
                    $"Error received! JData: {_client.SerializationHelper.SerializeJson(args.Metadata)}.");
                break;
            case "server":
                ExecuteServer(
                    JsonConvert.DeserializeObject<IEnumerable<byte>>(args.Metadata.GetValueOrDefault("buffer")
                        ?.ToString()!)!.ToArray(),
                    Convert.ToInt32(args.Metadata.GetValueOrDefault("server_id")?.ToString()),
                    Guid.Parse(args.Metadata.GetValueOrDefault("client_id")?.ToString()!));
                break;
            case "meta":
                break;
        }
    }

    public static void ExecuteServer(byte[] buffer, int port, Guid guid)
    {
        _client.SendAsync("server", new Dictionary<string, object>
        {
            { "buffer", JsonConvert.SerializeObject(buffer) },
            { "server_id", port },
            { "client_id", guid }
        }); // echo
    }

    private static void ServerConnected(object sender, ConnectionEventArgs args)
    {
        Console.WriteLine("Server connected");
    }

    private static void ServerDisconnected(object sender, DisconnectionEventArgs args)
    {
        Console.WriteLine("Server disconnected");
    }

    private static async Task<SyncResponse> SyncRequestReceived(SyncRequest req)
    {
        return new SyncResponse(req, "Hello back at you!");
    }
}