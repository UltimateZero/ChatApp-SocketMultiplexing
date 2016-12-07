using DS_Chat_CS1.Core.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.LowLevel
{
    partial class Client
    {
        private class Receiver
        {
            private static readonly int RECEIVE_BUFFER_SIZE = 256;
            internal event EventHandler<DataReceivedEventArgs> DataReceived;

            internal Receiver(NetworkStream stream)
            {
                _data = new byte[RECEIVE_BUFFER_SIZE];
                _stream = stream;
                _thread = new Thread(Run);
                _thread.Start();
            }

            private void Run()
            {
                // main thread loop for receiving data...
                try
                {
                    int bytesRead = 0;
                    // ShutdownEvent is a ManualResetEvent signaled by
                    // Client when its time to close the socket.
                    while (!ShutdownEvent.WaitOne(0))
                    {
                        try
                        {
                            if (!_stream.DataAvailable)
                            {
                                Thread.Sleep(1);
                            }
                            else if ((bytesRead = _stream.Read(_data, 0, _data.Length)) > 0)
                            {
                                //Console.WriteLine("Received some data");
    
                                var handler = DataReceived;
                                if (handler != null)
                                { 
                                    DataReceivedEventArgs args = new DataReceivedEventArgs() { Data = new byte[bytesRead] };
                                    
                                    Array.Copy(_data, args.Data, bytesRead);
                                    handler(this, args);
                                }

                            }
                            else
                            {
                                // The connection has closed gracefully, so stop the
                                // thread.
                                ShutdownEvent.Set();
                            }
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine(ex.ToString());
                            // Handle the exception...
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception...
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    _stream.Close();
                }

            }

            private NetworkStream _stream;
            private Thread _thread;
            private byte[] _data;
            
        }
    }
}
