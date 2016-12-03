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
using TestSockets2;
using System.Runtime.InteropServices;

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
        }

        private void OnPacketReceived(object sender, PacketFullyReceivedEventArgs e)
        {
            Console.WriteLine("Received Packet in MainWindow");
            List<byte> data = e.Packet.data;
            string message = Encoding.ASCII.GetString(data.ToArray());
            if (toClients.Count != 0 && sender == toClients[0] ) //From Alice
            {
                Dispatcher.Invoke(() => { listClientMessages.Items.Add("Alice: " + message); });
                
            }
            else
            {
                Dispatcher.Invoke(() => { listServerMessages.Items.Add("Bob: " + message); });
            }
        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            string ipStr = serverIp.Text;
            int port = Convert.ToInt32(serverPort.Text);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            server = new TcpServer(endPoint);
            server.NewClient += onNewClient;
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
            client.SendData(Encoding.ASCII.GetBytes(message));
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
            client.SendData(Encoding.ASCII.GetBytes(message));
            Dispatcher.Invoke(() => { listServerMessages.Items.Add("Alice: " + message); });
        }
    }
}
