using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Socks5.Core.Entities;
using Socks5.Core.TCP;

namespace Socks5.Core.Socks
{
    internal class SocksRoutine
    {
        public static async Task Begin(Client client)
        {
            var authTypes = RequestAuth(client);

            if (authTypes.Count <= 0)
            {
                client.Send(new [] { (byte)HeaderTypes.Zero, (byte)HeaderTypes.FF });
                client.Disconnect();
            }

            client.Send(new [] { (byte)HeaderTypes.Socks5, (byte)HeaderTypes.Zero });

            var request = RequestTunnel(client);

            if (request != null)
            {
                var tunnel = new SocksTunnel(client, request);
                await tunnel.Open();
            }
        }

        private static List<AuthTypes> RequestAuth(Client client)
        {
            var received = client.Receive(out var buffer);

            if (received <= 0 || buffer == null || (HeaderTypes) buffer[0] != HeaderTypes.Socks5)
            {
                return null;
            }

            var methods = Convert.ToInt32(buffer[1]);
            var authTypes = new List<AuthTypes>();

            for (var i = 2; i < methods + 2; i++)
            {
                authTypes.Add((AuthTypes)buffer[i]);
            }

            return authTypes;
        }

        private static User RequestLogin(Client client)
        {
            //request authentication.
            client.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.Login });
            var recv = client.Receive(out var buff);

            if (recv <= 0 || buff == null || buff[0] != 0x01)
            {
                return null;
            }

            var numusername = Convert.ToInt32(buff[1]);
            var numpassword = Convert.ToInt32(buff[(numusername + 2)]);

            var username = Encoding.ASCII.GetString(buff, 2, numusername);
            var password = Encoding.ASCII.GetString(buff, numusername + 3, numpassword);

            return new User(username, password, (IPEndPoint)client.RemoteEndPoint);
        }

        private static SocksRequest RequestTunnel(Client client)
        {
            var recv = client.Receive(out var buff);

            if (recv <= 0 || buff == null || (HeaderTypes) buff[0] != HeaderTypes.Socks5)
            {
                return null;
            }

            switch ((StreamType)buff[1])
            {
                case StreamType.Stream:
                {
                    var fwd = 4;
                    var address = "";

                    switch ((AddressType)buff[3])
                    {
                        case AddressType.Ip:
                        {
                            for (int i = 4; i < 8; i++)
                            {
                                //grab IP.
                                address += Convert.ToInt32(buff[i]).ToString() + (i != 7 ? "." : "");
                            }
                            fwd += 4;
                        }
                            break;
                        case AddressType.Domain:
                        {
                            int domainlen = Convert.ToInt32(buff[4]);
                            address += Encoding.ASCII.GetString(buff, 5, domainlen);
                            fwd += domainlen + 1;
                        }
                            break;
                        case AddressType.Ipv6:
                            //can't handle IPV6 traffic just yet.
                            return null;
                    }

                    var po = new byte[2];
                    Array.Copy(buff, fwd, po, 0, 2);
                    var port = BitConverter.ToUInt16(new byte[] { po[1], po[0] }, 0);
                    return new SocksRequest(StreamType.Stream, (AddressType)buff[3], address, port);
                }
                default:
                    //not supported.
                    return null;

            }
        }
    }
}
