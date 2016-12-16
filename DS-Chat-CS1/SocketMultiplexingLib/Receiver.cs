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
    public static class Constants
    {
        public readonly static byte SWITCH = 7;
        public readonly static byte END = 8;
        public readonly static byte ESC = 9;
    }


    public class Receiver
    {

        enum ReceiveState
        {
            SWITCH, END, UNKNOWN
        }

        enum PacketState
        {
            NORMAL, READING_ID, IN_ESC
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        
        private readonly int RECEIVE_BUFFER = 256;
        private byte[] buffer;

        private Socket _socket;
        private Thread _thread;

        private bool in_packet = false;
        private int prevId;
        private int currentId;
        private ReceiveState currentReceiveState = ReceiveState.UNKNOWN;
        private PacketState currentPacketState = PacketState.NORMAL;

        private byte[] idBuffer;
        private int idBufferCounter = 0;

        public Receiver(Socket socket)
        {
            buffer = new byte[RECEIVE_BUFFER];
            idBuffer = new byte[4];

            _socket = socket;
            _thread = new Thread(Run);
            

        }


        public void Start()
        {
            _thread.Start();
        }

        private void Run()
        {
            int bytesRead;
            while ((bytesRead = _socket.Receive(buffer)) > 0) {
                HandleReceivedData(buffer, 0, bytesRead);
            }

        }

        public void HandleReceivedData(byte[] buffer, int offset, int bytesRead)
        {
            List<byte> receivedData = new List<byte>();
            byte b;
            for(int i = offset; i < bytesRead; i++)
            {
                b = buffer[i];
                switch(currentPacketState)
                {
                    case PacketState.NORMAL:
                        if (b == Constants.SWITCH)
                        {
                            currentReceiveState = ReceiveState.SWITCH;
                            currentPacketState = PacketState.READING_ID;
                        }
                        else if (b == Constants.END)
                        {
                            currentReceiveState = ReceiveState.END;
                            currentPacketState = PacketState.READING_ID;
                        }
                        else if (b == Constants.ESC)
                        {
                            currentPacketState = PacketState.IN_ESC;
                        }
                        else
                        {
                            if (!in_packet)
                            {
                                throw new Exception("Received data before receiving packet Id");
                            }
                            receivedData.Add(b);
                        }
                        break;
                    case PacketState.IN_ESC:
                        receivedData.Add(b);
                        currentPacketState = PacketState.NORMAL;
                        break;
                    case PacketState.READING_ID:
                        idBuffer[idBufferCounter++] = b;
                        if(idBufferCounter >= 4)
                        {
                            idBufferCounter = 0;
                            currentId = System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt32(idBuffer, 0));
                            currentPacketState = PacketState.NORMAL;

                            if(currentReceiveState == ReceiveState.SWITCH)
                            {
                                //New packet
                                in_packet = true;
                            }
                            else if(currentReceiveState == ReceiveState.END)
                            {
                                //End packet
                                if (DataReceived != null)
                                    DataReceived(this, new DataReceivedEventArgs()
                                    {
                                        Id = currentId,
                                        Data = receivedData.Count > 0 ? receivedData.ToArray() : null,
                                        isEnd = true
                                    });
                                prevId = currentId;
                                currentId = 0;
                                in_packet = false;
                                receivedData.Clear();
                            }
                        }
                        break;
                }

            }
            

            if (in_packet && receivedData.Count != 0 && DataReceived != null)
                DataReceived(this, new DataReceivedEventArgs() {
                    Id = currentId,
                    Data = receivedData.ToArray(),
                    isEnd = false
                });
      

        }
    }
}
