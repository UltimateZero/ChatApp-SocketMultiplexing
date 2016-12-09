using DS_Chat_CS1.Core.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.LowLevel
{
    partial class Client : IDisposable
    {  // Called by producers to send data over the socket.

        //public void SendData(string data)
        //{
        //    byte[] bytes = Encoding.ASCII.GetBytes(data);
        //    SendData(bytes, 0);
        //}

        


        public int SendData(byte[] data, int priority)
        {
            return _packetsender.SendData(data, priority);
        }


        public int SendOrdered(byte[] data)
        {
            return SendData(data, 0);
        }

        public int SendRandom(byte[] data)
        {
            return SendData(data, 3);
        }

        // Consumers register to receive data.
        public event EventHandler<PacketFullyReceivedEventArgs> PacketReceived;
        public event EventHandler<PacketSentEventArgs> PacketSent;


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
            else //Already connected
            {
                _client = client;
                PrepareStreams();
            }
            _client.NoDelay = true;
            
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

            _packetsender.PacketSent += onPacketSent;
        }



        private void OnReadyToSend(object sender, DataReadyEventArgs e)
        {
            _sender.SendData(e.Data);
        }

        private void OnPacketReceived(object sender, PacketFullyReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Console.WriteLine("Received packet " + packet.id);
            //Console.WriteLine("Received full packet: " );
            //Console.WriteLine("[{0}]", string.Join(", ", (Array.ConvertAll(packet.data.ToArray(), c => (int)c))));
            //Console.WriteLine("Full packet as string: " + Encoding.ASCII.GetString(packet.data.ToArray(), 0, packet.data.Count));
            var handler = PacketReceived;
            if (handler != null) PacketReceived(this, e);  // re-raise event to outer subscribers
        }

        private void onPacketSent(object sender, PacketSentEventArgs e)
        {
            Console.WriteLine("Sent packet " + e.PacketId);
            var handler = PacketSent;
            if (handler != null) PacketSent(this, e);  // re-raise event to outer subscribers
        }

        public void Dispose()
        {
            Client.ShutdownEvent.Set();
        }

        internal IPEndPoint GetRemoteEndPoint()
        {
            return (IPEndPoint) _client.Client.RemoteEndPoint;
        }

        private PacketSender _packetsender;
        private PacketReceiver _packetreceiver;
        private TcpClient _client;
        private NetworkStream _stream;
        private Receiver _receiver;
        private Sender _sender;
    }
}
