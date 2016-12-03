using Priority_Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSockets2
{
    class PacketSender
    {
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

            // Console.WriteLine(packet.ToString());
            if (!packetsMap.TryAdd(packet.id, packet))
            {
                Console.WriteLine("COULDNT ADD PACKET ID " + packet.id);
                throw new Exception();
            }
            Console.WriteLine("Preparing packet with id " + packet.id);
            Segment seg = new Segment();
            int size = 0;
            seg.packet = packet;
            for (int i = 0; i < data.Length; i++)
            {

                if (size >= MAX_LENGTH)
                {
                    packet.segments.Enqueue(seg);
                    seg = new Segment();
                    seg.packet = packet;
                    size = 0;
                }
                size++;
                seg.data.Add(data[i]);

            }
            packet.segments.Enqueue(seg);
            packet.numOfSegments = packet.segments.Count;
            lock (lockObj)
                EnqueuePacket(packet);
        }

        private void EnqueuePacket(PacketObject packet)
        {
            lock (lockObj)
            {
                priorityQueue.Enqueue(packet, packet.priority);
            }

            //while (packet.segments.Count != 0)
            //{
            //    lock(priorityQueue)
            //    {
            //        priorityQueue.Enqueue(packet.segments.Dequeue(), packet.priority);
            //    }

            //}
            Console.WriteLine("Enqueued " + packet.numOfSegments + " segments for packet " + packet.id);
        }

        object lockObj = new object();
        List<byte> dataBuffer = new List<byte>();
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
                        if (head.cancelled) continue;

                        if (currentId != head.id)
                        {
                            currentId = head.id;
                            dataBuffer.Add(SWITCH); //Switch
                            AddId(dataBuffer, currentId);
                        }


                        int toRead = head.offset + MAX_LENGTH;
                        if (toRead > head.data.Length)
                        {
                            toRead = toRead - head.data.Length;
                        }
                        for (int i = head.offset; i < toRead; i++)
                        {
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
                        }





                        if (head.segments.Count != 0)
                        {
                            Segment seg = head.segments.Dequeue();

                            if (!seg.packet.cancelled)
                            {
                                //Blocking send
                                SendSegment(seg);

                            }
                            else
                            {
                                priorityQueue.Remove(head);
                            }

                        }
                        else
                        {
                            if (priorityQueue.Contains(head) && head != null)
                            {
                                Console.WriteLine("Queue: " + priorityQueue.Count);
                                priorityQueue.Remove(head);
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

        private void SendSegment(Segment seg)
        {
            List<byte> data = new List<byte>();
            if (currentId != seg.packet.id)
            {
                currentId = seg.packet.id;
                data.Add(SWITCH);
                AddId(data, currentId);
            }
            EscapeAndAddData(data, seg.data.ToArray());

            var handler = DataReady;
            if (handler != null)
            {
                handler(this, new DataReadyEventArgs() { Data = data.ToArray() });
                Console.WriteLine("Sent segment for packet " + seg.packet.id);
                seg.packet.sentSegments++;



                if (seg.packet.sentSegments == seg.packet.numOfSegments) //END
                {
                    data = new List<byte>();
                    data.Add(END);
                    AddId(data, currentId);
                    handler(this, new DataReadyEventArgs() { Data = data.ToArray() });
                    Console.WriteLine("END packet " + seg.packet.id);
                    PacketObject ignored;
                    packetsMap.TryRemove(seg.packet.id, out ignored);
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

        private void EscapeAndAddData(List<byte> buf, byte[] data)
        {
            foreach (byte b in data)
            {
                if (b >= SWITCH && b <= ESC)
                {
                    buf.Add(ESC);
                }
                buf.Add(b);
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

        public static readonly int MAX_LENGTH = 256; //Atomic packet in bytes


        public static readonly byte SWITCH = PacketReceiver.SWITCH;
        public static readonly byte END = PacketReceiver.END;
        public static readonly byte ESC = PacketReceiver.ESC;

        class PacketObject : FastPriorityQueueNode
        {
            public int id;
            public int priority = 0;
            public int offset = 0;
            public byte[] data;
            public int sentSegments = 0;
            public int numOfSegments = 0;
            public bool cancelled = false;
            public Queue<Segment> segments = new Queue<Segment>();
            public override string ToString()
            {
                return "Packet: id=" + id;
            }
        }
        class Segment : IComparable
        {
            public PacketObject packet;
            public List<byte> data = new List<byte>();

            public int CompareTo(object obj)
            {
                Segment seg = (Segment)obj;
                return this.packet.priority - seg.packet.priority;
            }
        }
    }
}
