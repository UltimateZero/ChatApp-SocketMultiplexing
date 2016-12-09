using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.LowLevel
{
    public partial class Client
    {
        private sealed class Sender
        {
            internal void SendData(byte[] data)
            {
                // transition the data to the thread and send it...
                lock(sendQueue)
                {
                    sendQueue.Enqueue(data);
                }
            }

            internal Sender(NetworkStream stream)
            {
                sendQueue = new Queue<byte[]>();
                _stream = stream;
                _thread = new Thread(Run);
                _thread.Start();
            }

            private void Run()
            {
                // main thread loop for sending data...
                while(true)
                {
                    lock(sendQueue)
                    {
                        if (sendQueue.Count != 0)
                        {
                            byte[] data = sendQueue.Dequeue();
                            _stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }

                }
            }

            private NetworkStream _stream;
            private Thread _thread;
            private Queue<byte[]> sendQueue;
        }
    }
}
