using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DS_Chat_CS1.Core
{
    class UserConnection
    {
        List<Socket> fileSockets;
        Socket textSocket;

        //Main socket for text and data (typing notifications, etc) AND multiple sockets for files
        
        //One socket, multiplexing 


    }
}
