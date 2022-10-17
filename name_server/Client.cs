using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace name_server
{
    internal class Client
    {
        public event EventHandler<EventArgs> ClientConnection;
        public event EventHandler<ClientMessageEventArgs> MessageReceived;

        private TcpClient tcpClient;
        private NetworkStream stream;
        public string Id { get; private set; }
        public string Nickname { get; private set; }
        public string Ip { get; private set; }
        public bool IsConnected { get; set; }
        internal Client(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            Id = Guid.NewGuid().ToString();


        }

        internal void Processing()
        {
            try
            {
                stream = tcpClient.GetStream();
                string[] clientinf = DecodeStreamData().Split(",");
                Nickname = clientinf[1];
                Ip = clientinf[0];
                IsConnected = true;

                ClientConnection?.Invoke(this, EventArgs.Empty);

                while (true)
                {
                    try
                    {
                        string message = DecodeStreamData();            // BLOCKING
                        MessageReceived?.Invoke(this, new ClientMessageEventArgs(DateTime.Now, message));
                    }
                    catch (Exception)
                    {
                        // TODO: ???

                    }
                }

            }
            catch (Exception)
            {
                // TODO: ???
            }
        }

        internal void Send(string message)
        {
            stream.Write(Encoding.UTF8.GetBytes(message));
        }


        private string DecodeStreamData()
        {
            byte[] buffer = new byte[1024];
            int byteCount = 0;
            string message = string.Empty;

            do
            {
                byteCount = stream.Read(buffer, 0, buffer.Length);
                message += Encoding.UTF8.GetString(buffer, 0, byteCount);
            } while (stream.DataAvailable);

            return message;
        }
    }








}
