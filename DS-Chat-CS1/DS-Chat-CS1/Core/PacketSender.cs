using Priority_Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TestSockets2
{
    class PacketSender
    {
        public static readonly int MAX_LENGTH = 512; //Atomic packet in bytes

        internal event EventHandler<DataReadyEventArgs> DataReady;

        public PacketSender()
        {
            packetsMap = new ConcurrentDictionary<int, PacketObject>();
            priorityQueue = new FastPriorityQueue<PacketObject>(10000);
            _thread = new Thread(Run);
            _thread.Start();
        }


        public void SendData(byte[] data, int priority)
        {
            PacketObject packet = new PacketObject();
            packet.id = Interlocked.Increment(ref idCounter);
            packet.priority = priority;
            packet.data = data;

            if (!packetsMap.TryAdd(packet.id, packet))
            {
                Console.WriteLine("COULDNT ADD PACKET ID " + packet.id);
                throw new Exception();
            }
            Console.WriteLine("Preparing packet with id " + packet.id);
            
            EnqueuePacket(packet);
        }

        private void EnqueuePacket(PacketObject packet)
        {
            lock (lockObj)
            {
                priorityQueue.Enqueue(packet, packet.priority);
                if(priorityQueue.Count == priorityQueue.MaxSize)
                {
                    MessageBox.Show("MAX SIZE");
                }
            }

            Console.WriteLine("Enqueued packet " + packet.id);
        }

        object lockObj = new object();
        List<byte> dataBuffer = new List<byte>();
        PacketObject ignored;
        private void Run()
        {
            while (true)
            {

                lock (lockObj)
                {
                    if (priorityQueue.Count != 0)
                    {

                        Console.WriteLine("Starting to send...");
                        PacketObject head = priorityQueue.First;
                        if (head.cancelled)
                        {
                            priorityQueue.Remove(head);
                            continue;
                        }

                        if (currentId != head.id)
                        {
                            currentId = head.id;
                            dataBuffer.Add(SWITCH); //Switch
                            AddId(dataBuffer, currentId);
                        }


                        int toRead = head.offset + MAX_LENGTH;
                        if (toRead > head.data.Length)
                        {
                            toRead = head.data.Length;
                        }
                        for (int i = head.offset; i < toRead; i++)
                        {
                            Console.WriteLine("Offset: " + i + ", Size: " + head.data.Length + ", toRead: " + toRead);
                            if (head.data[i] >= SWITCH && head.data[i] <= ESC) //Escape
                            {
                                dataBuffer.Add(ESC);
                            }
                            dataBuffer.Add(head.data[i]);
                        }


                        head.offset = toRead;
                        if (head.offset >= head.data.Length)
                        {
                            //Send END
                            dataBuffer.Add(END);
                            AddId(dataBuffer, currentId);

                            priorityQueue.Remove(head);
                            
                            packetsMap.TryRemove(head.id, out ignored);
                            Console.WriteLine("END packet " + head.id);
                        }

                        SendBytes(dataBuffer.ToArray());
                        dataBuffer.Clear();
                    }
                    else
                    {
                        Thread.Sleep(1); //Avoid constant CPU usage
                    }
                }

            }
        }


        private void SendBytes(byte[] data)
        {
            var handler = DataReady;
            if (handler != null)
            {
                handler(this, new DataReadyEventArgs() { Data = data });
            }
        }


        private void AddId(List<byte> buf, int id)
        {
            byte[] idBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(id));
            buf.AddRange(idBytes);
        }


        Thread _thread;

        int idCounter = 0;
        int currentId = -1;
        FastPriorityQueue<PacketObject> priorityQueue;
        ConcurrentDictionary<int, PacketObject> packetsMap;

    

        public static readonly byte SWITCH = PacketReceiver.SWITCH;
        public static readonly byte END = PacketReceiver.END;
        public static readonly byte ESC = PacketReceiver.ESC;

        class PacketObject : FastPriorityQueueNode
        {
            public int id;
            public int priority = 0;
            public int offset = 0;
            public byte[] data;
            public bool cancelled = false;
            public override string ToString()
            {
                return "Packet: id=" + id;
            }
        }

    }
}
