using DS_Chat_CS1.Core.LowLevel;
using DS_Chat_CS1.Core.Protocol;
using DS_Chat_CS1.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace DS_Chat_CS1
{
    public class MainContext
    {
        private static MainContext _instance;
        public static MainContext Instance {
            get {
                if (_instance == null)
                    _instance = new MainContext();
                return _instance;
            }
        }
        private IPEndPoint ownEndpoint;
        private TcpServer ownServer;
        private List<User> users;
        private Dictionary<Client, ChatWindow> chats;
        private Client coordinator;
        private IPEndPoint coordinatorEndpoint;

        public class UsersListReceivedEventArgs{ }
        public event EventHandler<UsersListReceivedEventArgs> UsersListReceived;

        private MainContext()
        {
            ownEndpoint = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 0);
            ownServer = new TcpServer(ownEndpoint);
            ownServer.NewClient += OwnServer_NewClient;
            users = new List<User>();
            chats = new Dictionary<Client, ChatWindow>();
            coordinator = new Client();
            coordinatorEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.3"), 1111);
            
        }



        private void OwnServer_NewClient(object sender, Client client)
        {
            client.PacketSent += Client_PacketSent;
            client.PacketReceived += Client_PacketReceived;
            foreach (User user in users)
            {
                if(client.GetRemoteEndPoint().Equals(user.Endpoint))
                {
                    user.Connection = client;
      
                    return;
                }
            }

            users.Add(new User() { Username = "Unknown", Connection = client, Endpoint = client.GetRemoteEndPoint() });


        }

        internal void OpenChatWindow(IPEndPoint endPoint)
        {
            var client = chats.Keys.FirstOrDefault(x => x.GetRemoteEndPoint().Equals(endPoint));
            if(client == null)
            {
                client = new Client();
                client.StartConnect(endPoint);
                client.PacketSent += Client_PacketSent;
                client.PacketReceived += Client_PacketReceived;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chats.Add(client, new ChatWindow());
                });
            }
            chats[client].ShowWindow();
        }



        class ImageFile
        {
            public string fileName;
            public FileStream stream;
            public ImageMessage imageMessage;
            public int segmentsCount;
        }
        
        Dictionary<Client, List<ImageFile>> filesMap = new Dictionary<Client, List<ImageFile>>();

        private void Client_PacketReceived(object sender, Core.Events.PacketFullyReceivedEventArgs e)
        {
            FyzrPacket packet = FyzrParser.FromData(e.Packet.data.ToArray());
            Client client = sender as Client;
            if(!chats.ContainsKey(client))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatWindow chatWindow = new ChatWindow();
                    chatWindow.ShowWindow();
                    chats.Add(client, chatWindow);
                });
                
            }
            if(packet.method == FyzrPacket.Method.TEXT)
            {
                Encoding enc = Encoding.GetEncoding(packet.headers["Content-Encoding"]);

                string message = enc.GetString(packet.body);
                TextMessage msg = new TextMessage()
                {
                    Side = MessageSide.You,
                    Text = message
                    
                };
                chats[client].AddTextMessage(msg);
            }
            else if(packet.method == FyzrPacket.Method.FILE)
            {
                string fileName = packet.headers["Filename"];

                int segmentLength = Convert.ToInt32(packet.headers["Content-Length"]);
                int segmentCount = Convert.ToInt32(packet.headers["Segment"]);
                int totalSegments = Convert.ToInt32(packet.headers["Total-Segments"]);

                long pos = Convert.ToInt32(packet.headers["Position"]);


                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments) + @"\TestReceived";
                System.IO.Directory.CreateDirectory(path);
                path += @"\" +fileName;
                if (!filesMap.ContainsKey(client))
                {
                    filesMap.Add(client, new List<ImageFile>());
                }

                ImageFile imageFile = filesMap[client].FirstOrDefault(x => x.fileName.Equals(fileName));
                if(imageFile == null)
                {
                    imageFile = new ImageFile() { fileName = fileName, segmentsCount=0, stream= new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None),
                    imageMessage = chats[client].CreateImageMessage(fileName)};
                    imageFile.stream.SetLength(Convert.ToInt32(packet.headers["Total-Length"]));
                    filesMap[client].Add(imageFile);
                }

                imageFile.segmentsCount++;

                imageFile.stream.Seek(pos, SeekOrigin.Begin);

                imageFile.stream.Write(packet.body, 0, packet.body.Length);

                if (imageFile.segmentsCount == totalSegments)
                {
                    imageFile.stream.Flush();
                    imageFile.stream.Close();
                    imageFile.stream = null;
                    imageFile.imageMessage.ImageUrl = path;
                    Console.WriteLine("Done file");
                    filesMap.Remove(client);

                }

                imageFile.imageMessage.Loading = (int)(imageFile.segmentsCount * 100 / totalSegments);


            }
            


        }

        private void Client_PacketSent(object sender, Core.Events.PacketSentEventArgs e)
        {
           
        }


        Thread requestThread;

        private void Coordinator_PacketReceived(object sender, Core.Events.PacketFullyReceivedEventArgs e)
        {
            FyzrPacket packet = FyzrParser.FromData(e.Packet.data.ToArray());
            if(packet.method == FyzrPacket.Method.COMMAND)
            {
                string cmdType = packet.headers["Command-Type"];
                if(cmdType.Equals("usersList"))
                {
                    Encoding enc = Encoding.GetEncoding(packet.headers["Content-Encoding"]);
                    if (packet.body != null)
                    {
                        string body = enc.GetString(packet.body);

                        users.Clear();

                        string[] lines = body.Split('\n');
                        foreach (string line in lines)
                        {
                            string[] splitted = line.Trim().Split('#');
                            string[] endPoint = splitted[1].Split(':');
                            users.Add(new User() { Username = splitted[0], Endpoint = new IPEndPoint(IPAddress.Parse(endPoint[0]), Convert.ToInt32(endPoint[1])) });
                        }

                        var handler = UsersListReceived;
                        if (handler != null) handler(this, new UsersListReceivedEventArgs());

                    }

                    if (requestThread == null)
                    {
                        requestThread = new Thread(RunRequestThread);
                        requestThread.Start();
                    }

                }
            }
        }

        private void RunRequestThread()
        {
            while(true)
            {
                Thread.Sleep(1000 * 10);
                RequestUsersList();
                Console.WriteLine("Sent users list request");
            }
        }
        
        public void RequestUsersList()
        {
            FyzrPacket packet = new FyzrPacket();
            packet.method = FyzrPacket.Method.COMMAND;
            packet.headers.Add("Command-Type", "requestUsersUpdate");
            packet.headers.Add("Listener-Port", "" + ownServer.listener.LocalEndpoint.ToString().Split(':')[1]);

            coordinator.SendOrdered(FyzrParser.ToData(packet));
        }


        public bool RegisterNickname(string nickname)
        {
            coordinator.StartConnect(coordinatorEndpoint);
            coordinator.PacketReceived += Coordinator_PacketReceived;
            FyzrPacket packet = new FyzrPacket();
            packet.method = FyzrPacket.Method.COMMAND;
            packet.headers.Add("Command-Type", "requestUsers");
            packet.headers.Add("Username", nickname);
            packet.headers.Add("Listener-Port", ""+ownServer.listener.LocalEndpoint.ToString().Split(':')[1]);

            coordinator.SendOrdered(FyzrParser.ToData(packet));


            return true;
        }





        public List<User> GetUsers()
        {
            return users;
        }

        public void ConnectToUser(User user)
        {
            if(user.Connection == null)
            {
                var client = user.Connection = new Client();
                client.StartConnect(user.Endpoint);
            }

        }

        internal void SendMessage(ChatWindow chatWindow, TextMessage msg)
        {
            var client = chats.FirstOrDefault(x => x.Value == chatWindow).Key;
            var packet = TextProtocol.CreateTextPacket(msg.Text, MessageProtocol.MessageType.PRIVATE, null);
            client.SendOrdered(FyzrParser.ToData(packet));
        }


        const int maxSegmentSize = 1024;
        internal void SendImageFile(ChatWindow chatWindow, string absolutePath, string fileName, ImageMessage msg)
        {
            var client = chats.FirstOrDefault(x => x.Value == chatWindow).Key;
            FileStream fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[maxSegmentSize];

            long length = fileStream.Length;
            long segmentCount = 1;
            long totalSegments = (long)Math.Ceiling((double)length / buffer.Length);

            long pos = 0;
            Console.WriteLine("File length: " + length);
            while (pos < length)
            {
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                byte[] data = new byte[bytesRead];
                Array.Copy(buffer, 0, data, 0, bytesRead);

                FyzrPacket packet = FileProtocol.CreateFilePacket(data, fileName, MessageProtocol.MessageType.PRIVATE, null);
                packet.headers.Add("Total-Length", "" + length);
                packet.headers.Add("Total-Segments", "" + totalSegments);
                packet.headers.Add("Segment", "" + segmentCount);
                packet.headers.Add("Position", "" + pos);

                client.SendRandom(FyzrParser.ToData(packet));
                msg.Loading = (int)(segmentCount * 100 / totalSegments );
                pos += buffer.Length;
                Console.WriteLine("Count: " + segmentCount++);
                Console.WriteLine("Bytes read: " + bytesRead);
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
