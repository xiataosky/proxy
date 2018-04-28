using System;
using System.Collections.Generic;
using System.Text;
using socks5;
using socks5.HTTP;
namespace PacketEditor.Plugins
{
    class DataReceived : socks5.Plugin.DataHandler
    {
        public override bool OnStart()
        {
            return true;
        }

        string replaceWith = "X-Requested-With: Socks5Debugger-Thr";

        private bool enabled = true;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public override void OnClientDataReceived(object sender, socks5.TCP.DataEventArgs e)
        {
            //if data is HTTP, make sure it's not compressed so we can capture it in plaintext.
            if (e.Buffer.FindString(" HTTP/1.1") != -1 && e.Buffer.FindString("Accept-Encoding") != -1)
            {
                int x = e.Buffer.FindString("Accept-Encoding:");
                int y = e.Buffer.FindString("\r\n", x + 1);
                e.Buffer = e.Buffer.ReplaceBetween(x, y, Encoding.ASCII.GetBytes(replaceWith));
                e.Buffer = e.Buffer.ReplaceString("HTTP/1.1", "HTTP/1.0");
                e.Count = e.Count - (y - x) + replaceWith.Length;
            }
            Utils.Add(new DataCapture.Data(e.Request, e.Buffer, e.Count, DataCapture.DataType.Sent));
        }

        public override void OnServerDataReceived(object sender, socks5.TCP.DataEventArgs e)
        {
            Utils.Add(new DataCapture.Data(e.Request, e.Buffer, e.Count, DataCapture.DataType.Received));
        }
    }
}
