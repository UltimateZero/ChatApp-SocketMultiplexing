using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core.Protocol
{
    class FyzrParser
    {
        enum Phase
        {
            METHOD, HEADER, BODY
        }

        public static FyzrPacket FromData(byte[] data)
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
                            try
                            {
                                packet.method = (FyzrPacket.Method)Enum.Parse(typeof(FyzrPacket.Method), method);
                            } catch(Exception)
                            {
                                packet.method = FyzrPacket.Method.UNKNOWN;
                            }
                            

                        }
                        else {
                            method += (char)b;
                        }
                            
                        break;
                    case Phase.HEADER:
                        if (b == '\n')
                        {
                            newLineCounter++;
                            header += '\n';
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

                        }
                        else
                        {
                            header += (char)b;
                            newLineCounter = 0;
                        }
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

            //Console.WriteLine("From Data: \n" + packet.ToString());
            return packet;

        }


        public static byte[] ToData(FyzrPacket packet)
        {
            Console.WriteLine("To Data: \n" + packet.ToString());
            List<byte> data = new List<byte>();


          
            data.AddRange(Encoding.ASCII.GetBytes(packet.method.ToString()));

            data.Add((byte)'\n');

            foreach (KeyValuePair<string, string> entry in packet.headers)
            {
                string line = "";
                line += entry.Key + ": " + entry.Value + "\n";
                data.AddRange(Encoding.ASCII.GetBytes(line));
            }

            data.Add((byte)'\n');

            data.AddRange(packet.body);


            return data.ToArray();
        }

    }
}
