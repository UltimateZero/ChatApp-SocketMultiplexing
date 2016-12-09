using DS_Chat_CS1.Core.LowLevel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Model
{
    class User
    {
        public Client Connection;
        public string Username { get; set; }
        public IPEndPoint Endpoint { get { return Connection.GetRemoteEndPoint(); } }
    }
}
