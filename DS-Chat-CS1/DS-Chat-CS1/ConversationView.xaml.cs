using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            //Console.WriteLine(bar.Value);
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
                        //break;
                    }
                    if(ob is Grid && ob != pp)
                    {
                        Grid ss = ob as Grid;
                        ss.Visibility = Visibility.Visible;
                    }
                }
     
            }
        }

        private void progImgbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MetroProgressBar bar = sender as MetroProgressBar;

            //Console.WriteLine(bar.Value);
            if (bar.Value == 100)
            {
                Grid pp = (Grid)bar.Parent;
                pp.Visibility = Visibility.Hidden;
 

            }
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            Image img = null;
            if (mnu != null)
            {
                img = ((ContextMenu)mnu.Parent).PlacementTarget as Image;
                
                ImageSource source = img.Source;
                Console.WriteLine("Got Image source " + source);
                Process.Start(source.ToString());
            }
        }

        private void OpenMedia_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            MediaElement img = null;
            if (mnu != null)
            {
                img = ((ContextMenu)mnu.Parent).PlacementTarget as MediaElement;

                Uri source = img.Source;
                Console.WriteLine("Got media source " + source);
                Process.Start(source.ToString());
            }
        }

        private void btnPlayClicked(object sender, RoutedEventArgs e)
        {
            var slider = sender as Button;
            StackPanel pp = (StackPanel)slider.Parent;
            Grid ppp = (Grid)pp.Parent;
            Grid panel = (Grid)ppp.Parent;
            foreach (var ob in panel.Children)
            {
                if (ob is MediaElement)
                {
                    MediaElement el = (MediaElement)ob;
                    el.Play();

                    break;
                }
            }
        }

        private void btnPauseClicked(object sender, RoutedEventArgs e)
        {
            var slider = sender as Button;
            StackPanel pp = (StackPanel)slider.Parent;
            Grid ppp = (Grid)pp.Parent;
            Grid panel = (Grid)ppp.Parent;
            foreach (var ob in panel.Children)
            {
                if (ob is MediaElement)
                {
                    MediaElement el = (MediaElement)ob;
                    el.Pause();

                    break;
                }
            }
        }

        private void btnStopClicked(object sender, RoutedEventArgs e)
        {
            var slider = sender as Button;
            StackPanel pp = (StackPanel)slider.Parent;
            Grid ppp = (Grid)pp.Parent;
            Grid panel = (Grid)ppp.Parent;
            foreach (var ob in panel.Children)
            {
                if (ob is MediaElement)
                {
                    MediaElement el = (MediaElement)ob;
                    el.Stop();

                    break;
                }
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            Grid ppp = (Grid)slider.Parent;
            Grid panel = (Grid)ppp.Parent;
            foreach (var ob in panel.Children)
            {
                if (ob is MediaElement)
                {
                    MediaElement el = (MediaElement)ob;
                    el.Volume = slider.Value/100;
                
                    break;
                }
            }
        }
    }
}
