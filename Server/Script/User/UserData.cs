using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

public enum MOVE_CONTROL
{
    STOP,
    DANCE,
    UP,
    DOWN,
    LEFT,
    RIGHT,
}

namespace Server
{
    class UserData
    {
        public const int BUFFER_SIZE = 32768;    // 2바이트 대비

        public Socket workSocket = null;                // 소켓
        public byte[] buf = new byte[BUFFER_SIZE];      // 버퍼
        public int recvLen = 0;                         // 데이터 길이
    }
}
