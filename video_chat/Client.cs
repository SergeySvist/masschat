using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace video_chat
{
    internal class Client
    {
        
        ConnectionSettings cs;
        TcpClient client = new TcpClient();
        NetworkStream ns;
        private string ip;
        public Client(ConnectionSettings connectionSettings)
        {
            cs = connectionSettings;
            string hostName = Dns.GetHostName();
            ip = Dns.GetHostByName(hostName).AddressList[1].ToString();
        }

        public void Connect()
        {
            client.Connect(cs.Ip, cs.Port);
            ns = client.GetStream();
            SendMessage($"{ip},{cs.Nickname}");
            //SendMessage(ip);
        }

        public void SendMessage(string message)
        {
            ns.Write(Encoding.UTF8.GetBytes(message));
        }

        public string ReceiveMessage()
        {
            byte[] buffer = new byte[1024];
            int byteCount = 0;
            string message = string.Empty;

            do
            {
                byteCount = ns.Read(buffer, 0, buffer.Length);
                message += Encoding.UTF8.GetString(buffer, 0, byteCount);
            } while (ns.DataAvailable);

            return message;
        }
        

    }
}
