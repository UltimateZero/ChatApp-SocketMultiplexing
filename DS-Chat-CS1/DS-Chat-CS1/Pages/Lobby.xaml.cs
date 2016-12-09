using System;
using System.Collections.Generic;
using System.Linq;
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
            ListBoxItem item = new ListBoxItem();
            item.Content = "Hello";
            item.ToolTip = "IP";
            listUsers.Items.Add(item);
        }
    }
}
