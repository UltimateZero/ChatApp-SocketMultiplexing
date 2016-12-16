using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketMultiplexingLib.Events
{
    public class DataReceivedEventArgs
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public bool isEnd;
    }
}
