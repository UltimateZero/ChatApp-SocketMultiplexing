﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Events
{
    class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }
}
