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
using static HCI_Library.Core.User;

namespace HCI_Library.Pages
{
    /// <summary>
    /// Interaction logic for Books.xaml
    /// </summary>
    public partial class Books : UserControl, ISwitchable
    {
        private static Books instance = null;

        public static Books getInstance()
        {
            if (instance == null)
            {
                instance = new Books();
            }
            return instance;
        }


        public Books()
        {
            InitializeComponent();
            var uriSource = new Uri("/HCI_Library;component/Pages/Icons/Book2.jpg", UriKind.Relative);
            var image = new BitmapImage(uriSource);
            for (int i = 0; i < 20; i++)
            {
                listBooks.Items.Add(new Book() { Title = "Software Engineering 8 " + i, Author = "Sommerville", ImageUrl = image });
            }

        }

        public void CheckUser()
        {
            User currentUser = ConnectionManager.getInstance().CurrentUser;
            switch (currentUser.Type)
            {
                case UserType.Student:
                    btnAdd.Visibility = Visibility.Hidden;
                    break;
                case UserType.Teacher:
                    btnAdd.Visibility = Visibility.Visible;
                    break;
                case UserType.Admin:
                    btnAdd.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void btnRefresh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnAdd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(
                 new Action(() => {
                  MainAll.getInstance().ShowBookInfo(null);

             })
            );
           
        }



        private void listBook_Request(object sender, RoutedEventArgs e)
        {
           
            RequestBook(listBooks.SelectedItem as Book);
        }

        private void listBooks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(listBooks.SelectedItem == null)
            {
                return;
            }

            RequestBook(listBooks.SelectedItem as Book);
        }

        private void RequestBook(Book book)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to send a request for: " + book.Title, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                
            }
        }
    }
}
