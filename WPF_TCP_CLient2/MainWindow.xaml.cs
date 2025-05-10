using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    
    namespace WPF_TCP_Client;
    
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
    
        public MainWindow()
        {
            InitializeComponent();
            ConnectToServerAsync();
        }
    
        private async void ConnectToServerAsync()
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync("127.0.0.1", 8888); // Connect to the server
                _networkStream = _tcpClient.GetStream();
                ChatMessages.Items.Add("Connected to the server.");
    
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }
    
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
    
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await Dispatcher.InvokeAsync(() => ChatMessages.Items.Add(message));
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => ChatMessages.Items.Add($"Error: {ex.Message}"));
            }
        }
    
        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageInput.Text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await _networkStream.WriteAsync(buffer, 0, buffer.Length);
    
                    ChatMessages.Items.Add($"You: {message}");
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
            _networkStream?.Close();
            _tcpClient?.Close();
        }
    }