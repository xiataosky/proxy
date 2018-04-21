using System;

namespace Socks5.Core.TCP
{
    public class DataEventArgs : EventArgs
    {
        private byte[] _buffer;
        private int _count;
        private int _offset;

        public byte[] Buffer => _buffer;
        public int Count => _count;
        public int Offset => _offset;
        
        public DataEventArgs(byte[] buffer, int count)
        {
            _buffer = buffer;
            _count = count;
            _offset = 0;
        }
    }
}
