using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketMultiplexingLib
{
    public class TcpServer
    {
        public Socket listener;

        public event EventHandler<Client> NewClient;


        public TcpServer(IPEndPoint endPoint)
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(100);
            WaitForClients();
        }

        private void WaitForClients()
        {
            Console.WriteLine("Waiting for clients on " + listener.LocalEndPoint);
            listener.BeginAccept(new AsyncCallback((asyncResult) =>
            {
                try
                {

                    Socket tcpClient = listener.EndAccept(asyncResult);

                    if (tcpClient != null)
                    {
                        Console.WriteLine("Received connection request from: " + tcpClient.RemoteEndPoint.ToString());
                        Client client = new Client(tcpClient);
                        HandleNewClient(client);

                    }

                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw e;
                }

                //Continue waiting for more clients
                WaitForClients();

            }), null);
        }


        private void HandleNewClient(Client client)
        {
            var handler = NewClient;
            if(handler != null)
            {
                handler(this, client);
            }
        }
    }
}
