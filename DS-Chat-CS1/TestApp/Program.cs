using SocketMultiplexingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {



        static void Main(string[] args)
        {
            var ServerEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
            TcpServer server = new TcpServer(ServerEndpoint);

            Client client = new Client();
            client.Connect(ServerEndpoint);
            string test = "";
            for(int i = 0; i < 100000; i++)
            {
                test += 'a';
            }
            var state = client.SendData(Encoding.ASCII.GetBytes(test), true);
            for(int i = 0; i < 10000; i++)
            Console.WriteLine(state.Progress);
            Console.WriteLine("SENT");

            while (true)
                Console.Read();
        }


    }
}
