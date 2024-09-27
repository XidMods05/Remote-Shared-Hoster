using System.Text;
using WatsonTcp;

namespace RSH.Test.Project1;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var server = new WatsonTcpServer("0.0.0.0", 9000);
        server.Events.ClientConnected += ClientConnected;
        server.Events.ClientDisconnected += ClientDisconnected;
        server.Events.MessageReceived += MessageReceived;
        server.Callbacks.SyncRequestReceivedAsync = SyncRequestReceived;

        server.Start();

        /*var guid = Guid.NewGuid();
        // send a message
        await server.SendAsync(guid, "Hello, client!");

        // send a message with metadata
        Dictionary<string, object> md = new Dictionary<string, object>();
        md.Add("foo", "bar");
        await server.SendAsync(guid, "Hello, client!  Here's some metadata!", md);

        // send and wait for a response
        try
        {
            SyncResponse resp = await server.SendAndWaitAsync(
                5000, guid,
                "Hey, say hello back within 5 seconds!");

            Console.WriteLine("My friend says: " + Encoding.UTF8.GetString(resp.Data));
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Too slow...");
        } */

        for (;;) ;
    }

    private static void ClientConnected(object sender, ConnectionEventArgs args)
    {
        Console.WriteLine("Client connected: " + args.Client);
    }

    private static void ClientDisconnected(object sender, DisconnectionEventArgs args)
    {
        Console.WriteLine(
            "Client disconnected: "
            + args.Client
            + ": "
            + args.Reason);
    }

    private static void MessageReceived(object sender, MessageReceivedEventArgs args)
    {
        Console.WriteLine(
            "Message from "
            + args.Client
            + ": "
            + Encoding.UTF8.GetString(args.Data));
    }

    private static async Task<SyncResponse> SyncRequestReceived(SyncRequest req)
    {
        Console.WriteLine(Encoding.UTF8.GetString(req.Data) + "                        from ");
        return new SyncResponse(req, "Hello back at you! ddddd");
    }
}