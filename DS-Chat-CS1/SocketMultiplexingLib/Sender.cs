using SocketMultiplexingLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketMultiplexingLib
{
    public class PacketStatus
    {
        public int id;
        public ManualResetEvent isDone = new ManualResetEvent(false);
        public bool isCancelled = false;
        public int Progress { get; set; }
    }
    class PacketObject
    {
        public int offset = 0;
        public byte[] data;
        public PacketStatus status = new PacketStatus();

    }
    public class Sender
    {
        private static readonly int QUANTOM_SIZE = 256;
        private object lockObj = new object();


        public event EventHandler<DataSentEventArgs> DataSent;

        private Socket _socket;
        private Thread _thread;

        private Queue<PacketObject> urgentQueue;
        private Queue<PacketObject> randomQueue;
        private int idCounter = 0;
        private int currentId = -1;
        private List<byte> dataBuffer;

        public Sender(Socket socket)
        {
            urgentQueue = new Queue<PacketObject>();
            randomQueue = new Queue<PacketObject>();
            dataBuffer = new List<byte>();

            _socket = socket;
            _thread = new Thread(Run);
            
        }

        public void Start()
        {
            _thread.Start();
        }


        public PacketStatus SendData(byte[] data, bool isUrgent)
        {
            var packet = new PacketObject();
            packet.data = data;
            packet.status.id = Interlocked.Increment(ref idCounter);
            lock(lockObj)
            {
                if (isUrgent)
                {
                    urgentQueue.Enqueue(packet);
                }
                else
                {
                    randomQueue.Enqueue(packet);
                }
            }
            Console.WriteLine("Enqueued packet for sending");
            return packet.status;
        }






        private void Run()
        {
            while(true)
            {
                lock(lockObj)
                {
                    if (urgentQueue.Count != 0)
                    {
                        var packet = urgentQueue.Peek();
                        if (SendPacketObject(packet))
                        {
                            packet.status.isDone.Set();
                            urgentQueue.Dequeue();
                        }

                    }
                    else if (randomQueue.Count != 0)
                    {
                        var packet = randomQueue.Peek();
                        if (SendPacketObject(packet))
                        {
                            packet.status.isDone.Set();
                            randomQueue.Dequeue();
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }

            }
        }
        

        private bool SendPacketObject(PacketObject packet)
        {
            if (packet.status.isCancelled)
            {
                return true;
            }
            else
            {
                bool wasEnd = false;
                if (currentId != packet.status.id)
                {
                    currentId = packet.status.id;
                    dataBuffer.Add(Constants.SWITCH);
                    AddId(dataBuffer, packet.status.id);
                }


                int toRead = packet.offset + QUANTOM_SIZE;
                if (toRead > packet.data.Length)
                {
                    toRead = packet.data.Length;
                }

                for (int i = packet.offset; i < toRead; i++)
                {
                    if (packet.data[i] >= Constants.SWITCH && packet.data[i] <= Constants.ESC) //Escape
                    {
                        dataBuffer.Add(Constants.ESC);
                    }
                    dataBuffer.Add(packet.data[i]);
                }

                
                packet.offset = toRead;
                if (packet.offset >= packet.data.Length)
                {
                    packet.status.Progress = 100;
                    //Send END
                    dataBuffer.Add(Constants.END);
                    AddId(dataBuffer, packet.status.id);
                    wasEnd = true;
                } 
                else
                {
                    packet.status.Progress = (int)(packet.offset * 100.0 / packet.data.Length);
                }

                SendBytes(dataBuffer.ToArray());
                dataBuffer.Clear();

                return wasEnd;
            }
        }


        private void SendBytes(byte[] data)
        {
            _socket.Send(data);
        }

        static void AddId(List<byte> data, int id)
        {
            byte[] idBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(id));
            data.AddRange(idBytes);
        }
    }
}
