using System;

namespace Socks5.Core.TCP
{
    public class ClientEventArgs : EventArgs
    {
        private Client _client;

        public Client Client => _client;

        public ClientEventArgs(Client client)
        {
            _client = client;
        }
    }
}
