using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Protocol
{
    public class TextProtocol : MessageProtocol
    {
        public static FyzrPacket CreateTextPacket(string message, MessageType type, string to)
        {
            FyzrPacket packet = new FyzrPacket();
            packet.method = FyzrPacket.Method.TEXT;
            packet.headers.Add("Content-Encoding", Encoding.Default.WebName);
            packet.headers.Add("Chat-Type", "" + type);

            if (to != null)
                packet.headers.Add("To", to);


            packet.body = Encoding.Default.GetBytes(message);

            packet.headers.Add("Content-Length", "" + packet.body.Length);

            return packet;
        }
    }
}
