using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {
        public static List<User> v_user = new List<User>();
        public static List<INFO.Room> v_rooms = new List<INFO.Room>();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static int roomCount;

        /**
         * @brief 초기화
         * @port 포트번호
         */
        public Server(int port)
        {
            v_rooms.Clear();
            v_user.Clear();
            roomCount = 0;
            WaitingSocket(port);
        }

        /**
         * @brief 유저 제거
         * @target 제거 대상
         */
        public static void RemoveUser(User target)
        {
            v_user.Remove(target);
            Console.WriteLine(string.Format("n(User) : {0}", v_user.Count));
        }

        /**
         * @brief 클라이언트의 소켓 접속 대기
         * @part 포트 번호
         */
        public static void WaitingSocket(int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.NoDelay = true;
            listener.LingerState = new LingerOption(true, 0);
            listener.SendBufferSize = 81920;        //!< 때에 맞게 맞추어 사용할 것
            listener.ReceiveBufferSize = 81920;     //!< 때에 맞게 맞추어 사용할 것

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                Console.WriteLine("Waiting For A Connection...");

                while(true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n\nPress ENTER to continue...");
            Console.Read();
        }

        /**
         * @brief 클라이언트가 접속에 성공한 경우 호출되는 콜백 함수
         * @result 결과
         */
        static void AcceptCallback(IAsyncResult result)
        {
            allDone.Set();

            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);
            handler.NoDelay = true;
            handler.LingerState = new LingerOption(true, 0);
            handler.SendBufferSize = 81920;
            handler.ReceiveBufferSize = 81920;

            v_user.Add(new User(handler));
            Console.WriteLine(string.Format("n(User) : {0}", v_user.Count));
        }
    }
}
