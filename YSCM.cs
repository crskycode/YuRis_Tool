using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#pragma warning disable IDE0017
#pragma warning disable IDE0063

namespace YuRis_Tool
{
    class YSCM
    {
        public enum ExprEvalResult : byte
        {
            Integer,
            String,
            Decimal,
            Raw
        }

        public enum ResultValidateMode : byte
        {
            ValidateMinimum,
        }

        public class ExpressionInfo
        {
            public string Keyword;
            public ExprEvalResult ResultType;
            public ResultValidateMode ValidateMode;

            public override string ToString()
            {
                return $"Arg({Keyword}), Type:({ResultType})";
            }
        }

        public ExpressionInfo GetExprInfo(int commandId, int exprId)
        {
            var cmd = _commandsInfo[commandId];
            if (cmd.ArgExprs.Count <= exprId)
            {
                return null;
            }
            return cmd.ArgExprs[exprId];
        }

        public class CommandInfo
        {
            public string Name;
            public List<ExpressionInfo> ArgExprs;

            public override string ToString()
            {
                return $"Cmd({Name}), ArgExprs:({ArgExprs.Count})";
            }
        }

        List<CommandInfo> _commandsInfo;
        List<string> _errorMessage;
        byte[] _unknowBlock;

        public IReadOnlyList<CommandInfo> CommandsInfo
        {
            get => _commandsInfo;
        }

        public void Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }
            CommandIDGenerator.GenerateType(this);
        }

        void Read(BinaryReader reader)
        {
            var magic = reader.ReadInt32();

            if (magic != 0x4D435359)
            {
                throw new Exception("Not a valid YSCM file.");
            }

            reader.ReadInt32(); // version

            var count = reader.ReadInt32();

            reader.ReadInt32(); // zero

            _commandsInfo = new List<CommandInfo>(count);

            for (var i = 0; i < count; i++)
            {
                var cmd = new CommandInfo();
                
                cmd.Name = reader.ReadAnsiString();

                //Debug.WriteLine($"{i:X2} {cmd.Name}");

                var actionCount = reader.ReadByte();

                cmd.ArgExprs = new List<ExpressionInfo>(actionCount);

                for (var j = 0; j < actionCount; j++)
                {
                    var act = new ExpressionInfo();
                    act.Keyword = reader.ReadAnsiString();
                    act.ResultType = (ExprEvalResult)reader.ReadByte();
                    act.ValidateMode = (ResultValidateMode)reader.ReadByte();
                    cmd.ArgExprs.Add(act);
                }

                _commandsInfo.Add(cmd);
            }

            _errorMessage = new List<string>(37);

            for (var i = 0; i < 37; i++)
            {
                var s = reader.ReadAnsiString();
                _errorMessage.Add(s);
            }

            _unknowBlock = reader.ReadBytes(256);

            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        }
    }
}
