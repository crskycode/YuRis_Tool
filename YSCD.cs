using System;
using System.Collections.Generic;
using System.IO;

namespace YuRis_Tool
{
    class YSCD
    {
        public record struct YSKeywordDefine(string Name, byte A, byte B, byte C, byte D)
        {
            public override string ToString()
            {
                return $"{Name}";
            }
        }

        public record struct YSCommandDefine(string Name, byte KeywordCount, List<YSKeywordDefine> Keywords)
        {
            public override string ToString()
            {
                return $"{Name}[{string.Join(", ", Keywords)}]";
            }
        }

        public record struct YSReservedVariableDefine(string Name, YSVR.VariableType Type, uint[] Dimensions);

        static List<YSCommandDefine> _commands = new List<YSCommandDefine>();
        static List<YSReservedVariableDefine> _vars = new List<YSReservedVariableDefine>();
        
        public static List<YSReservedVariableDefine> ReservedVars => _vars;

        public static void Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }

            static void Read(BinaryReader reader)
            {
                var magic = reader.ReadInt32();

                if (magic != 0x44435359)
                {
                    throw new Exception("Not a valid YSCD file.");
                }

                reader.ReadInt32(); // version
                var cmdCount = reader.ReadInt32(); // count
                reader.ReadInt32(); // zero

                for(int i = 0; i< cmdCount; i++)
                {
                    var name = reader.ReadAnsiString();
                    var kwCount = reader.ReadByte();

                    var kws = new List<YSKeywordDefine>();

                    for(int j = 0; j < kwCount; j++)
                    {
                        var kName = reader.ReadAnsiString();
                        var a = reader.ReadByte();
                        var b = reader.ReadByte();
                        var c = reader.ReadByte();
                        var d = reader.ReadByte();

                        kws.Add(new YSKeywordDefine(kName, a, b, c, d));
                    }

                    _commands.Add(new YSCommandDefine(name, kwCount, kws));
                }

                var varCount = reader.ReadInt32();
                reader.ReadInt32(); // zero

                for(int i = 0;i< varCount; i++)
                {
                    var name = reader.ReadAnsiString();
                    var type = (YSVR.VariableType)reader.ReadByte();
                    var dimCount = reader.ReadByte();

                    var dims = new List<uint>();
                    for(int j = 0;j < dimCount; j++)
                    {
                        dims.Add(reader.ReadUInt32());
                    }

                    _vars.Add(new YSReservedVariableDefine(name, type, dims.ToArray()));
                }

                //Console.WriteLine("");
            }
        }
    }
}
