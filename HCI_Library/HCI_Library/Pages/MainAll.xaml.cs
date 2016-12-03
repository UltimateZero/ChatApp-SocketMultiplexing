using HCI_Library.Core;
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

namespace HCI_Library.Pages
{
    /// <summary>
    /// Interaction logic for MainAll.xaml
    /// </summary>
    public partial class MainAll : UserControl
    {
        private static MainAll instance = null;

        public static MainAll getInstance()
        {
            if (instance == null)
            {
                instance = new MainAll();
            }
            return instance;
        }

        public MainAll()
        {
            InitializeComponent();
            Books_Clicked(null, null);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Clicked");
        }

        private void Books_Clicked(object sender, RequestNavigateEventArgs e)
        {
            ClearSelection();
            lblBooks.FontWeight = FontWeights.Bold;
            SwitchTo(Books.getInstance());
        }

        private void Profile_Clicked(object sender, RequestNavigateEventArgs e)
        {
            ClearSelection();
            lblProfile.FontWeight = FontWeights.Bold;
           SwitchTo(Profile.getInstance());
        }

        private void Requests_Clicked(object sender, RequestNavigateEventArgs e)
        {
            ClearSelection();
            lblRequests.FontWeight = FontWeights.Bold;
            SwitchTo(Requests.getInstance());
        }

        private void Help_Clicked(object sender, RequestNavigateEventArgs e)
        {
            ClearSelection();
            lblHelp.FontWeight = FontWeights.Bold;
        }

        private void SwitchTo(ISwitchable page)
        {
            page.CheckUser();
            InnerBody.Content = page;
        }

        private void ClearSelection()
        {
           InnerBody.Content = null;
           lblBooks.FontWeight = FontWeights.Normal;
           lblProfile.FontWeight = FontWeights.Normal;
           lblRequests.FontWeight = FontWeights.Normal;
           lblHelp.FontWeight = FontWeights.Normal;
        }


        public void ShowBookInfo(Book book)
        {
            if(book == null)
            {
                book = new Book();
            }
            BookInfo.getInstance().SetBook(book);
            InnerBody.Content = null;
            InnerBody.Content = BookInfo.getInstance();

        }
    }
}
