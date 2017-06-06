using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class ServerObject
    {
        static TcpListener tcpListener;
        const int PORT = 8888;
        List<ClientObject> clients = new List<ClientObject>(); 

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, PORT);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {                
                clients[i].Stream.Write(data, 0, data.Length);                
            }
        }

        protected internal void SendUserList(string id)
        {
            String message = "";
            message += "ListUpdate";
            foreach (var client in clients)
                message += ":" + client.UserName;

            int i;
            for (i = 0; clients[i].Id != id; i++) ;

            byte[] data = Encoding.Unicode.GetBytes(message);
            clients[i].Stream.Write(data, 0, data.Length);
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop(); 

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); 
            }
            Environment.Exit(0); 
        }

    }
}
   

