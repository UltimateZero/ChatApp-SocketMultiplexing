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
            if (nickname.Length == 0)
            {
                MessageBox.Show("Must enter nickname");
                return;
            }

            btnConnect.IsEnabled = false;
            txtNickname.IsEnabled = false;

            progressRing.IsActive = true;
            lblState.Text = "Connecting...";
            indicatorsPanel.Visibility = Visibility.Visible;


            await Task.Run(() =>
            {
                Thread.Sleep(1000 * 2);
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
