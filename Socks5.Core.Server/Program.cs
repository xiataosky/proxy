using System;
using System.Net;
using System.Threading;

namespace Socks5.Core.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SocksServer(IPAddress.Any, 8080);
            server.Start();
            Console.WriteLine("Started server.");
            while (true)
            {
                Thread.Sleep(60000);
            }
        }
    }
}
