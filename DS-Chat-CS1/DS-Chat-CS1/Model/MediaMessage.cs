
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
        private double _loading = 0;
        public double Loading
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

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
