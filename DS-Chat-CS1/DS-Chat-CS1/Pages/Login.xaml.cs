using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS_Chat_CS1.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

        private void txtNickname_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                btnConnect_Click(this, null);
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string nickname = txtNickname.Text.Trim();
            string ipStr = txtCoordinatorIp.Text.Trim();
            string portStr = txtCoordinatorPort.Text.Trim();
            if (nickname.Length == 0)
            {
                MessageBox.Show("Must enter nickname");
                return;
            }
            if(nickname.Contains('#'))
            {
                MessageBox.Show("Not allowed characters: #");
                return;
            }
            if (ipStr.Length == 0)
            {
                MessageBox.Show("Must enter IP");
                return;
            }
            if (portStr.Length == 0)
            {
                MessageBox.Show("Must enter port");
                return;
            }

            btnConnect.IsEnabled = false;
            txtNickname.IsEnabled = false;

            progressRing.IsActive = true;
            lblState.Text = "Connecting...";
            indicatorsPanel.Visibility = Visibility.Visible;


            await Task.Run(() =>
            {
                MainContext.Instance.RegisterNickname(nickname, ipStr, Convert.ToInt32(portStr));
            });

            SwitchToLobby();



        }

        private void SwitchToLobby()
        {
            InitWindow window = (InitWindow)Window.GetWindow(this);
            window.SwitchToLobby();
        }
    }
}
