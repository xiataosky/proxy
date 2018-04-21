using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Socks5.Core.Entities;
using Socks5.Core.TCP;

namespace Socks5.Core.Socks
{
    internal class SocksTunnel
    {
        private SocksRequest _request;

        private Client _client;
        private Client _remoteClient;

        public SocksTunnel(Client client, SocksRequest request)
        {
            _client = client;
            _request = request;
        }

        public async Task Open()
        {
            Console.WriteLine("{0}:{1}", _request.Address, _request.Port);

            if (_request.Error != SocksError.Granted)
            {
                _client.Send(_request.GetData(true));
                _client.Disconnect();
            }

            var remote = new TcpClient();
            await remote.ConnectAsync(_request.Address, _request.Port);

            var requestData = _request.GetData(true);
            if (!remote.Connected)
            {
                requestData[1] = (byte) SocksError.Unreachable;
            }
            else
            {
                requestData[1] = (byte) HeaderTypes.Zero;
            }

            _client.Send(requestData);

            _remoteClient = new Client(remote);

            _client.OnReceive += async (sender, e) =>
            {
                
            };

            _remoteClient.OnReceive += async (sender, e) =>
            {
                
            };
        }
    }
}
