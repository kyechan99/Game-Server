using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Util
    {
        public static int GetShort(byte[] buf, int idx, out short val)
        {
            val = BitConverter.ToInt16(buf, idx);
            return idx + 2;
        }

        public static int GetInteger(byte[] buf, int idx, out short val)
        {
            val = BitConverter.ToInt16(buf, idx);
            return idx + 2;
        }

        public static int GetString(byte[] buf, int idx, out short val)
        {
            val = BitConverter.ToInt16(buf, idx);
            return idx + 2;
        }

        public static byte[] InitToByte(int val)
        {
            byte[] temp = new byte[4];
            temp[3] = (byte)((val & 0xff000000) >> 24);
            temp[2] = (byte)((val & 0x00ff0000) >> 16);
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        public static byte[] ShortToByte(int val)
        {
            byte[] temp = new byte[2];
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        public static int SetShort(byte[] buf, int idx, int val)
        {
            Buffer.BlockCopy(ShortToByte(val), 0, buf, idx, 2);
            return idx + 2;
        }

        public static int SetInteger(byte[] buf, int idx, int val)
        {
            Buffer.BlockCopy(InitToByte(val), 0, buf, idx, 4);
            return idx + 4;
        }

        public static int SetString(byte[] buf, int idx, string txt)
        {
            byte[] temp = Encoding.UTF8.GetBytes(txt);
            Buffer.BlockCopy(ShortToByte(temp.Length), 0, buf, idx, 2);
            Buffer.BlockCopy(temp, 0, buf, idx + 2, temp.Length);
            return idx + temp.Length + 2;
        }
    }
}
