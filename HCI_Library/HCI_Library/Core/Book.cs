using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HCI_Library.Core
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public BitmapImage ImageUrl { get; set; }
        public override string ToString()
        {
            return "Book: Title=" + Title;
        }
    }
}
