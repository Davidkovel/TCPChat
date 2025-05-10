using System.Net;
using System.Net.Sockets;

namespace TCPServer;

public interface IServer
{
    Task StartAsync();
}

public class TcpChatServer : IServer
{
    private readonly TcpListener _listener;

    public TcpChatServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        try
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        finally
        {
            _listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream) { AutoFlush = true })
        {
            try
            {
                var userName = await reader.ReadLineAsync();
                if (userName == null) return;

                Console.WriteLine($"{userName} connected (IP: {client.Client.RemoteEndPoint})");

                while (true)
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null) break;

                    var formattedMessage = $"{userName}: {message}";
                    Console.WriteLine(formattedMessage);
                    await writer.WriteLineAsync(formattedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Client disconnected");
            }
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        IServer server = new TcpChatServer(8888);
        await server.StartAsync();
    }
}