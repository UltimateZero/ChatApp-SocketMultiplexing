
using DS_Chat_CS1.Model;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;

namespace DS_Chat_CS1
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : MetroWindow
    {
        public MessageCollection messages = new MessageCollection();
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        public ChatWindow()
        {
            AllocConsole();
            InitializeComponent();
            messages.Add(new TextMessage()
            {
                Side = MessageSide.You,
                Text = "Hello sir. How may I help you?"
            });

            messages.Add(new TextMessage()
            {
                Side = MessageSide.Me,
                Text = "Hello sir. How may I help you?"
            });

            var msg = new MediaMessage()
            {
                Side = MessageSide.You
            };



            var msg1 = new ImageMessage()
            {
                Side = MessageSide.You,
                ImageUrl = @"C:\Users\UltimateZero\Pictures\maxresdefault.jpg", Loading=100
        };

            messages.Add(msg1);
            messages.Add(new ImageMessage()
            {
                Side = MessageSide.Me,
                ImageUrl = @"C:\Users\UltimateZero\Pictures\maxresdefault.jpg",
                Loading = 100
            });




            messages.Add(new MediaMessage()
            {
                Side = MessageSide.Me,
                MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4",
                Loading = 0
            });
            messages.Add(new MediaMessage()
            {
                Side = MessageSide.You,
                MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4",
                Loading = 0
            });

            var asd = new MediaMessage()
            {
                Side = MessageSide.You,
                MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4",
                Loading = 0
            };
            messages.Add(asd);
             test(asd);

            this.DataContext = messages;
        }

        private async void test(MediaMessage msg)
        {
            await Task.Run(() => {
                for(int i = 0; i < 4; i++)
                {
                    Thread.Sleep(500);
                    msg.Loading += 25;
                }
                
            });
            msg.MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4"; // @"C:\Users\UltimateZero\Pictures\maxresdefault.jpg";
            Console.WriteLine("Done");
        }

        private void txtSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                }
                else
                {
                    e.Handled = true;
                    messages.Add(new TextMessage()
                    {
                        Side = MessageSide.Me,
                        Text = txtSend.Text
                    });
                    txtSend.Clear();
                }
            }
 
        }

  
    }
}
