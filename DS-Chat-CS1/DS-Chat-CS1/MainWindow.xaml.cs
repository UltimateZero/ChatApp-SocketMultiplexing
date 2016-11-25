using DS_Chat_CS1.Core;
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

namespace DS_Chat_CS1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketManager socketMan;
        DebugWindow debugWindow;

        public MainWindow()
        {
            InitializeComponent();
            debugWindow = new DebugWindow();
            debugWindow.Show();
            socketMan = new SocketManager(debugWindow);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string ipStr = textBox.GetLineText(0);
            int port = Convert.ToInt32(textBox3.GetLineText(0));
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);

            socketMan.ConnectToClient(endPoint);
 
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string ipStr = textBox1.GetLineText(0);
            int port = Convert.ToInt32(textBox2.GetLineText(0));
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            socketMan.StartListening(endPoint);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if(textBox4.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter some text first");
                return;
            }
            debugWindow.addMessage("Send data clicked");
            string message = textBox4.GetLineText(0) + "#";

            socketMan.SendToClient(0, message);


        }
    }
}
