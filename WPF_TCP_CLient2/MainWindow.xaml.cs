using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_TCP_CLient2;

public partial class MainWindow : Window
{
    private TcpListener _tcpListener;
    private TcpClient _connectedClient;

    public MainWindow()
    {
        InitializeComponent();
        StartServerAsync();
    }

    private async void StartServerAsync()
    {
        _tcpListener = new TcpListener(IPAddress.Any, 8888);// Port number can be changed
        try
        {
            _tcpListener.Start();
            ChatMessages.Items.Add("Server started. Waiting for connections...");

            while (true)
            {
                _connectedClient = await _tcpListener.AcceptTcpClientAsync();
                ChatMessages.Items.Add($"Incoming connection: {_connectedClient.Client.RemoteEndPoint}");
                _ = HandleClientAsync(_connectedClient);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Server error: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        using (tcpClient)
        {
            var buffer = new byte[1024];
            var stream = tcpClient.GetStream();

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await Dispatcher.InvokeAsync(() => ChatMessages.Items.Add($"Client: {message}"));
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => ChatMessages.Items.Add($"Client error: {ex.Message}"));
            }
        }
    }

    private async void SendMessage_Click(object sender, RoutedEventArgs e)
    {
        if (_connectedClient == null || !_connectedClient.Connected)
        {
            MessageBox.Show("No client connected.");
            return;
        }

        try
        {
            string message = MessageInput.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                var stream = _connectedClient.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                ChatMessages.Items.Add($"Server: {message}");
                MessageInput.Clear();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending message: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _tcpListener?.Stop();
        _connectedClient?.Close();
    }
}