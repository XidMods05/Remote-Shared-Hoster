RemoteSharedHost: A Secure and Reliable Traffic Relay This project aims to provide a reliable and secure method for transmitting local traffic to a global network and back. 

Example RemoteSharedHost Client in Test.Project2 

```C#
_client.SendAsync("meta", new Dictionary<string, object>
        {
            { "auth", "77dxzdd" },
            { "link_server", 9339 }
        });
```
- 77dxzdd - RemoteSharedHost Client username
- 9339 - port that will be created on the machine running RemoteSharedHost and to which TCP clients will be able to connect

```C#
public static void ExecuteServer(byte[] buffer, int port, Guid guid)
{
  _client.SendAsync("server", new Dictionary<string, object>
  {
    { "buffer", JsonConvert.SerializeObject(buffer) },
    { "server_id", port },
    { "client_id", guid }
  }); // echo
}
```
- receive the message from server on port (server_id / link_server) (for example 9339)
*(echo receive for example)*

Example usage: RemoteSharedHost dotnet run -l=50 -p=5000
-p - port that RemoteSharedHost Client connects to.
