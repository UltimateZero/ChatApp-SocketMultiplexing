using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSockets2
{
    partial class Client : IDisposable
    {  // Called by producers to send data over the socket.

        public void SendData(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            SendData(bytes, 1);
        }

        public void SendData(byte[] data, int priority)
        {
            _packetsender.SendData(data, priority);
        }
        // Consumers register to receive data.
        public event EventHandler<PacketFullyReceivedEventArgs> PacketReceived;


        public static ManualResetEvent ShutdownEvent = new ManualResetEvent(false);


        public Client() 
            : this(null)
        {
            
        }
        public Client(TcpClient client)
        {
            if (client == null) //New connection
            {
                _client = new TcpClient();
            }
            else //Alreadyconnected
            {
                _client = client;
                PrepareStreams();
            }
            
        }

        public void StartConnect(IPEndPoint endPoint)
        {
            _client.Connect(endPoint);
            PrepareStreams();
            Console.WriteLine("Connected to endpoint: " + endPoint);
        }

        private void PrepareStreams()
        {
            _stream = _client.GetStream();

            _receiver = new Receiver(_stream);
            _sender = new Sender(_stream);

            _packetreceiver = new PacketReceiver();
            _packetsender = new PacketSender();

            _receiver.DataReceived += _packetreceiver.OnDataReceived;
            _packetreceiver.PacketReceived += OnPacketReceived;
            _packetsender.DataReady += OnReadyToSend;
        }

        private void OnReadyToSend(object sender, DataReadyEventArgs e)
        {
            _sender.SendData(e.Data);
        }

        private void OnPacketReceived(object sender, PacketFullyReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Console.WriteLine("Received full packet: " );
            Console.WriteLine("[{0}]", string.Join(", ", (Array.ConvertAll(packet.data.ToArray(), c => (int)c))));
            Console.WriteLine("Full packet as string: " + Encoding.ASCII.GetString(packet.data.ToArray(), 0, packet.data.Count));
            var handler = PacketReceived;
            if (handler != null) PacketReceived(this, e);  // re-raise event to outer subscribers

        }

        public void Dispose()
        {
            Client.ShutdownEvent.Set();
        }

        private PacketSender _packetsender;
        private PacketReceiver _packetreceiver;
        private TcpClient _client;
        private NetworkStream _stream;
        private Receiver _receiver;
        private Sender _sender;
    }
}
