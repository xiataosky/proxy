using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Socks5.Core.Entities
{
    public class User
    {
        private string _username;
        private string _password;
        private IPEndPoint _ip;

        public string Username
        {
            get => _username;
            private set => _username = value;
        }

        public string Password
        {
            get => _password;
            private set => _password = value;
        }

        public IPEndPoint Ip
        {
            get => _ip;
            private set => _ip = value;
        }

        public User(string un, string pw, IPEndPoint ip)
        {
            _username = un;
            _password = pw;
            _ip = ip;
        }
    }
}
