using DS_Chat_CS1.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.LowLevel
{
    class Packet
    {
        public int id;
        public List<byte> data = new List<byte>();
    }

    class PacketReceiver
    {
        internal event EventHandler<PacketFullyReceivedEventArgs> PacketReceived;

        internal void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            byte[] buffer = e.Data;

            foreach(byte b in buffer)
            {
                switch(packetState)
                {
                    case PacketState.NORMAL:

                        if (b == SWITCH) //SWITCH
                        {
                            //Console.WriteLine("Found SWITCH");
                            receiveState = ReceiveState.SWITCH;
                            packetState = PacketState.READING_ID;
                        }
                        else if (b == END)
                        {
                            //Console.WriteLine("Found END");
                            receiveState = ReceiveState.END;
                            packetState = PacketState.READING_ID;

                        }
                        else if (b == ESC)
                        {
                            packetState = PacketState.IN_ESC;
                            //Console.WriteLine("Found ESC");
                        }

                        else
                        {
                            // Console.WriteLine("Appending byte");
                            AppendToPacket(b);
                        }

                        break;

                    case PacketState.IN_ESC:
                        //Console.WriteLine("Appending escaped byte");
                        packetState = PacketState.NORMAL;
                        AppendToPacket(b);
                        

                        break;

                    case PacketState.READING_ID:
                        idBuffer.Add(b);
                        //Console.WriteLine("Appending ID byte");
                        if (idBuffer.Count == 4)
                        {
                           byte[] buf = idBuffer.ToArray();
                           idBuffer.Clear();
                           packetState = PacketState.NORMAL;

                            int id = System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buf, 0));
                            //Console.WriteLine("Found ID: " + id);
                            ChangeCurrentId(id);

                        }
                        break;
                }
            }

        }

        private void ChangeCurrentId(int id)
        {
            prevId = currentId;
            currentId = id;
            if (currentId == -1)
            {
                Console.WriteLine("Something wrong happened, -1 id");
                return;
            }
            switch (receiveState)
            {
                case ReceiveState.SWITCH:
                    if (packetsMap.ContainsKey(currentId))
                    {
                        //Console.WriteLine("Resuming packet " + currentId);
                    }
                    else
                    {
                        //if (prevId != currentId && prevId != -1)
                            //Console.WriteLine("Pausing packet " + prevId);
                        //Console.WriteLine("Beginning new packet " + currentId);
                        Packet packet = new Packet();
                        packet.id = currentId;
                        packetsMap.Add(currentId, packet);
                    }

                    break;
                case ReceiveState.END:
                    //Console.WriteLine("Finishing packet " + currentId);
                    HandleFinishedPacket(packetsMap[currentId]);
                    prevId = currentId;
                    currentId = -1;
                    break;
            }


        }



        private void AppendToPacket(byte b)
        {
            if (currentId == -1)
            {
                Console.WriteLine("Something wrong happened, -1 id");
                return;
            }
            // Console.WriteLine("Appending data to packet id " + currentId);
            packetsMap[currentId].data.Add(b);
        }


        private void HandleFinishedPacket(Packet packet)
        {
            packetsMap.Remove(packet.id); //Remove from map

            //re-raise full packet
            var handler = PacketReceived;
            if (handler != null)
            {
                PacketFullyReceivedEventArgs args = new PacketFullyReceivedEventArgs() { Packet = packet };
                handler(this, args);
            }
        }


        public enum PacketState
        {
            NORMAL, IN_ESC, READING_ID
        }
        public enum ReceiveState
        {
            SWITCH, END
        }
        public static readonly byte SWITCH = 7;
        public static readonly byte END = 8;
        public static readonly byte ESC = 9;

        Dictionary<int, Packet> packetsMap = new Dictionary<int, Packet>();
        private int currentId = -1;
        private int prevId = -1;
        private List<byte> idBuffer = new List<byte>();
        private PacketState packetState = PacketState.NORMAL;
        private ReceiveState receiveState;

    }
}
