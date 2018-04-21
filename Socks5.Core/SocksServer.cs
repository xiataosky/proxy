using System;
using System.Net;
using Socks5.Core.TCP;
using Socks5.Core.Socks;

namespace Socks5.Core
{
    public class SocksServer
    {
        private TcpServer _tcpServer;

        public SocksServer(IPAddress ipAddress, int port)
        {
            _tcpServer = new TcpServer(ipAddress, port);
            _tcpServer.Connect += async (sender, item) =>
            {
                item.Client.OnDisconnect += (o, args) =>
                {
                    Console.WriteLine("Client disconnected.");
                };
                try
                {
                    await SocksRoutine.Begin(item.Client);
                }
                catch (Exception ex)
                {
                    item.Client.Disconnect();
                    Console.WriteLine(ex);
                }
            };
        }

        public void Start()
        {
            _tcpServer.Start();
        }

        public void Stop()
        {
            _tcpServer.Stop();
        }
    }
}
