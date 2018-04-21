using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Socks5.Core.TCP
{
    public class Client
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly EndPoint _remoteEndPoint;

        private bool _receiving;

        public EventHandler<ClientEventArgs> OnDisconnect = delegate { };
        public EventHandler<DataEventArgs> OnReceive = delegate { };
        public EventHandler<DataEventArgs> OnSend = delegate { };

        public EndPoint RemoteEndPoint => _remoteEndPoint;

        public Client(TcpClient client)
        {
            _tcpClient = client;
            _remoteEndPoint = _tcpClient.Client.RemoteEndPoint;
            _stream = _tcpClient.GetStream();
        }

        public async Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            if (!_stream.CanWrite)
            {
                throw new Exception("Cannot write to buffer.");
            }

            await _stream.WriteAsync(buffer, offset, count);
            await _stream.FlushAsync();

            OnSend(this, new DataEventArgs(buffer, count));

            return count;
        }

        public async Task<int> ReceiveAsync()
        {
            if (_receiving)
            {
                return -1;
            }

            if (!_stream.CanRead)
            {
                throw new Exception("Cannot read from buffer.");
            }

            _receiving = true;

            var buffer = new byte[_tcpClient.ReceiveBufferSize];
            var received = await _stream.ReadAsync(buffer, 0, _tcpClient.ReceiveBufferSize);

            OnReceive(this, new DataEventArgs(buffer, received));

            _receiving = false;

            return received;
        }

        public int Receive(out byte[] receivedBytes)
        {
            if (!_stream.CanRead)
            {
                throw new Exception("Cannot read from buffer.");
            }
            
            var buffer = new byte[_tcpClient.ReceiveBufferSize];
            var received = _stream.Read(buffer, 0, _tcpClient.ReceiveBufferSize);
            receivedBytes = buffer;

            return received;
        }

        public void Send(byte[] buffer, int offset = 0, int count = 0)
        {
            if (!_stream.CanWrite)
            {
                throw new Exception("Cannot write to buffer.");
            }

            if (count == 0)
            {
                count = buffer.Length;
            }

            _stream.Write(buffer, offset, count);
            _stream.Flush();
        }

        public void Disconnect()
        {
            _tcpClient.Close();
            _stream.Close();
        }
    }
}
