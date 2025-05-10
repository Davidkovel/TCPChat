using System.Net;
using System.Net.Sockets;

namespace TCPServer;

class Program
{
    static async Task Main(string[] args)
    {
        var listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        try
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client); // Обработка клиента в фоне
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream))
        {
            try
            {
                // Первое сообщение - имя пользователя
                var userName = await reader.ReadLineAsync();
                Console.WriteLine($"{userName} connected");

                while (true)
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null) break;

                    Console.WriteLine($"{userName}: {message}");
                    await writer.WriteLineAsync($"{userName}: {message}");
                    await writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Client disconnected");
            }
        }
    }
}