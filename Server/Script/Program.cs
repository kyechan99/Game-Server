using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(10000);      //!< 포트 번호

            while(true)
            {
                Thread.Sleep(1);
            }
        }
    }
}
