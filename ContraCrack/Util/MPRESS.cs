using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ContraCrack.Util
{
    public static class MPRESS
    {
        public static bool GetPackedAssembly(string fileName, out byte[] outBytes)
        {
            FileStream input = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            int length = (int)input.Length;
            input.Seek(60L, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(input);
            int read = reader.ReadInt32();
            if ((read >= 2) && (read <= (length - 512)))
            {
                input.Seek((long)read, SeekOrigin.Begin);
                if (reader.ReadUInt32() == 17744)
                {
                    read += 348;
                    input.Seek((long)read, SeekOrigin.Begin);
                    int read2 = reader.ReadInt32();
                    if ((read2 < length) && (read2 >= 768))
                    {
                        read2 += 16;
                        length -= read2;
                        byte[] buffer = new byte[length];
                        input.Seek((long)read2, SeekOrigin.Begin);
                        input.Read(buffer, 0, length);
                        input.Close();
                        if (UnpackBytes(buffer, out outBytes, length))
                        {
                            return true;
                        }
                    }
                }
            }
            outBytes = null;
            return false;
        }
        public static bool UnpackBytes(byte[] c, out byte[] a, int l)
        {
            a = null;
            if ((c[4] == 77) && (c[6] == 90))
            {
                int fileLength = (((8 + c[0]) + (c[1] << 8)) + (c[2] << 16)) + (c[3] << 24);
                byte[] emptyByteArray = new byte[fileLength];
                if (lzmat(emptyByteArray, c, l) != 0)
                {
                    a = emptyByteArray;
                    return true;
                }
            }
            return false;
        }
        public static unsafe int lzmat(byte[] byteArray1, byte[] byteArray2, int int1)
        {
            int num2;
            byte[] buffer = new byte[] {};
            //Try swapping these, they might be wrong
            fixed (byte* numRef2 = buffer)
            {
                fixed (byte* numRef = buffer)
                {
                    byte num3 = 0;
                    num2 = 0;
                    int index = 0;
                    numRef[0] = numRef2[4];
                    index = 5;
                    num2 = 1;
                    num3 = 0;
                    while (index < (int1 - num3))
                    {
                        byte num5 = numRef2[index++];
                        if (num3 != 0)
                        {
                            num5 = (byte)(num5 >> 4);
                            num5 = (byte)(num5 + ((byte)(numRef2[index] << 4)));
                        }
                        int num4 = 0;
                        while ((num4 < 8) && (index < (int1 - num3)))
                        {
                            if ((num5 & 128) != 128)
                            {
                                numRef[num2] = numRef2[index];
                                if (num3 != 0)
                                {
                                    numRef[num2] = (byte)(numRef[num2] >> 4);
                                    IntPtr* ptr1 = (IntPtr*)(numRef + num2);
                                    ptr1[0] = (IntPtr)((byte)(ptr1[0] + ((byte)(numRef2[index + 1] << 4))));
                                }
                                num2++;
                                index++;
                            }
                            int num9 = numRef2[index];
                            if (num3 != 0)
                            {
                                num9 = num9 >> 4;
                            }
                            index++;
                            num9 &= 1048575;
                            int num8 = 0;
                            if (num2 < 2177)
                            {
                                num8 = num9 >> 1;
                                if ((num9 & 1) == 1)
                                {
                                    index += num3;
                                    num8 = (num8 & 2047) + 129;
                                    num3 = (byte)(num3 ^ 1);
                                }
                                else
                                {
                                    num8 = (num8 & 127) + 1;
                                }
                            }
                            else
                            {
                                num8 = num9 >> 2;
                                switch ((num9 & 3))
                                {
                                    case 0:
                                        num8 = (num8 & 63) + 1;
                                        goto Label_01CC;

                                    case 1:
                                        index += num3;
                                        num8 = (num8 & 1023) + 65;
                                        num3 = (byte)(num3 ^ 1);
                                        goto Label_01CC;

                                    case 2:
                                        num8 = (num8 & 16383) + 1089;
                                        index++;
                                        goto Label_01CC;

                                    case 3:
                                        index += 1 + num3;
                                        num8 = (num8 & 262143) + 17473;
                                        num3 = (byte)(num3 ^ 1);
                                        goto Label_01CC;
                                }
                            }
                        Label_01CC:
                            int num7 = numRef2[index];
                            if (num3 != 0)
                            {
                                num7 = num7 >> 4;
                                num3 = 0;
                                index++;
                            }
                            else
                            {
                                num7 &= 4095;
                                num3 = 1;
                            }
                            if ((num7 & 15) != 15)
                            {
                                num7 = (num7 & 15) + 3;
                            }
                            else
                            {
                                index++;
                                if (num7 != 4095)
                                {
                                    num7 = (num7 >> 4) + 18;
                                }
                                else
                                {
                                    num7 = numRef2[index];
                                    if (num3 != 0)
                                    {
                                        num7 = num7 >> 4;
                                    }
                                    num7 &= 65535;
                                    index += 2;
                                    if (num7 == 65535)
                                    {
                                        if (num3 != 0)
                                        {
                                            IntPtr choni = (IntPtr)((numRef2 + index) - 4);
                                            num7 = ((int)choni & 252) << 5;
                                            index++;
                                            num3 = 0;
                                        }
                                        else
                                        {
                                            IntPtr ptr = (IntPtr)((numRef2 + index) - 5);
                                            num7 = ((int)ptr & 4032) << 1;
                                        }
                                        num7 += (num5 & 127) + 4;
                                        num7 = num7 << 1;
                                        while (num7-- != 0)
                                        {
                                            numRef[num2] = numRef2[index];
                                            index += 4;
                                            num2 += 4;
                                        }
                                        break;
                                    }
                                    num7 += 273;
                                }
                            }
                            int num6 = num2 - num8;
                            while (num7-- != 0)
                            {
                                numRef[num2++] = numRef[num6++];
                            }
                            num4++;
                            num5 = (byte)(num5 << 1);
                        }
                    }
                }
            }
            return num2;
        }


    }
}
