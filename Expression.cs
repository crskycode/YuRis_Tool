using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuRis_Tool
{
    class Expression
    {
        readonly Encoding _encoding = Encoding.GetEncoding("shift_jis");

        public void Execute(BinaryReader reader, int address, int length)
        {
            reader.BaseStream.Position = address;

            var endPosition = reader.BaseStream.Position + length;

            while (reader.BaseStream.Position < endPosition)
            {
                var pointer = reader.BaseStream.Position;
                var opcode = reader.ReadByte();
                var opcodeLength = reader.ReadInt16();

                switch (opcode)
                {
                    // sub_42294C
                    case 0x21:
                    {
                        Debug.WriteLine($"{pointer:X8} NE");
                        break;
                    }
                    // sub_422EB4
                    case 0x25:
                    {
                        Debug.WriteLine($"{pointer:X8} Mod");
                        break;
                    }
                    // sub_422FBC
                    case 0x26:
                    {
                        Debug.WriteLine($"{pointer:X8} Land(&&)");
                        break;
                    }
                    // sub_421B28
                    case 0x29:
                    {
                        // [dummy]
                        reader.ReadByte();
                        break;
                    }
                    // sub_422D88
                    case 0x2A:
                    {
                        Debug.WriteLine($"{pointer:X8} Mul");
                        break;
                    }
                    // sub_42268C
                    case 0x2B:
                    {
                        Debug.WriteLine($"{pointer:X8} Add");
                        break;
                    }
                    // sub_422D24
                    case 0x2D:
                    {
                        Debug.WriteLine($"{pointer:X8} Sub");
                        break;
                    }
                    // sub_422E00
                    case 0x2F:
                    {
                        Debug.WriteLine($"{pointer:X8} Div");
                        break;
                    }
                    // sub_422B58
                    case 0x3C:
                    {
                        Debug.WriteLine($"{pointer:X8} LT(<)");
                        break;
                    }
                    // sub_4227DC
                    case 0x3D:
                    {
                        Debug.WriteLine($"{pointer:X8} EQ");
                        break;
                    }
                    // sub_422ABC
                    case 0x3E:
                    {
                        Debug.WriteLine($"{pointer:X8} BT(>)");
                        break;
                    }
                    // sub_421F20
                    case 0x42:
                    {
                        // Load byte

                        // [value]
                        var val = reader.ReadByte();
                        Debug.WriteLine($"{pointer:X8} Push Int8 0x{val:X}");
                        break;
                    }
                    // sub_420FA0
                    case 0x48:
                    {
                        // Initialize a variable

                        // [type]
                        reader.ReadByte();
                        // [id]
                        var id = reader.ReadUInt16();

                        Debug.WriteLine($"{pointer:X8} Init ${id}");

                        break;
                    }
                    //sub_422078
                    case 0x4C:
                    {
                        // Push Immd Int64
                        // [value]
                        var val = reader.ReadInt64();
                        Debug.WriteLine($"{pointer:X8} Push Int64 0x{val:X}");
                        break;
                    }
                    // sub_420D94
                    case 0x4D:
                    {
                        // Push string

                        // [string]
                        var bstr = reader.ReadBytes(opcodeLength);
                        var str = _encoding.GetString(bstr);
                        Debug.WriteLine($"{pointer:X8} Push STR {str}");
                        break;
                    }
                    // sub_422C88
                    case 0x53:
                    {
                        Debug.WriteLine($"{pointer:X8} LE(<=)");
                        break;
                    }
                    // sub_42198C
                    case 0x56:
                    {
                        // [type]
                        reader.ReadByte();
                        // [id]
                        var id = reader.ReadUInt16();

                        Debug.WriteLine($"{pointer:X8} R ${id}");

                        break;
                    }
                    // sub_421F98
                    case 0x57:
                    {
                        // Push Immd Int16
                        // [value]
                        var val = reader.ReadInt16();
                        Debug.WriteLine($"{pointer:X8} Push Int16 0x{val:X}");
                        break;
                    }
                    // sub_422BF0
                    case 0x5A:
                    {
                        Debug.WriteLine($"{pointer:X8} BE(>=)");
                        break;
                    }
                    // sub_4221C8
                    case 0x73:
                    {
                        // NumToStr
                        Debug.WriteLine($"{pointer:X8} NumToStr");
                        break;
                    }
                    // sub_423068
                    case 0x7C:
                    {
                        Debug.WriteLine($"{pointer:X8} Lor(||)");
                        break;
                    }
                    default:
                    {
                        throw new Exception("unknow opcode");
                    }
                }
            }
        }
    }
}
