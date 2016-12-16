using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketMultiplexingLib.Events
{
    public class DataSentEventArgs
    {
        public PacketStatus PacketStatus { get; set; }
    }
}
