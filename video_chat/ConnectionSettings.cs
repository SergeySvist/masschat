using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace video_chat
{
    internal struct ConnectionSettings
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string Nickname { get; set; }

        public ConnectionSettings(string ip, int port, string nickname)
        {
            Ip = ip;
            Port = port;
            Nickname = nickname;
        }
    }
}
