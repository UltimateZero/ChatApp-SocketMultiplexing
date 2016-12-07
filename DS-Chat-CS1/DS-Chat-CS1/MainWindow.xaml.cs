﻿using System;
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
using TestSockets2;
using System.Runtime.InteropServices;
using System.IO;

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

            //AllocConsole();
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



        int currentPacketType = 0;

        private void OnPacketReceived(object sender, PacketFullyReceivedEventArgs e)
        {
            Console.WriteLine("Received Packet in MainWindow");
            List<byte> data = e.Packet.data;
            int newLine = data.IndexOf((byte)'\n');
            if(newLine != -1)
            {
                string type = Encoding.ASCII.GetString(data.ToArray(), 0, newLine);
                if(type.Equals("TEXT"))
                {
                    currentPacketType = 1;
                }
                else if(type.Equals("FILE"))
                {
                    currentPacketType = 2;
                }
                else if (type.Equals("FILE1"))
                {
                    currentPacketType = 3;
                }

            }
            if(currentPacketType == 1)
            {
                string message = Encoding.ASCII.GetString(data.ToArray());
                if (toClients.Count != 0 && sender == toClients[0]) //From Alice
                {
                    Dispatcher.Invoke(() => { listClientMessages.Items.Add("Alice: " + message); });

                }
                else
                {
                    Dispatcher.Invoke(() => { listServerMessages.Items.Add("Bob: " + message); });
                }
            }
            else if (currentPacketType == 2)
            {
                Dispatcher.Invoke(() =>
                {
                    int count = Convert.ToInt32(lblServerSegments.Content);
                    count++;
                    lblServerSegments.Content = count;
                });
            }
            else if (currentPacketType == 3)
            {
                Dispatcher.Invoke(() =>
                {
                    int count = Convert.ToInt32(lblSecondSegments.Content);
                    count++;
                    lblSecondSegments.Content = count;
                });
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
            string message = "TEXT\n"+txtClientMessage.Text;


            Client client = toClients[0];
            client.SendData(message);
            Dispatcher.Invoke(() => { listClientMessages.Items.Add("Bob: " + message); });
        }

        private void btnServerSend_Click(object sender, RoutedEventArgs e)
        {
            if (txtServerMessage.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter some text first");
                return;
            }
            string message = "TEXT\n" + txtServerMessage.Text;


            Client client = fromClients[0];
            client.SendData(message);
            Dispatcher.Invoke(() => { listServerMessages.Items.Add("Alice: " + message); });
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                Client client = toClients[0];
                FileStream fileStream = new FileStream(@"D:\College\AAST\Sixth semester\schedule.PNG", FileMode.Open, FileAccess.Read);
                int count = 0;
                byte[] buffer = new byte[512];
                List<byte> bytes = new List<byte>();
                long length = fileStream.Length;
                long pos = fileStream.Position;
                Console.WriteLine("File length: " + length);
                while(pos < length)
                {
                    fileStream.Read(buffer, 0, buffer.Length);
                    bytes.AddRange(Encoding.ASCII.GetBytes("FILE\n"));
                    bytes.AddRange(buffer);
                    client.SendData(bytes.ToArray(), 3);
                    pos += buffer.Length;
                    Console.WriteLine("Count: " + count++);
                    bytes.Clear();
                }
            });
            



        }

        private void btnSendFile_Copy_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                Client client = toClients[0];
                FileStream fileStream = new FileStream(@"D:\College\AAST\Sixth semester\schedule.PNG", FileMode.Open, FileAccess.Read);
                int count = 0;
                byte[] buffer = new byte[512];
                List<byte> bytes = new List<byte>();
                long length = fileStream.Length;
                long pos = fileStream.Position;
                Console.WriteLine("File length: " + length);
                while (pos < length)
                {
                    fileStream.Read(buffer, 0, buffer.Length);
                    bytes.AddRange(Encoding.ASCII.GetBytes("FILE1\n"));
                    bytes.AddRange(buffer);
                    client.SendData(bytes.ToArray(), 3);
                    pos += buffer.Length;
                    Console.WriteLine("Count: " + count++);
                    bytes.Clear();
                }
            });
        }
    }
}
