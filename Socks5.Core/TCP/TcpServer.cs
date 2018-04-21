using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Socks5.Core.TCP
{
    public class TcpServer
    {
        public event EventHandler<ClientEventArgs> Connect = delegate { }; 
        public event EventHandler<ClientEventArgs> Disconnect = delegate { }; 

        private readonly TcpListener _tcpListener;
        private readonly int _backLog = 10000;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private bool _isStarted;

        public TcpServer(IPAddress ipAddress, int port)
        {
            _tcpListener = new TcpListener(ipAddress, port);
        }

        public void Start()
        {
            if (!_isStarted)
            {
                _isStarted = true;
                _tcpListener.Start(_backLog);
                Task.Run(async () => { await Accept(); });
            }
        }

        public void Stop()
        {
            if (_isStarted)
            {
                _tokenSource.Cancel();
            }
        }

        private async Task Accept()
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());
            _token = _tokenSource.Token;

            try
            {
                while (!_token.IsCancellationRequested)
                {
                    await Task.Run(async () =>
                    {
                        var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                        Connect(this, new ClientEventArgs(new Client(tcpClient)));
                    }, _token);
                }
            }
            finally
            {
                _tcpListener.Stop();
                _isStarted = false;
            }
        }
    }
}