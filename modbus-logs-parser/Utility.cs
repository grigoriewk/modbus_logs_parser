using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_app_for_techart
{
    class Utility
    {
        static public byte[] CRC16(byte[] data)
        {
            byte[] result = new byte[2];
            ushort CRC = 0xFFFF;
            char CRCl;

            for (int i = 0; i < data.Length - 2; i++)
            {
                CRC ^= data[i];
                for (int k = 0; k < 8; k++)
                {
                    CRCl = (char)(CRC & 0x0001);
                    CRC = (ushort)((CRC >> 1) & 0x7FFF);
                    if (CRCl == 1)
                    {
                        CRC = (ushort)(CRC ^ 0xA001);
                    }
                }
            }
            result[0] = (byte)(CRC & 0xFF);
            result[1] = (byte)((CRC >> 8) & 0xFF);
            return result;
        }

        static public bool CheckBit(byte x)
        {
            return (x & 0x80) > 0;
        }
    }
}
