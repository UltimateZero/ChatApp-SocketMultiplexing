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


    /**
     * segment size: 8 bytes (atomic)
     * Queue:
     * Append to queue, gives id, prepend "BEGIN", append "END"
     * 
     * BEGIN, PAUSE, RESUME, END
     * 
     * 
     * 
     * 
     * BEGIN 123
     * asdfghjh
     * PAUSE 123
     * BEGIN 345
     * sadsa
     * sgffgf
     * dsadsa
     * sdasdsa
     * END 345
     * RESUME 123
     * dsadsa
     * asdsad
     * fgdgfd
     * sad
     * END 123
     */

    class Packet
    {
        long id;
    }



    class SocketListener
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
        }

        string TAG = "Listener";
        //Listener Ip
        private IPEndPoint listenerEndpoint;
        private Socket listener;
        private DebugWindow debugWindow;

        private Packet currentPacket;

        public SocketListener(DebugWindow debugWindow)
        {
            this.debugWindow = debugWindow;
        }

        #region Accepting
        public void StartListening(IPEndPoint endPoint)
        {
            this.listenerEndpoint = endPoint;

            InitListener();

            debugWindow.addMessage(TAG, "Listener initiated: " + endPoint.ToString());

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
            debugWindow.addMessage(TAG, "Waiting for clients...");
            listener.BeginAccept(new AsyncCallback(OnClientConnected), null);
        }

        private void OnClientConnected(IAsyncResult asyncResult)
        {
            try
            {

                Socket clientSocket = listener.EndAccept(asyncResult);


                if (clientSocket != null)
                {
                    debugWindow.addMessage(TAG, "Received connection request from: " + clientSocket.RemoteEndPoint.ToString());
                    HandleClientRequest(clientSocket);
                }
            }

            catch (Exception e)
            {
                debugWindow.addMessage(TAG, e.ToString());
                throw e;
            }

            //Continue waiting for more clients
            WaitForClients();
        }


        private void HandleClientRequest(Socket clientSocket)
        {
            //sockets.Add(clientSocket);
            WaitForReceive(clientSocket);
        }
        #endregion

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
                StateObject state = (StateObject) asyncResult.AsyncState;

                Socket clientSocket = state.workSocket;

                int bytesRead = clientSocket.EndReceive(asyncResult);
                if (bytesRead > 0)
                {

                    for(int i = 0; i < bytesRead; i++)
                    {
                        state.byteBuffer.Add(state.buffer[i]);
                    }

                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                   // string data = state.sb.ToString();
                    debugWindow.addMessage(TAG, "Received size: " + bytesRead + "\nData: " + data);

                    if (data.EndsWith("#"))
                    {

                        debugWindow.addMessage(TAG, "RECEIVED full message: " + data);
                        HandleFullReceivedMessage(data);
                        //Clear buffers
                        Array.Clear(state.buffer, 0, StateObject.BufferSize);
                        state.sb.Clear();
                    }

                }
                else
                {
                    debugWindow.addMessage(TAG, "less than 1");
                }

                clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnReceiveData), state);

            }
            catch (Exception e)
            {
                debugWindow.addMessage(TAG, e.ToString());
            }


        }


        private void HandleFullReceivedMessage(string fullMessage)
        {
            debugWindow.addMessage(TAG, "User sent " + fullMessage);
        }
    }

    class SocketManager
    {

        private List<Socket> sockets;
        private DebugWindow debugWindow;


        public SocketManager(DebugWindow debugWindow)
        {
            this.debugWindow = debugWindow;
            sockets = new List<Socket>();


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
