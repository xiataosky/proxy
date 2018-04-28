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

using System;
using System.Net.Sockets;

namespace socks5.TCP
{
    public class Client
    {
        public event EventHandler<ClientEventArgs> OnClientDisconnected = delegate { };

        public event EventHandler<DataEventArgs> OnDataReceived = delegate { };
        public event EventHandler<DataEventArgs> OnDataSent = delegate { };

        public Socket Sock { get; set; }
        public bool Receiving { get; private set; }

        public Client(Socket sock, int packetSize)
        {
            //start the data exchange.
            Sock = sock;
            _buffer = new byte[packetSize];
            Sock.ReceiveBufferSize = packetSize;
            Sock.SendBufferSize = packetSize;
        }

        public int Receive(byte[] data, int offset, int count)
        {
            try
            {
                if (!IsConnected())
                {
                    Disconnect();
                    return -1;
                }

                var received = Sock.Receive(data, offset, count, SocketFlags.None);
                if (received <= 0)
                {
                    Disconnect();
                    return -1;
                }
                var dargs = new DataEventArgs(this, data, received);
                OnDataReceived(this, dargs);
                return received;
            }
            catch (Exception ex)
            {
                #if DEBUG
                Console.WriteLine(ex.ToString()); 
                #endif 
                Disconnect();
                return -1;
            }
        }

        public void ReceiveAsync(int buffersize = -1)
        {
            try
            {
                if (!IsConnected())
                {
                    Disconnect();
                    return;
                }

                if (buffersize > -1)
                {
                    _buffer = new byte[buffersize];
                }
                Receiving = true;
                Sock.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, DataReceived, Sock);
            }
            catch(Exception ex)
            {
                #if DEBUG
                Console.WriteLine(ex.ToString()); 
                #endif
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (IsConnected())
            {
                _disposed = true;
                OnClientDisconnected(this, new ClientEventArgs(this));
                Sock.Close();
                Dispose();
            }
        }

        public bool Send(byte[] buff)
        {
            return Send(buff, 0, buff.Length);
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            try
            {
                if (!IsConnected())
                {
                    Disconnect();
                    return;
                }

                Sock.BeginSend(buff, offset, count, SocketFlags.None, DataSent, Sock);
            }
            catch (Exception ex)
            {
                #if DEBUG
                Console.WriteLine(ex.ToString()); 
                #endif
                Disconnect();
            }
        }

        public bool Send(byte[] buff, int offset, int count)
        {
            try
            {
                if (!IsConnected())
                {
                    Disconnect();
                    return false;
                }

                if (Sock.Send(buff, offset, count, SocketFlags.None) <= 0)
                {
                    Disconnect();
                    return false;
                }
                var data = new DataEventArgs(this, buff, count);
                OnDataSent(this, data);
                return true;
            }
            catch (Exception ex)
            {
                #if DEBUG
                Console.WriteLine(ex.ToString());
                #endif
                Disconnect();
                return false;
            }
        }

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        private byte[] _buffer;


        private void DataReceived(IAsyncResult res)
        {
            Receiving = false;
            try
            {
                if (!IsConnected())
                {
                    Disconnect();
                    return;
                }

                var received = ((Socket)res.AsyncState).EndReceive(res, out var err);
                if (received <= 0 || err != SocketError.Success)
                {
                    Disconnect();
                    return;
                }

                var data = new DataEventArgs(this, _buffer, received);
                OnDataReceived(this, data);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                Disconnect();
            }
        }

        private void DataSent(IAsyncResult res)
        {
            try
            {
                if (!IsConnected())
                {
                    Disconnect();
                    return;
                }

                var sent = ((Socket) res.AsyncState).EndSend(res);
                if (sent < 0)
                {
                    Sock.Shutdown(SocketShutdown.Both);
                    Sock.Close();
                    return;
                }

                var data = new DataEventArgs(this, new byte[] { }, sent);
                OnDataSent(this, data);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        private bool IsConnected()
        {
            try
            {
                return !(_disposed || Sock == null || !Sock.Connected && Sock.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false; 
            }
        }

        protected virtual void Dispose(bool disposing)
        {

            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (!disposing)
            {
                return;
            }

            Sock = null;
            _buffer = null;
            OnClientDisconnected = null;
            OnDataReceived = null;
            OnDataSent = null;
        }
    }
}
