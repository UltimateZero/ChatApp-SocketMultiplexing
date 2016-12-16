using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketMultiplexingLib
{
    public class Client
    {
        private Sender _sender;
        private Receiver _receiver;
        private Socket _socket;

        public Client() : this(null)
        {

        }

        public Client(Socket socket)
        {
            if(socket == null)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                _socket = socket;
                InitializeThreads();
            }


        }

        public void Connect(IPEndPoint endPoint)
        {
            _socket.Connect(endPoint);
            Console.WriteLine("Connected to " + endPoint);
            InitializeThreads();
        }

        private void InitializeThreads()
        {
            _sender = new Sender(_socket);
            _receiver = new Receiver(_socket);
            _receiver.DataReceived += _receiver_DataReceived;

            _sender.Start();
            _receiver.Start();
        }

        private void _receiver_DataReceived(object sender, Events.DataReceivedEventArgs e)
        {
           // Console.WriteLine("Received data for id " + e.Id);
           // Console.WriteLine("Was END: " + e.isEnd);
            if(e.Data != null)
            {
                //Console.WriteLine("Data size: " + e.Data.Length);
               // Console.WriteLine("Data: " + Encoding.ASCII.GetString(e.Data));
            }
        }

        public PacketStatus SendData(byte[] data, bool isUrgent)
        {
            return _sender.SendData(data, isUrgent);
        }
    }
}
