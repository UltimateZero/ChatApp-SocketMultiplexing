using DS_Chat_CS1.Model;
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

namespace DS_Chat_CS1.Pages
{
    /// <summary>
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : UserControl
    {
        public Lobby()
        {
            InitializeComponent();
            MainContext.Instance.UsersListReceived += MainContext_UsersListReceived;
        }

        private void MainContext_UsersListReceived(object sender, MainContext.UsersListReceivedEventArgs e)
        {
            Dispatcher.Invoke(() => {

                List<User> users = MainContext.Instance.GetUsers();
                listUsers.Items.Clear();
                foreach (var user in users)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.MouseDoubleClick += Item_MouseDoubleClick;
                    item.Content = user.Username;
                    item.ToolTip = user.Endpoint.ToString();
                    listUsers.Items.Add(item);
                }
            });

        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string endPointStr = (sender as ListBoxItem).ToolTip as string;

            MainContext.Instance.OpenChatWindow(endPointStr, (sender as ListBoxItem).Content.ToString());

        }
    }
}
