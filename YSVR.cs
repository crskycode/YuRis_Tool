using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YuRis_Tool
{
    public class YSVR
    {
        public enum VariableScope : byte
        {
            None,
            Global,
            Script
        }

        public record struct Variable(VariableScope Scope, short ScriptIndex, short VariableId, byte Type, uint[] Dimensions, object Value);

        static List<Variable> _variables = new List<Variable>();

        public static void Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }
        }

        static void Read(BinaryReader reader)
        {
            var magic = reader.ReadInt32();

            if (magic != 0x52565359)
            {
                throw new Exception("Not a valid YSVR file.");
            }

            reader.ReadInt32(); // version

            var count = reader.ReadUInt16();


            for (int i = 0; i < count; i++)
            {
                var scope = (VariableScope)reader.ReadByte();
                var scriptIndex = reader.ReadInt16();
                var variableId = reader.ReadInt16();
                var type = reader.ReadByte();
                var dimensionCount = reader.ReadByte();
                var dimensions = new uint[dimensionCount];
                for (var o = 0; o < dimensionCount; o++)
                { 
                    dimensions[o] = reader.ReadUInt32();
                }

                object value = null;
                switch (type)
                {
                    case 1:
                        value = reader.ReadInt64();
                        break;
                    case 2:
                        value = reader.ReadDouble();
                        break;
                    case 3:
                    {
                        var offset = 0;
                        var length = reader.ReadUInt16();
                        if(length > 0)
                            value = Instruction.GetInstruction(0, reader.ReadBytes(length).AsSpan(), ref offset);
                        break;
                    }
                    default:
                        break;
                };
                _variables.Add(new Variable(scope, scriptIndex, variableId, type, dimensions, value));
            }
        }

        public static string GetDecompiledVarName(int scriptIndex, short variableId)
        {
            var variable = _variables.Where(v => v.ScriptIndex == scriptIndex && v.VariableId == variableId).FirstOrDefault();
            if(variable == default)
            {
                variable = _variables.Where(v => v.VariableId == variableId).FirstOrDefault();
            }

            if(variable != default)
            {
                return $"{(variable.Scope == VariableScope.Global ? "global" : "script")}.{variableId}";
            }
            else
            {
                //stack local var?
                return $"local.{variableId}";
            }
        }

        public static void WriteGlobalVarDecl(TextWriter writer)
        {
            foreach (var variable in _variables.Where(v => v.Scope == VariableScope.Global))
            {
                var v = $"{GetDecompiledVarName(0, variable.VariableId)}{(variable.Value == null ? "" : $"={variable.Value}")}";
                switch (variable.Type)
                {
                    case 1:
                    {
                        writer.WriteLine($"G_INT[{v}]");
                        break;
                    }
                    case 2:
                    {
                        writer.WriteLine($"G_FLT[{v}]");
                        break;
                    }
                    case 3:
                    {
                        writer.WriteLine($"G_STR[{v}]");
                        break;
                    }
                }
            }
        }
    }
}
