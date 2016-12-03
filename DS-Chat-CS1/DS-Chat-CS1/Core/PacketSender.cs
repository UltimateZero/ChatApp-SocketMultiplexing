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
            priorityQueue = new SimplePriorityQueue<Segment>();
            _thread = new Thread(Run);
            _thread.Start();
        }

        public void SendData(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            SendData(bytes);
        }
        public void SendData(byte[] data)
        {
            PacketObject packet = new PacketObject();
            packet.id = Interlocked.Increment(ref idCounter);
            

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

            EnqueuePacket(packet, 0);
        }

        private void EnqueuePacket(PacketObject packet, int priority)
        {
            packet.priority = priority;
            while (packet.segments.Count != 0)
            {
                priorityQueue.Enqueue(packet.segments.Dequeue(), packet.priority); //priority
            }
            Console.WriteLine("Enqueued " + packet.numOfSegments + " segments for packet " + packet.id);
        }


        private void Run()
        {
            while (true)
            {

                if (priorityQueue.Count != 0)
                {

                    Console.WriteLine("Starting to send...");
                    Segment seg = priorityQueue.Dequeue();
                   // if (!seg.packet.cancelled) //Ignore if cancelled
                  //  {
                        //Blocking send
                        SendSegment(seg);
                   // }
                }
                else 
                {
                    Thread.Sleep(1); //Avoid constant CPU usage
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
            if(handler != null)
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
        SimplePriorityQueue<Segment> priorityQueue;
        ConcurrentDictionary<int, PacketObject> packetsMap;

        public static readonly int MAX_LENGTH = 8; //Atomic packet in bytes


        public static readonly byte SWITCH = PacketReceiver.SWITCH;
        public static readonly byte END = PacketReceiver.END;
        public static readonly byte ESC = PacketReceiver.ESC;

        class PacketObject
        {
            public int id;
            public int priority = 0;
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
