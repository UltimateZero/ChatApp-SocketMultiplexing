using DS_Chat_CS1.Pages;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DS_Chat_CS1
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : MetroWindow
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        Login login;
        Lobby lobby;
        public InitWindow()
        {
            AllocConsole();
            InitializeComponent();
            login = new Login();
            lobby = new Lobby();
            Title = "Login";
            transitioning.Content = login;

        }


        public void SwitchToLobby()
        {
            Title = "Lobby";
            transitioning.Content = lobby;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            MainContext.Instance.SendDisconnect();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}
