
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Model
{
    class MediaMessage : Message, INotifyPropertyChanged
    {
        private string _mediaUrl;
        public string MediaUrl
        {
            get { return _mediaUrl; }
            set
            {
                _mediaUrl = value;
                RaisePropertyChanged("MediaUrl");
            }
        }
        private int _loading = 0;
        public int Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                RaisePropertyChanged("Loading");
            }
        }


        public MediaMessage()
        {
            Task.Run(() => {
                for (int i = 0; i < 4; i++)
                {
                    Thread.Sleep(500);
                    Loading += 25;
                }

            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
