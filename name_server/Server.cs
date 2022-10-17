using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace name_server
{
    internal class Server
    {
        private string host;
        private int port;

        private TcpListener? listener;

        private List<Client> clients = new List<Client>();

        internal Server(string host = "192.168.0.105", int port = 8080)
        {
            this.host = host;
            this.port = port;
        }

        internal void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse(host), port);
                listener.Start();

                Console.WriteLine($"Server started at {host}:{port}");

                while (true)
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();

                    Client client = new Client(tcpClient);

                    client.ClientConnection += ClientConnectedHandler;
                    client.MessageReceived += MessageReceivedHandler;
                    clients.Add(client);

                    Task.Run(() => client.Processing());
                }




            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        private void MassSending(string message, Client client)
        {
            string sendMessage = $"{message}";

            Parallel.ForEach(clients, (c) =>
            {
                //if (c.Id != client.Id)
                c.Send(sendMessage);
            });
        }

        private void DmSending(string message, Client receiver)
        {
            receiver.Send(message);
        }

        internal void ClientConnectedHandler(object? sender, EventArgs args)
        {
            Client? client = sender as Client;
            string sendMessage = "users=";
            foreach (Client c in clients)
            {
                if(c.IsConnected == true)
                    sendMessage += $"{c.Ip},{c.Nickname},{c.Id};";
            }

            MassSending(sendMessage, client);
            
            Console.WriteLine($"{client.Nickname}=>{client.Ip} connected.....");
        }

        internal void MessageReceivedHandler(object? sender, ClientMessageEventArgs args)
        {
            Client? client = sender as Client;
            if (args.Message.IndexOf("dmto=") != -1)
            {
                string res = args.Message.Substring(args.Message.IndexOf("dmto=") + "dmto=".Length);
                string uid = res.Substring(0, res.IndexOf("|"));
                string message = res.Substring(res.IndexOf("|")+1);
                Client clientTo = clients.Where(c => c.Id == uid).FirstOrDefault();

                DmSending(message, clientTo);
                Console.WriteLine($"{client.Nickname} : {message} => dm to {clientTo.Nickname}");
            }
            else if(args.Message == "unlogin")
            {
                client.IsConnected = false;
                ClientConnectedHandler(clients[0], EventArgs.Empty);
            }
            else if (args.Message == "login")
            {
                client.IsConnected = true;
                ClientConnectedHandler(clients[0], EventArgs.Empty);
            }
            else if (args.Message == "exit")
            {
                clients.Remove(client);
                ClientConnectedHandler(clients[0], EventArgs.Empty);
            }
            else if (args.Message.IndexOf("mass=") != -1)
            {
                string res = args.Message.Substring(args.Message.IndexOf("mass=") + "mass=".Length);

                MassSending($"{client.Nickname} => {res}", client);
                Console.WriteLine($"{client.Nickname} : {args.Message}");
            }
        }
    }
}
