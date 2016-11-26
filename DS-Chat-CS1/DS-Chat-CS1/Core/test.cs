using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace ConsoleApplication2
{
    class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 2;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        //
        public List<byte> byteBuffer = new List<byte>();

        public int id = -1;
        public string packet;

    }

    class Program
    {
        IPEndPoint listenerEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
        Socket listener;
        public Program()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(listenerEndpoint);
            listener.Listen(100);

            WaitForClients();
        }

        private void WaitForClients()
        {
            Console.WriteLine("Waiting for clients...");
            listener.BeginAccept(new AsyncCallback((asyncResult) =>
            {
                try
                {

                    Socket clientSocket = listener.EndAccept(asyncResult);


                    if (clientSocket != null)
                    {
                        Console.WriteLine( "Received connection request from: " + clientSocket.RemoteEndPoint.ToString());
                        HandleClientRequest(clientSocket);
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw e;
                }

                //Continue waiting for more clients
                WaitForClients();
            
            }), null);
        }


        private void HandleClientRequest(Socket clientSocket)
        {
            //sockets.Add(clientSocket);
            WaitForReceive(clientSocket);
        }

        private void WaitForReceive(Socket clientSocket)
        {
            StateObject state = new StateObject();
            state.workSocket = clientSocket;
            clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnReceiveData), state);
        }

        private void OnReceiveData(IAsyncResult asyncResult)
        {
            
            StateObject state = (StateObject)asyncResult.AsyncState;

            Socket clientSocket = state.workSocket;

            int bytesRead = clientSocket.EndReceive(asyncResult);

            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

            string data = state.sb.ToString();

            Console.WriteLine("Receiving data, Length: " + bytesRead + ", Data: " + data);

            if (state.id == -1)
            {
                Console.WriteLine("New packet");
                if (data.StartsWith("BEGIN ") && data.Contains("#"))
                {
                    string id = "";
                    int i;
                    for (i = 6; i < data.Length; i++)
                    {
                        if (data[i] == '#')
                        {
                            break;
                        }
                        id += data[i];
                    }
                    state.id = Convert.ToInt32(id);
                    Console.WriteLine("Got begin with id " + id);
                    data = data.Substring(i+1);
                    state.sb.Clear();
                    state.sb.Append(data);

                }

                
                if (state.id != -1)
                {
                    Console.WriteLine("Appending leftovers to packet id " + state.id);
                    state.packet += data;
                }
                
                int indexOfEnd = data.IndexOf("END " + state.id + "#");

                if (indexOfEnd != -1)
                {
                    state.packet = data.Substring(0, data.Length - (data.Length - indexOfEnd));
                    state.sb.Clear();
                    state.sb.Append(data);
                }



                if (indexOfEnd != -1)
                {
                    Console.WriteLine("Got full packet: " + state.packet);
                    state.id = -1;
                    state.sb.Clear();
                    Array.Clear(state.buffer, 0, StateObject.BufferSize);
                }

            }
            else
            {
              Console.WriteLine("Appending to packet id " + state.id);
              state.packet += data;
              int indexOfEnd = data.IndexOf("END " + state.id + "#");

              if (indexOfEnd != -1)
              {
                  state.packet = data.Substring(0, data.Length -(data.Length - indexOfEnd));
                  state.sb.Clear();
                  state.sb.Append(data);
              }

              

              if (indexOfEnd != -1)
              {
                  Console.WriteLine("Got full packet: " + state.packet);
                  state.id = -1;
                  state.sb.Clear();
                  Array.Clear(state.buffer, 0, StateObject.BufferSize);
              }
            }

            Array.Clear(state.buffer, 0, StateObject.BufferSize);
           // state.sb.Clear();



            //Receive again
            clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnReceiveData), state);
 
        }

        static void Main(string[] args)
        {
            Console.WriteLine("s or c?");
            string choice = Console.ReadLine().Trim();
            if (choice.Equals("s"))
            {
                new Program();
                while (true)
                {
                    Console.Read();
                }
            }
            else if (choice.Equals("c"))
            {
                Client client = new Client();
                while (true)
                {
                    string input = Console.ReadLine().Trim();
                    if (input.Equals("q"))
                    {

                    }
                    else
                    {
                        client.SendMessage(input);
                    }
                }
            }
            
            
        }
    }
}


class Client
{
    Socket socket;
    public Client()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 12345);

        socket.Connect(endPoint);
        Console.WriteLine("Client: connected");
      
    }

    public void SendMessage(string message)
    {
        message = "BEGIN 123#" + message + "END 123#";
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(OnSendMessage), socket);
    }

    private void OnSendMessage(IAsyncResult asyncResult)
    {
        Socket socket = (Socket)asyncResult.AsyncState;

        socket.EndSend(asyncResult);
        Console.WriteLine("Done sending");

    }
}
