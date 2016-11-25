using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace DS_Chat_CS1.Core
{
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 2;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    class SocketManager
    {
        //Listener Ip
        private IPEndPoint listenerEndpoint;

        private List<Socket> sockets;
        private Socket listener;
        private DebugWindow debugWindow;


        public SocketManager(DebugWindow debugWindow)
        {
            this.debugWindow = debugWindow;
            sockets = new List<Socket>();


        }

        public void StartListening(IPEndPoint endPoint)
        {
            this.listenerEndpoint = endPoint;

            InitListener();

            debugWindow.addMessage("Listener initiated");

            WaitForClients();

        }

        private void InitListener()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(listenerEndpoint);
            listener.Listen(100);
        }

        private void WaitForClients()
        {
           
            listener.BeginAccept(new AsyncCallback(OnClientConnected), null);
        }

        private void OnClientConnected(IAsyncResult asyncResult)
        {
            try
            {
                
                Socket clientSocket = listener.EndAccept(asyncResult);
                

                if (clientSocket != null) {
                    debugWindow.addMessage("Received connection request from: " + clientSocket.RemoteEndPoint.ToString());
                    HandleClientRequest(clientSocket);
                }
            }

            catch(Exception e)
            {
                debugWindow.addMessage(e.ToString());
                throw e;
            }

            //Continue waiting for more clients
            WaitForClients();
        }


        private void HandleClientRequest(Socket clientSocket)
        {
            sockets.Add(clientSocket);
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
            try
            {
                StateObject state = (StateObject)asyncResult.AsyncState;

                Socket clientSocket = state.workSocket;
                
                int bytesRead = clientSocket.EndReceive(asyncResult);
                if (bytesRead > 0)
                {
       

                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    string data = state.sb.ToString();
                    debugWindow.addMessage("Received size: " + bytesRead + "\nData: " + data);
                  
                    if(data.EndsWith("#"))
                    {

                        debugWindow.addMessage("RECEIVED full message: " + data);
                        HandleFullReceivedMessage(data);
                        //Clear buffers
                        Array.Clear(state.buffer, 0, StateObject.BufferSize);
                        state.sb.Clear();
                    }

                }
                else
                {
                    debugWindow.addMessage("less than 1");
                }

                clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnReceiveData), state);

            } catch(Exception e)
            {
                debugWindow.addMessage(e.ToString());
            }


        }


        private void HandleFullReceivedMessage(string fullMessage)
        {
            debugWindow.addMessage("User sent " + fullMessage);
        }

        public void ConnectToClient(IPEndPoint endPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sockets.Add(socket);
            socket.Connect(endPoint);
            debugWindow.addMessage("Connected to " + endPoint.ToString());
        
      

        }

        public void SendToClient(int index, string message)
        {
            Socket socket = sockets[sockets.Count-1];
            if(socket != null)
            {
                try
                {
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(OnSendMessage), socket);
                }
                 catch(Exception e)
                {
                    debugWindow.addMessage("Client: " + e.ToString());
                }
            }
            
        }

        private void OnSendMessage(IAsyncResult asyncResult)
        {
            Socket socket = (Socket)asyncResult.AsyncState;
            socket.EndSend(asyncResult);
            debugWindow.addMessage("Done sending");

        }

    }
}
