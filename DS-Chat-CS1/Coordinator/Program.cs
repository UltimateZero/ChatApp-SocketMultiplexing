using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS_Chat_CS1.Core.LowLevel;
using DS_Chat_CS1.Core.Protocol;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Coordinator
{
    class UserObject
    {
        public string Username;
        public string Room;
        public IPEndPoint Endpoint;
        public DateTime lastReceived;
        public Client client;

        public override string ToString()
        {
            return Username + "#" + Endpoint + "#" + Room;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
            while(true)
            {
                Console.Read();
            }
        }
        IPEndPoint ownEndpoint;
        TcpServer ownServer;
        List<UserObject> users;
        Thread checkThread;
        Program()
        {
            users = new List<UserObject>();
            ownEndpoint = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 1111);
            ownServer = new TcpServer(ownEndpoint);
            Console.WriteLine("Coordinator started at " + ownEndpoint.ToString());
            ownServer.NewClient += OwnServer_NewClient;

            checkThread = new Thread(RunCheckThread);
            checkThread.Start();
        }

        private void RunCheckThread()
        {
            while(true)
            {
                Thread.Sleep(1000 * 10);
                int removed = users.RemoveAll(u => (DateTime.Now - u.lastReceived).Seconds > 30);
                if(removed != 0)
                    Console.WriteLine("Removed " + removed + " idle clients");
            }
        }

        private void OwnServer_NewClient(object sender, Client client)
        {
            client.PacketReceived += Client_PacketReceived;
            client.ClientDisconnected += Client_ClientDisconnected;
        }

        private void Client_ClientDisconnected(object sender, DS_Chat_CS1.Core.Events.ClientDisconnectedEventArgs e)
        {
            var client = sender as Client;
            int removed = users.RemoveAll( x =>x.client == client  );
            Console.WriteLine("Removed " + removed + " disconnected clients");

        }

        private void Client_PacketReceived(object sender, DS_Chat_CS1.Core.Events.PacketFullyReceivedEventArgs e)
        {
            //Console.WriteLine("Received something");
            var client = sender as Client;
            FyzrPacket packet = FyzrParser.FromData(e.Packet.data.ToArray());

            if (packet.method == FyzrPacket.Method.COMMAND)
            {
                string cmdType = packet.headers["Command-Type"];
                if (cmdType.Equals("requestUsers"))
                {
                    if (!packet.headers.ContainsKey("Username")) return;
                    string username = packet.headers["Username"].Trim();
                    if (username.Length == 0) return;
                    int port = Convert.ToInt32(packet.headers["Listener-Port"]);
                    /*
 * Username#Endpoint#Room
 *    
   */
                    string response = "";
                    foreach (var user in users)
                    {
                        response += user.ToString() + '\n';
                    }
                    response = response.Trim();

                    IPEndPoint endPoint = new IPEndPoint(client.GetRemoteEndPoint().Address, port);
                    var u = new UserObject() { Username = username, Endpoint = endPoint, Room = null, client = client  };
                    users.Add(u);
                    u.lastReceived = DateTime.Now;

                    FyzrPacket responsePacket = new FyzrPacket();
                    responsePacket.method = FyzrPacket.Method.COMMAND;
                    responsePacket.headers.Add("Content-Encoding", Encoding.Default.WebName);
                    responsePacket.body = Encoding.Default.GetBytes(response);
                    responsePacket.headers.Add("Content-Length", "" + responsePacket.body.Length);
                    responsePacket.headers.Add("Command-Type", "usersList");

                    client.SendOrdered(FyzrParser.ToData(responsePacket));


                }
                else if (cmdType.Equals("requestUsersUpdate"))
                {
                    int port = Convert.ToInt32(packet.headers["Listener-Port"]);
                    IPEndPoint endPoint = new IPEndPoint(client.GetRemoteEndPoint().Address, port);
                    var u = users.Find(x => x.Endpoint.Equals(endPoint));
                    u.lastReceived = DateTime.Now;

                    string response = "";
                    foreach (var user in users)
                    {
                        if(user != u)
                             response += user.ToString() + '\n';
                    }
                    response = response.Trim();

                    FyzrPacket responsePacket = new FyzrPacket();
                    responsePacket.method = FyzrPacket.Method.COMMAND;
                    responsePacket.headers.Add("Content-Encoding", Encoding.Default.WebName);
                    responsePacket.body = Encoding.Default.GetBytes(response);
                    responsePacket.headers.Add("Content-Length", "" + responsePacket.body.Length);
                    responsePacket.headers.Add("Command-Type", "usersList");

                    client.SendOrdered(FyzrParser.ToData(responsePacket));

                }
                else if(cmdType.Equals("disconnect"))
                {
                    Console.WriteLine("Disconnect received");
                    client.Disconnect();
                }
                    
             




            }

        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
