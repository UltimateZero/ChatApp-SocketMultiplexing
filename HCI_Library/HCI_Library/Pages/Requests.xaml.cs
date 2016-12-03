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
    /// Interaction logic for Requests.xaml
    /// </summary>
    public partial class Requests : UserControl, ISwitchable
    {
        private static Requests instance = null;

        public static Requests getInstance()
        {
            if (instance == null)
            {
                instance = new Requests();
            }
            return instance;
        }

        public void CheckUser()
        {
            //throw new NotImplementedException();
        }

        class Request
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public BitmapImage ImageUrl { get; set; }
            public string OrderDate { get; set; }
            public string Expiry { get; set; }
            public string State { get; set; }
        }

        public Requests()
        {
            InitializeComponent();
            var uriSource = new Uri("/HCI_Library;component/Pages/Icons/Book2.jpg", UriKind.Relative);
            var image = new BitmapImage(uriSource);
            for (int i = 0; i < 20; i++)
            {
                listRequests.Items.Add(new Request() { Title = "Software Engineering 8 " + i, Author = "Sommerville", ImageUrl = image,
                OrderDate="11/11/2016", Expiry="18/11/2016", State="Pending approval"});
            }

        }
    }
}
