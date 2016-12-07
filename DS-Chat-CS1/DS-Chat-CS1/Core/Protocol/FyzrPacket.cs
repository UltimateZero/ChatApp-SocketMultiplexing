using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Protocol
{
    class FyzrPacket
    {
        public enum Method
        {
            COMMAND,
            TEXT,
            FILE
        }

        public FyzrPacket()
        {
            headers = new Dictionary<string, string>();
        }

        public Method method;
        public Dictionary<string, string> headers;
        public byte[] body;
    }
}
