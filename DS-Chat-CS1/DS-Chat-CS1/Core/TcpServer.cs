using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestSockets2
{
    class TcpServer
    {
        TcpListener listener;

        public event EventHandler<Client> NewClient;


        public TcpServer(IPEndPoint endPoint)
        {
            listener = new TcpListener(endPoint);
            listener.Start(100);
            WaitForClients();
        }

        private void WaitForClients()
        {
            Console.WriteLine("Waiting for clients...");
            listener.BeginAcceptTcpClient(new AsyncCallback((asyncResult) =>
            {
                try
                {

                    TcpClient tcpClient = listener.EndAcceptTcpClient(asyncResult);

                    if (tcpClient != null)
                    {
                        Console.WriteLine("Received connection request from: " + tcpClient.Client.RemoteEndPoint.ToString());
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
