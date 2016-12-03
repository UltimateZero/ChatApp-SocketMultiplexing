using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSockets2
{
    class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }
}
