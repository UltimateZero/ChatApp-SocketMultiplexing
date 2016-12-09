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
    public class MainContext : IDisposable
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
           // coordinatorEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.3"), 1111);
            
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
            public long segmentsCount;
        }

        class MediaFile
        {
            public string fileName;
            public FileStream stream;
            public MediaMessage mediaMessage;
            public long segmentsCount;
        }

        Dictionary<Client, List<ImageFile>> imageFilesMap = new Dictionary<Client, List<ImageFile>>();
        Dictionary<Client, List<MediaFile>> mediaFilesMap = new Dictionary<Client, List<MediaFile>>();
        private object lockk = new object();
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
                lock (lockk)
                {

                    string fileName = packet.headers["Filename"];

                    long segmentLength = Convert.ToInt64(packet.headers["Content-Length"]);
                    long segmentCount = Convert.ToInt64(packet.headers["Segment"]);
                    long totalSegments = Convert.ToInt64(packet.headers["Total-Segments"]);

                    long pos = Convert.ToInt64(packet.headers["Position"]);


                    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments) + @"\TestReceived";
                    System.IO.Directory.CreateDirectory(path);
                    path += @"\" + fileName;

                    string fileType = packet.headers["File-Type"];
                    if (fileType.Equals("Image"))
                    {
                        if (!imageFilesMap.ContainsKey(client))
                        {
                            imageFilesMap.Add(client, new List<ImageFile>());
                        }

                        ImageFile imageFile = imageFilesMap[client].FirstOrDefault(x => x.fileName.Equals(fileName));
                        if (imageFile == null)
                        {
                            imageFile = new ImageFile()
                            {
                                fileName = fileName,
                                segmentsCount = 0,
                                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None),
                                imageMessage = chats[client].CreateImageMessage(fileName)
                            };
                            imageFile.stream.SetLength(Convert.ToInt64(packet.headers["Total-Length"]));
                            imageFilesMap[client].Add(imageFile);
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
                            imageFilesMap[client].Remove(imageFile);

                        }

                        imageFile.imageMessage.Loading = ((double)imageFile.segmentsCount * 100.0 / totalSegments);
                    }

                    else if (fileType.Equals("Media"))
                    {
                        if (!mediaFilesMap.ContainsKey(client))
                        {
                            mediaFilesMap.Add(client, new List<MediaFile>());
                        }

                        MediaFile mediaFile = mediaFilesMap[client].FirstOrDefault(x => x.fileName.Equals(fileName));
                        if (mediaFile == null)
                        {
                            try
                            {
                                mediaFile = new MediaFile()
                                {
                                    fileName = fileName,
                                    segmentsCount = 0,
                                    stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None),
                                    mediaMessage = chats[client].CreateMediaMessage(fileName)
                                };
                                mediaFile.stream.SetLength(Convert.ToInt64(packet.headers["Total-Length"]));
                                mediaFilesMap[client].Add(mediaFile);
                            } catch(Exception ex)
                            {
                                Console.WriteLine("EXCEPTION: " + ex.ToString());
                                Console.WriteLine("media map size: " + mediaFilesMap[client].Count);
                            }
                        }

                        mediaFile.segmentsCount++;

                        mediaFile.stream.Seek(pos, SeekOrigin.Begin);

                        mediaFile.stream.Write(packet.body, 0, packet.body.Length);

                        if (mediaFile.segmentsCount == totalSegments)
                        {
                            mediaFile.stream.Flush();
                            mediaFile.stream.Close();
                            mediaFile.stream = null;
                            mediaFile.mediaMessage.MediaUrl = path;
                            Console.WriteLine("Done file");
                            mediaFilesMap[client].Remove(mediaFile);

                        }

                        mediaFile.mediaMessage.Loading = ((double)mediaFile.segmentsCount * 100.0 / totalSegments);
                    }

                }
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


        public bool RegisterNickname(string nickname, string coordinatorIp, int coordinatorPort)
        {
            coordinatorEndpoint = new IPEndPoint(IPAddress.Parse(coordinatorIp), coordinatorPort);
            coordinator.StartConnect(coordinatorEndpoint);
            Thread.Sleep(10);
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


        const int maxSegmentSize = 1024*20;
        internal void SendFile(string absolutePath, string fileName)
        {

        }
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
                packet.headers.Add("File-Type", "Image");

                client.SendRandom(FyzrParser.ToData(packet));
                msg.Loading = ((double)segmentCount * 100.0 / totalSegments);
                pos += buffer.Length;
                Console.WriteLine("Count: " + segmentCount++);
                Console.WriteLine("Bytes read: " + bytesRead);
                Thread.Sleep(2);
            }
        }
        internal void SendMediaFile(ChatWindow chatWindow, string absolutePath, string fileName, MediaMessage msg)
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
                packet.headers.Add("File-Type", "Media");

                client.SendRandom(FyzrParser.ToData(packet));
                msg.Loading = ((double)segmentCount * 100.0 / totalSegments);
                pos += buffer.Length;
                Console.WriteLine("Count: " + segmentCount++);
                Console.WriteLine("Bytes read: " + bytesRead);
                Thread.Sleep(2);
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

        public void Dispose()
        {
            if(coordinator.IsConnected())
            {
                coordinator.Disconnect();
            }
        }
    }
}
