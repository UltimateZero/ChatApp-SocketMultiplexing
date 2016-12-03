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
    /// Interaction logic for BookInfo.xaml
    /// </summary>
    public partial class BookInfo : UserControl
    {
        private static BookInfo instance = null;

        public static BookInfo getInstance()
        {
            if (instance == null)
            {
                instance = new BookInfo();
            }
            return instance;
        }

        private Book currentBook;

        public BookInfo()
        {
            InitializeComponent();
        }

        public void SetBook(Book book)
        {
            currentBook = book;

            txtTitle.Text = book.Title;
            txtAuthor.Text = book.Author;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
