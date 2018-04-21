using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Socks5.Core.Entities
{
    public class SocksRequest
    {
        private AddressType _addressType;
        private StreamType _streamType;
        private string _address;
        private int _port;
        private SocksError _socksError;

        public AddressType AddressType => _addressType;
        public StreamType StreamType => _streamType;
        public string Address => _address;
        public int Port => _port;
        public SocksError Error => _socksError;

		public SocksRequest(StreamType type, AddressType addrtype, string address, int port)
		{
		    _addressType = addrtype;
			_streamType = type;
			_address = address;
			_port = port;
			_socksError = SocksError.Granted;
		}

        private IPAddress GetIpAddress()
        {
            if (_addressType == AddressType.Ip)
            {
                try
                {
                    return IPAddress.Parse(_address);
                }
                catch { _socksError = SocksError.NotSupported; return null; }
            }
            else if (_addressType == AddressType.Domain)
            {
                try
                {
                    foreach (IPAddress p in Dns.GetHostAddresses(_address))
                        if (p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            return p;
                    return null;
                }
                catch
                {
                    _socksError = SocksError.HostUnreachable;
                    return null;
                }
            }

            return null;
        }

		public byte[] GetData(bool NetworkToHostOrder)
		{
			byte[] data;
			var port = 0;

		    if (NetworkToHostOrder)
		    {
		        port = IPAddress.NetworkToHostOrder(_port);
		    }
		    else
		    {
		        port = IPAddress.HostToNetworkOrder((short)_port);
		    }

			if (_addressType == AddressType.Ip)
			{
				data = new byte[10];
				string[] content = GetIpAddress().ToString().Split('.');
				for (int i = 4; i < content.Length + 4; i++)
					data[i] = Convert.ToByte(Convert.ToInt32(content[i - 4]));
				Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, 8, 2);
			}
			else if (_addressType == AddressType.Domain)
			{
				data = new byte[_address.Length + 7];
				data[4] = Convert.ToByte(_address.Length);
				Buffer.BlockCopy(Encoding.ASCII.GetBytes(_address), 0, data, 5, _address.Length);
				Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, data.Length - 2, 2);
			}
			else return null;
			data[0] = 0x05;                
			data[1] = (byte)_socksError;
			data[2] = 0x00;
			data[3] = (byte)_addressType;
			return data;
		}
	}
}
