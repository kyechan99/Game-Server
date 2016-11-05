using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

/********************************* 패킷
 * CONNECT : 접속 성공
 * DISCONNECT : 접속 끊김
 * LOGIN : 로그인
 * LOGOUT : 로그아웃
 * USER : 유저 정보 ( 내가 아닌 유저 )
 * ADDUSER : 나 ( 본인 추가 )
 * MOVE : 이동
 * CHAT : 채팅
 */

namespace Server
{
    class User
    {
        UserData userData = new UserData();
        /**** 유저가 가지고 있을 정보 (변수) ********************/
        string nickName = "";
        float posX = 0, posY = 0;
        MOVE_CONTROL myMove = MOVE_CONTROL.STOP;
        MOVE_CONTROL seeDirection = MOVE_CONTROL.STOP;
        /****************************************************/


        public User(Socket socket)
        {
            userData.workSocket = socket;

            userData.workSocket.BeginReceive(userData.buf, userData.recvLen, UserData.BUFFER_SIZE, 0, new AsyncCallback(ReadCallBack), userData);
            SendMsg("CONNECT");
        }

        /**
         * @brief 클라이언트로 보내는 패킷
         * @param result 결과
         */
        void ReadCallBack(IAsyncResult result)
        {
            try
            {
                Socket handler = userData.workSocket;
                int bytesRead = handler.EndReceive(result);

                if (bytesRead > 0)
                {
                    userData.recvLen += bytesRead;

                    while (true)
                    {
                        short len = 0;
                        Util.GetShort(userData.buf, 0, out len);

                        if (len > 0 && userData.recvLen >= len)
                        {
                            ParsePacket(len);
                            userData.recvLen -= len;

                            if (userData.recvLen > 0)
                            {
                                Buffer.BlockCopy(userData.buf, len, userData.buf, 0, userData.recvLen);
                            }
                            else
                            {
                                handler.BeginReceive(userData.buf, userData.recvLen, UserData.BUFFER_SIZE, 0, new AsyncCallback(ReadCallBack), userData);
                                break;
                            }
                        }
                        else
                        {
                            handler.BeginReceive(userData.buf, userData.recvLen, UserData.BUFFER_SIZE, 0, new AsyncCallback(ReadCallBack), userData);
                            break;
                        }
                    }
                }
                else
                {
                    handler.BeginReceive(userData.buf, userData.recvLen, UserData.BUFFER_SIZE, 0, new AsyncCallback(ReadCallBack), userData);
                }
            }
            catch (Exception)
            {
                Server.RemoveUser(this);
                Console.WriteLine("User Closed");
            }
        }

        /**
         * brief 패킷 분석
         * param len 길이
         */
        private void ParsePacket(int len)
        {
            string msg = Encoding.UTF8.GetString(userData.buf, 2, len - 2);
            string[] txt = msg.Split(':');      // 암호를 ':' 로 분리해서 읽음

            /************* 기능이 추가되면 덧붙일 것 ***************/
            if (txt[0].Equals("LOGIN"))
            {
                nickName = txt[1];
                Console.WriteLine(txt[1] + " is Login.");
                Login();
            }
            else if (txt[0].Equals("DISCONNECT"))
            {
                if (nickName.Length > 0)
                {
                    Console.WriteLine(nickName + " is Logout.");
                    Logout();
                }
                userData.workSocket.Shutdown(SocketShutdown.Both);
                userData.workSocket.Close();
            }
            else if (txt[0].Equals("MOVE"))
            {
                posX = float.Parse(txt[1]);
                posY = float.Parse(txt[2]);
                myMove = (MOVE_CONTROL)int.Parse(txt[3]);
                Move();
            }
            else if (txt[0].Equals("CHAT"))
            {
                Chat(txt[1]);
            }
            else
            {
                //!< 이 부분에 들어오는 일이 있으면 안됨 (패킷 실수)
                Console.WriteLine("Un Correct Message ");
            }
        }

        /**
         * @brief 로그인
         */
        void Login()
        {
            for (int i = 0; i < Server.v_user.Count; i++)
            {
                //!< 내가 아닌 다른 유저에게
                if (Server.v_user[i] != this)
                {
                    /******** 유저 정보들을 이곳에 추가 *********/
                    SendMsg(string.Format("USER:{0}:{1}:{2}:{3}:{4}", Server.v_user[i].nickName, Server.v_user[i].posX, Server.v_user[i].posY,
                        (int)Server.v_user[i].myMove, (int)Server.v_user[i].seeDirection));      // 현재 접속되 있는 유저 정보들을 방금 들어온 유저에게 전송

                    Server.v_user[i].SendMsg(string.Format("USER:{0}:{1}:{2}:{3}:{4}", nickName, /*posX*/0, /*posY*/0, (int)MOVE_CONTROL.STOP, (int)MOVE_CONTROL.DOWN));      // 기존에 접속해 있던 모든 유저들에게 내 정보 전송.
                    /****************************************/
                }
                else
                {
                    SendMsg(string.Format("ADDUSER:{0}", nickName));
                }
            }
        }

        /**
         * @brief 유저가 나가졌을때 다른 유저에게 이를 알림
         */
        void Logout()
        {
            int index = Server.v_user.IndexOf(this);

            for (int i = 0; i < Server.v_user.Count; i++)
            {
                if (Server.v_user[i] != this)
                {
                    Server.v_user[i].SendMsg(string.Format("LOGOUT:{0}", index));
                }
            }
        }

        /**
         * @brief 채팅
         */
        void Chat(string txt)
        {
            int idx = Server.v_user.IndexOf(this);

            for (int i = 0; i < Server.v_user.Count; i++)
            {
                Server.v_user[i].SendMsg(string.Format("CHAT:{0}:{1}", idx, txt));
            }
        }

        void Move()
        {
            int idx = Server.v_user.IndexOf(this);

            for (int i = 0; i < Server.v_user.Count; i++)
            {
                if (Server.v_user[i] != this)
                {
                    Server.v_user[i].SendMsg(string.Format("MOVE:{0}:{1}:{2}:{3}", idx, posX, posY, (int)myMove)); // 내 인덱스 번호와 현재 위치 이동할 방향을 보낸다.
                }
            }
            if (myMove > MOVE_CONTROL.STOP) seeDirection = myMove; // STOP이 아닌 경우 마지막 바라보던 방향을 저장해둔다.
        }

        /**
         * @brief 클라이언트로 보내는 패킷
         * @param msg 클라이언트가 인식할 메세지, 일종의 암호 (?)
         */
        void SendMsg(string msg)
        {
            try
            {
                if (userData.workSocket != null && userData.workSocket.Connected)
                {
                    byte[] buff = new byte[4096];
                    Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(msg).Length + 2), 0, buff, 0, 2);
                    Buffer.BlockCopy(Encoding.UTF8.GetBytes(msg), 0, buff, 2, Encoding.UTF8.GetBytes(msg).Length);
                    userData.workSocket.Send(buff, Encoding.UTF8.GetBytes(msg).Length + 2, 0);
                }
            }
            catch (System.Exception e)
            {
                if (nickName.Length > 0) Logout();

                userData.workSocket.Shutdown(SocketShutdown.Both);
                userData.workSocket.Close();

                Server.RemoveUser(this);

                Console.WriteLine("SendMsg Error : " + e.Message);
            }
        }

        /**
         * @brief 클라이언트로 보내는 패킷
         */
        byte[] ShortToByte(int val)
        {
            byte[] temp = new byte[2];
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }
    }
}
