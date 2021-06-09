using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNaS_Lab_2
{
    public class Icmp
    {
        public static byte[] CreateIcmpPackage()
        {
            byte[] package = new byte[64];

            // Тип 8 - эхо запрос
            package[0] = 8;
            // Код 0 
            package[1] = 0;
            // Контрольная сумма (BE)
            package[2] = 0xF7;
            // Контрольная сумма (LE)
            package[3] = 0xFF;

            return package;
        }

        public static byte GetIcmpType(byte[] data)
        {
            return data[20];
        }
    }
}
