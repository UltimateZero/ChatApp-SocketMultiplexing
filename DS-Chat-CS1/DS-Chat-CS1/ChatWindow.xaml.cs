
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
           // AllocConsole();
            InitializeComponent();
        //    messages.Add(new TextMessage()
        //    {
        //        Side = MessageSide.You,
        //        Text = "Hello sir. How may I help you?"
        //    });

        //    messages.Add(new TextMessage()
        //    {
        //        Side = MessageSide.Me,
        //        Text = "Hello sir. How may I help you?"
        //    });

        //    var msg = new MediaMessage()
        //    {
        //        Side = MessageSide.You
        //    };



        //    var msg1 = new ImageMessage()
        //    {
        //        Side = MessageSide.You,
        //        ImageUrl = @"C:\Users\UltimateZero\Pictures\maxresdefault.jpg", Loading=100
        //};

        //    messages.Add(msg1);
        //    messages.Add(new ImageMessage()
        //    {
        //        Side = MessageSide.Me,
        //        ImageUrl = @"C:\Users\UltimateZero\Pictures\maxresdefault.jpg",
        //        Loading = 100
        //    });




        //    messages.Add(new MediaMessage()
        //    {
        //        Side = MessageSide.Me,
        //        MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4",
        //        Loading = 0
        //    });
        //    messages.Add(new MediaMessage()
        //    {
        //        Side = MessageSide.You,
        //        MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4",
        //        Loading = 0
        //    });

        //    var asd = new MediaMessage()
        //    {
        //        Side = MessageSide.You,
        //        MediaUrl = @"D:\MovieZ\Spectre (2015)\Spectre+(2015)+1080p+BluRay+[DayT.se].mp4",
        //        Loading = 0
        //    };
        //    messages.Add(asd);
        //     test(asd);

            this.DataContext = messages;
        }

        internal void AddTextMessage(TextMessage msg)
        {
            Dispatcher.Invoke(() => { AddToMessages(msg); });
            
        }

        internal void ShowWindow()
        {
            Dispatcher.Invoke(()=>{
                Show();
                Activate();
            });
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
                    SendCurrentText();
                }
            }
 
        }

        private void SendCurrentText()
        {
            string text = txtSend.Text.Trim();
            if (text.Length == 0) return;
            TextMessage msg = new TextMessage()
            {
                Side = MessageSide.Me,
                Text = text
            };
            AddToMessages(msg);
            MainContext.Instance.SendMessage(this, msg);
            txtSend.Clear();
        }

        internal ImageMessage CreateImageMessage(string fileName)
        {
            var msg = new ImageMessage()
            {
                Side = MessageSide.You
            };
            Dispatcher.Invoke(() => { AddToMessages(msg); });
            
            return msg;
        }

        private void btnSendImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new System.Windows.Forms.OpenFileDialog();


                // Create OpenFileDialog 
               // Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".png";
                dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


                // Display OpenFileDialog by calling ShowDialog method 
                var result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Open document 
                    string path = dlg.FileName;
                    string filename = dlg.SafeFileName;
                    Console.WriteLine(path);
                    var msg = new ImageMessage()
                    {
                        ImageUrl = path,
                        Side = MessageSide.Me
                    };
                    AddToMessages(msg);
                    Task.Run(() =>
                    {
                        MainContext.Instance.SendImageFile(this, path, filename, msg);
                    });

                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal MediaMessage CreateMediaMessage(string fileName)
        {
            var msg = new MediaMessage()
            {
                Side = MessageSide.You
            };
            Dispatcher.Invoke(() => { AddToMessages(msg); });

            return msg;
        }

        private void btnSendMedia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new System.Windows.Forms.OpenFileDialog();


                // Create OpenFileDialog 
                // Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".png";
                dlg.Filter = "MP3 Files (*.mp3)|*.mp3|MP4 Files (*.mp4)|*.mp4|AVI Files (*.avi)|*.avi";


                // Display OpenFileDialog by calling ShowDialog method 
                var result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Open document 
                    string path = dlg.FileName;
                    string filename = dlg.SafeFileName;
                    Console.WriteLine(path);
                    var msg = new MediaMessage()
                    {
                        MediaUrl = path,
                        Side = MessageSide.Me
                    };
                    AddToMessages(msg);
                    Task.Run(() =>
                    {
                        MainContext.Instance.SendMediaFile(this, path, filename, msg);
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void AddToMessages(Message msg)
        {
            messages.Add(msg);
            ConversationScrollViewer.ScrollToVerticalOffset(ConversationScrollViewer.ExtentHeight);
        }

        private Boolean AutoScroll = true;

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (ConversationScrollViewer.VerticalOffset == ConversationScrollViewer.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                ConversationScrollViewer.ScrollToVerticalOffset(ConversationScrollViewer.ExtentHeight);
            }
        }

        private void btnSendText_Click(object sender, RoutedEventArgs e)
        {
            SendCurrentText();
        }
    }
}
