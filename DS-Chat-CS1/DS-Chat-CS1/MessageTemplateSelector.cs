
using DS_Chat_CS1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DS_Chat_CS1
{
    class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MeTextTemplate { get; set; }
        public DataTemplate YouTextTemplate { get; set; }
        public DataTemplate MeImageTemplate { get; set; }
        public DataTemplate YouImageTemplate { get; set; }
        public DataTemplate MeMediaTemplate { get; set; }
        public DataTemplate YouMediaTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Console.WriteLine("entered");
            var message = item as Message;

            if (message.Side == MessageSide.Me)
            {
                if(message is TextMessage)
                {
                    return MeTextTemplate;
                }
                else if(message is ImageMessage)
                {
                    return MeImageTemplate;
                }
                else if(message is MediaMessage)
                {
                    return MeMediaTemplate;
                }
                
            }
            else
            {
                if (message is TextMessage)
                {
                    return YouTextTemplate;
                }
                else if (message is ImageMessage)
                {
                    return YouImageTemplate;
                }
                else if (message is MediaMessage)
                {
                    return YouMediaTemplate;
                }
            }
            return null;
        }
    }
}
