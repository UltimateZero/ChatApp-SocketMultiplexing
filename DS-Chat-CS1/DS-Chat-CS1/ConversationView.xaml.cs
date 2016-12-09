using MahApps.Metro.Controls;
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

namespace DS_Chat_CS1
{
    /// <summary>
    /// Interaction logic for ConversationView.xaml
    /// </summary>
    public partial class ConversationView : UserControl
    {
        public ConversationView()
        {
            InitializeComponent();
        }

        private void MediaElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MediaElement element = sender as MediaElement;
           
            if ("Playing".Equals(element.Name))
            {
                element.Pause();
                element.Name = "Paused";
            }
            else
            {
                element.Play();
                element.Name = "Playing";
            }
        }

        private void MetroProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MetroProgressBar bar = sender as MetroProgressBar;

            Console.WriteLine(bar.Value);
            if(bar.Value == 100)
            {
                Grid pp = (Grid) bar.Parent;
                Grid panel = (Grid)pp.Parent;
                foreach(var ob in panel.Children)
                {
                    if(ob is MediaElement)
                    {
                        MediaElement el = (MediaElement)ob;
                        el.Name = "Playing";
                        el.Play();
                        pp.Visibility = Visibility.Hidden;
                        break;
                    }
                }
     
            }
        }


    }
}
