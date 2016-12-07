using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Protocol
{
    class FyzerParser
    {
        enum Phase
        {
            METHOD, HEADER, BODY
        }



        public FyzrPacket FromData(byte[] data)
        {
            Phase phase = Phase.METHOD;
            FyzrPacket packet = new FyzrPacket();
            string method = "";
            string header = "";
            int newLineCounter = 0;
            int bodyCounter = 0;
            

            foreach (byte b in data)
            {
                switch (phase)
                {
                    case Phase.METHOD:
                        if (b == '\n')
                        {
                            phase = Phase.HEADER;

                            if (method.Equals("COMMAND"))
                            {
                                packet.method = FyzrPacket.Method.COMMAND;
                            }
                            else if (method.Equals("TEXT"))
                            {
                                packet.method = FyzrPacket.Method.TEXT;
                            }
                            else if (method.Equals("FILE"))
                            {
                                packet.method = FyzrPacket.Method.FILE;
                            }

                        }
                        else
                            method += (char)b;
                        break;
                    case Phase.HEADER:
                        if (b == '\n')
                        {
                            newLineCounter++;
                            if (newLineCounter == 2)
                            {
                                phase = Phase.BODY;

                                string[] lines = header.Split(new char[] { '\n' });
                                foreach (string line in lines)
                                {
                                    string[] pair = line.Split(new char[] { ':' });
                                    if (pair.Length == 2)
                                    {
                                        packet.headers.Add(pair[0].Trim(), pair[1].Trim());
                                    }
                                }


                            }
                            else
                            {
                                newLineCounter = 0;
                            }

                        }
                        else
                            header += (char)b;
                        break;
                    case Phase.BODY:
                        if (packet.body == null)
                        {
                            int length = Convert.ToInt32(packet.headers["Content-Length"]);
                            packet.body = new byte[length];
                        }

                        packet.body[bodyCounter++] = b;


                        break;
                }
            }


            return packet;

        }


        public byte[] ToData(FyzrPacket packet)
        {
            List<byte> data = new List<byte>();

            switch (packet.method)
            {
                case FyzrPacket.Method.COMMAND:
                    data.AddRange(Encoding.ASCII.GetBytes("COMMAND"));
                    break;
                case FyzrPacket.Method.TEXT:
                    data.AddRange(Encoding.ASCII.GetBytes("TEXT"));
                    break;
                case FyzrPacket.Method.FILE:
                    data.AddRange(Encoding.ASCII.GetBytes("FILE"));
                    break;
            }

            data.Add((byte)'\n');

            foreach (KeyValuePair<string, string> entry in packet.headers)
            {
                string line = "";
                line += entry.Key + ": " + entry.Value + "\n";
            }

            data.Add((byte)'\n');

            data.AddRange(packet.body);


            return data.ToArray();
        }

    }
}
