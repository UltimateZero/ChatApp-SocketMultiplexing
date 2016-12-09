using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
using DS_Chat_CS1.Core.LowLevel;
using DS_Chat_CS1.Core.Events;
using DS_Chat_CS1.Core.Protocol;

namespace DS_Chat_CS1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        TcpServer server;
        List<Client> toClients;
        List<Client> fromClients;

        public MainWindow()
        {
            toClients = new List<Client>();
            fromClients = new List<Client>();
            InitializeComponent();

            AllocConsole();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Client.ShutdownEvent.Set();
            Environment.Exit(0);
        }




        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string ipStr = clientIp.Text;
            int port = Convert.ToInt32(clientPort.Text);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            Client client = new Client();
            client.PacketReceived += OnPacketReceived;
            toClients.Add(client);
            client.StartConnect(endPoint);
            btnConnect.IsEnabled = false;
        }

        Dictionary<string, FileStream> filesMap = new Dictionary<string, FileStream>();
        Dictionary<string, long> filesSegmentsMap = new Dictionary<string, long>();
        
        private void OnPacketReceived(object sender, PacketFullyReceivedEventArgs e)
        {
            Console.WriteLine("Received Packet in MainWindow");
            List<byte> data = e.Packet.data;
            FyzrPacket packet = FyzrParser.FromData(data.ToArray());


            switch (packet.method)
            {
                case FyzrPacket.Method.TEXT:
                    Encoding enc = Encoding.GetEncoding(packet.headers["Content-Encoding"]);
                    

                    string message = enc.GetString(packet.body);
                    if (toClients.Count != 0 && sender == toClients[0]) //From Alice
                    {
                        Dispatcher.Invoke(() => { listClientMessages.Items.Add("Alice: " + message); });

                    }
                    else
                    {
                        Dispatcher.Invoke(() => { listServerMessages.Items.Add("Bob: " + message); });
                    }
                    break;
                case FyzrPacket.Method.FILE:
                   
                    lock (filesMap)
                    {
                        string fileName = packet.headers["Filename"];
                        string path = @"C:\Users\User\Documents\TestFolder\" + fileName;
                        FileStream fs;
                        if (!filesMap.ContainsKey(fileName))
                        {
                            filesMap.Add(fileName, new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None));
                            filesMap[fileName].SetLength(Convert.ToInt32(packet.headers["Total-Length"]));

                            filesSegmentsMap.Add(fileName, 0);
                        }

                        fs = filesMap[fileName];
                        filesSegmentsMap[fileName]++;

                        int segmentLength = Convert.ToInt32(packet.headers["Content-Length"]);
                        int segmentCount = Convert.ToInt32(packet.headers["Segment"]);
                        int totalSegments = Convert.ToInt32(packet.headers["Total-Segments"]);

                        long pos = Convert.ToInt32(packet.headers["Position"]);

                        fs.Seek(pos, SeekOrigin.Begin);

                        fs.Write(packet.body, 0, packet.body.Length);

                        if (filesSegmentsMap[fileName] == totalSegments)
                        {
                            fs.Flush();
                            fs.Close();
                            fs = null;
                            filesMap.Remove(fileName);
                            filesSegmentsMap.Remove(fileName);
                            Console.WriteLine("Done file");
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        int count = Convert.ToInt32(lblSecondSegments.Content);
                        count++;
                        lblSecondSegments.Content = count;
                    });


                    //Dispatcher.Invoke(() =>
                    //{
                    //    int count = Convert.ToInt32(lblServerSegments.Content);
                    //    count++;
                    //    lblServerSegments.Content = count;
                    //});

                    break;
            }

        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            string ipStr = serverIp.Text;
            int port = Convert.ToInt32(serverPort.Text);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            server = new TcpServer(endPoint);
            server.NewClient += onNewClient;

            btnListen.IsEnabled = false;
        }

        private void onNewClient(object sender, Client client)
        {
            client.PacketReceived += OnPacketReceived;
            fromClients.Add(client);
            Console.WriteLine("MainWindow: Client from listener added to list");
        }




        private void btnClientSend_Click(object sender, RoutedEventArgs e)
        {
            if (txtClientMessage.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter some text first");
                return;
            }
            string message = txtClientMessage.Text;


            Client client = toClients[0];
            client.SendOrdered(
              FyzrParser.ToData(
                  TextProtocol.CreateTextPacket(message, MessageProtocol.MessageType.PRIVATE, null)
                   ));
            Dispatcher.Invoke(() => { listClientMessages.Items.Add("Bob: " + message); });
        }

        private void btnServerSend_Click(object sender, RoutedEventArgs e)
        {
            if (txtServerMessage.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter some text first");
                return;
            }
            string message = txtServerMessage.Text;


            Client client = fromClients[0];
            client.SendOrdered(
                FyzrParser.ToData(
                    TextProtocol.CreateTextPacket(message, MessageProtocol.MessageType.PRIVATE, null)
                    ));
            Dispatcher.Invoke(() => { listServerMessages.Items.Add("Alice: " + message); });
        }

        int maxSegmentSize = 1024;
        private void SendFile(string absolutePath, string fileName, Client client)
        {
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
                pos += buffer.Length;
                Console.WriteLine("Count: " + segmentCount++);
                Console.WriteLine("Bytes read: " + bytesRead);
            }
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Client client = toClients[0];
                SendFile(@"C:\Users\User\Documents\TestFolder\pic.jpg", "copy.jpg", client);
            });




        }

        private void btnSendFile_Copy_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Client client = toClients[0];
                SendFile(@"C:\Users\User\Documents\TestFolder\text.txt", "copy.txt", client);
            });
        }
    }
}
