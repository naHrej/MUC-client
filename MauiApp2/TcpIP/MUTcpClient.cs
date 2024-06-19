using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MauiApp2.TcpIP
{
    public delegate void MessageReceivedDelegate(string message);

    public class MuTcpClient
    {
        private readonly TcpClient _client;
        public Guid Id { get; }
        private readonly MessageReceivedDelegate _messageReceivedDelegate;

        public MuTcpClient(string host, int port, MessageReceivedDelegate messageReceivedDelegate)
        {
            _client = new TcpClient();
            
            Id = Guid.NewGuid();
            _messageReceivedDelegate = messageReceivedDelegate;
            ConnectAsync(host, port);
        }

        private async void ConnectAsync(string host, int port)
        {
            await _client.ConnectAsync(host, port);
            Task.Run(ReceiveMessages);
            
            // set keep alive
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            // set keep alive time
            _client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 60);
        }

        public async Task SendMessage(string message)
        {
            var stream = _client.GetStream();
            var writer = new StreamWriter(stream);

            // Format the message for the LambdaMOO server
            // Each command is a line of text, with arguments separated by spaces
            // The line of text is terminated with a newline character
            message = message.Trim() + "\n";

            // Convert the message to bytes
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);

            // Send the bytes to the server
            await stream.WriteAsync(bytes, 0, bytes.Length);
            await writer.FlushAsync();
        }

        public async void ReceiveMessages()
        {
            var stream = _client.GetStream();
            var reader = new StreamReader(stream);

            while (true)
            {
                var message = await reader.ReadLineAsync();
                if (message != null) _messageReceivedDelegate?.Invoke(message);
            }
        }
    }
}