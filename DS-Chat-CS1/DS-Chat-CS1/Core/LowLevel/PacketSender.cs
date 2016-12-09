using DS_Chat_CS1.Core.Events;
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

namespace DS_Chat_CS1.Core.LowLevel
{
    public class PacketSender
    {
        public static readonly int MAX_LENGTH = 512; //Atomic packet in bytes

        internal event EventHandler<DataReadyEventArgs> DataReady;
        internal event EventHandler<PacketSentEventArgs> PacketSent;

        public PacketSender()
        {
            packetsMap = new ConcurrentDictionary<int, PacketObject>();
            priorityQueue = new FastPriorityQueue<PacketObject>(10000);
            urgentQueue = new Queue<PacketObject>();
            _thread = new Thread(Run);
            _thread.Start();
        }


        public int SendData(byte[] data, int priority)
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
            //Console.WriteLine("Preparing packet with id " + packet.id);

            EnqueuePacket(packet);
            return packet.id;
        }

        private void EnqueuePacket(PacketObject packet)
        {
            lock (lockObj)
            {
                if (packet.priority < 1)
                {
                    urgentQueue.Enqueue(packet);
                }
                else
                {
                    priorityQueue.Enqueue(packet, packet.priority);
                    if (priorityQueue.Count == priorityQueue.MaxSize)
                    {
                        MessageBox.Show("MAX SIZE");
                    }
                }

            }

            //Console.WriteLine("Enqueued packet " + packet.id);
        }

        object lockObj = new object();
        List<byte> dataBuffer = new List<byte>();
        PacketObject ignored;
        bool wasEnd = false;
        private void Run()
        {
            while (true)
            {

                lock (lockObj)
                {
                    if (urgentQueue.Count != 0)
                    {
                        PacketObject head = urgentQueue.Peek();

                        if (SendPacketObject(head)) //remove from queues?
                        {
                            urgentQueue.Dequeue();
                            packetsMap.TryRemove(head.id, out ignored);
                            var handler = PacketSent;
                            if (handler != null)
                            {
                                handler(this, new PacketSentEventArgs() { PacketId = head.id });
                            }
                        }

                    }
                    else if (priorityQueue.Count != 0)
                    {

                        PacketObject head = priorityQueue.First;
                        if (SendPacketObject(head))
                        {
                            priorityQueue.Remove(head);
                            packetsMap.TryRemove(head.id, out ignored);
                            var handler = PacketSent;
                            if (handler != null)
                            {
                                handler(this, new PacketSentEventArgs() { PacketId = head.id });
                            }
                        }

                    }
                    else
                    {
                        Thread.Sleep(1); //Avoid constant CPU usage
                    }
                }

            }
        }

        /*
         return true if packet should be removed from queue
             */
        private bool SendPacketObject(PacketObject packet)
        {
            if (packet.cancelled)
            {

                return true;
            }

            if (currentId != packet.id)
            {
                currentId = packet.id;
                dataBuffer.Add(SWITCH); //Switch
                AddId(dataBuffer, currentId);
            }


            int toRead = packet.offset + MAX_LENGTH;
            if (toRead > packet.data.Length)
            {
                toRead = packet.data.Length;
            }
            for (int i = packet.offset; i < toRead; i++)
            {
                //Console.WriteLine("Offset: " + i + ", Size: " + packet.data.Length + ", toRead: " + toRead);
                if (packet.data[i] >= SWITCH && packet.data[i] <= ESC) //Escape
                {
                    dataBuffer.Add(ESC);
                }
                dataBuffer.Add(packet.data[i]);
            }


            packet.offset = toRead;
            if (packet.offset >= packet.data.Length)
            {
                //Send END
                dataBuffer.Add(END);
                AddId(dataBuffer, currentId);

               // Console.WriteLine("END packet " + packet.id);
                wasEnd = true;

            }

            SendBytes(dataBuffer.ToArray());
            dataBuffer.Clear();

            if (wasEnd)
            {
                wasEnd = false;
                return true;
            }
            return false;
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
        Queue<PacketObject> urgentQueue;
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
