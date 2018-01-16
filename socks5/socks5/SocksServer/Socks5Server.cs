/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using socks5.TCP;
using System.Net;
using socks5.Plugin;
using socks5.Socks;
namespace socks5
{
    public class Socks5Server
    {
        public int Timeout { get; set; }
        public int PacketSize { get; set; }
        public bool LoadPluginsFromDisk { get; set; }
        public IPAddress OutboundIpAddress { get; set; }

        private readonly TcpServer _server;

        public List<SocksClient> Clients = new List<SocksClient>();

        private bool started;

        public Socks5Server(IPAddress ip, int port)
        {
            Timeout = 5000;
            PacketSize = 4096;
            LoadPluginsFromDisk = false;
            OutboundIpAddress = IPAddress.Any;
            _server = new TcpServer(ip, port);
            _server.onClientConnected += _server_onClientConnected;
        }

        public void Start()
        {
            if (started) return;
            PluginLoader.LoadPluginsFromDisk = LoadPluginsFromDisk;
            PluginLoader.LoadPlugins(); 
            _server.PacketSize = PacketSize;
            _server.Start();
            started = true;
        }

        public void Stop()
        {
            if (!started) return;
            _server.Stop();
            foreach (var c in Clients)
            {
                c.Client.Disconnect();
            }
            Clients.Clear();
            started = false;
        }

        private void _server_onClientConnected(object sender, ClientEventArgs e)
        {
            //call plugins related to ClientConnectedHandler.
            foreach (ClientConnectedHandler cch in PluginLoader.LoadPlugin(typeof(ClientConnectedHandler)))
            {
				try
				{
				    if (cch.OnConnect(e.Client, (IPEndPoint) e.Client.Sock.RemoteEndPoint))
				    {
				        continue;
				    }

				    e.Client.Disconnect();
				    return;
				}
				catch
				{
				}
            }
            var client = new SocksClient(e.Client);
            client.OnClientDisconnected += client_onClientDisconnected;
            Clients.Add(client);
            client.Begin(OutboundIpAddress, PacketSize, Timeout);
        }

        private void client_onClientDisconnected(object sender, SocksClientEventArgs e)
        {
            e.Client.OnClientDisconnected -= client_onClientDisconnected;
            foreach (ClientDisconnectedHandler cdh in PluginLoader.LoadPlugin(typeof(ClientDisconnectedHandler)))
            {
                cdh.OnDisconnected(sender, e);
            }
            Clients.Remove(e.Client);            
        }
    }
}
