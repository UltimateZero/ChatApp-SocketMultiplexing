
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Model
{
    class ImageMessage : Message, INotifyPropertyChanged
    {
        private string _imageUrl;
        public string ImageUrl { get { return _imageUrl; }
            set {
                _imageUrl = value;
                RaisePropertyChanged("ImageUrl");
            }
        }
        private string _progress;
        public string Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        private string _fileName;
        public string Filename
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged("Filename");
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
