using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Protocol
{
    public class FileProtocol : MessageProtocol 
    {
        public static FyzrPacket CreateFilePacket(byte[] segment, string fileName, MessageType type, string to)
        {
            FyzrPacket packet = new FyzrPacket();
            packet.method = FyzrPacket.Method.FILE;
            packet.headers.Add("Chat-Type", "" + type);
            packet.headers.Add("Filename", fileName);
            if (to != null)
                packet.headers.Add("To", to);


            packet.body = segment;

            packet.headers.Add("Content-Length", "" + packet.body.Length);

            return packet;
        }
    }
}
