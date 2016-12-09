using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Protocol
{
    public class FyzrPacket
    {
        public enum Method
        {
            COMMAND,
            TEXT,
            FILE,
            UNKNOWN
        }

        public FyzrPacket()
        {
            headers = new Dictionary<string, string>();
        }

        public Method method;
        public Dictionary<string, string> headers;
        public byte[] body;


        public override string ToString()
        {
            string outp =  method.ToString() + "\n";
            foreach (KeyValuePair<string, string> entry in headers)
            {
                outp += entry.Key + ": " + entry.Value + "\n";
            }
            outp += "\n";
            if (body != null)
                outp += "<body with size " + body.Length + ">";
            else
                outp += "<no body>";
            return outp;

        }
    }
}
