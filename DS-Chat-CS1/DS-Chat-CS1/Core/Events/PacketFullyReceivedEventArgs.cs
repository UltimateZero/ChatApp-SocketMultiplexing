﻿using DS_Chat_CS1.Core.LowLevel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Events
{
    public class PacketFullyReceivedEventArgs
    {
        public Packet Packet { get; set; }
    }
}
