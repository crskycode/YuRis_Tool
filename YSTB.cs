using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable IDE0017

namespace YuRis_Tool
{
    //Progress: Working
    //YSTB(yst0000xx.ybn) is binary script file 
    class YSTB
    {
        int _scriptId;

        byte[] _scriptBuffer;

        int _command_addr;
        int _command_size;
        int _cmdExpr_addr;
        int _cmdExpr_size;
        int _cmdData_addr;
        int _cmdData_size;
        int _lineIdx_addr;
        int _lineIdx_size;

        List<Command> _commands;

        YSCM _yscm;
        YSLB _yslb;

        public YSTB(YSCM yscm, YSLB yslb)
        {
            _yslb = yslb;
            _yscm = yscm;
        }

        public IReadOnlyList<Command> Commands
        {
            get => _commands;
        }

        public byte[] Buffer
        {
            get => _scriptBuffer;
        }

        void Crypt(byte[] ybnKey)
        {
            for (var i = 0; i < _command_size; i++)
            {
                _scriptBuffer[_command_addr + i] ^= ybnKey[i & 3];
            }

            for (var i = 0; i < _cmdExpr_size; i++)
            {
                _scriptBuffer[_cmdExpr_addr + i] ^= ybnKey[i & 3];
            }

            for (var i = 0; i < _cmdData_size; i++)
            {
                _scriptBuffer[_cmdData_addr + i] ^= ybnKey[i & 3];
            }

            for (var i = 0; i < _lineIdx_size; i++)
            {
                _scriptBuffer[_lineIdx_addr + i] ^= ybnKey[i & 3];
            }
        }

        public bool Load(string filePath, int scriptId, byte[] ybnKey)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            _scriptId = scriptId;
            var buffer = File.ReadAllBytes(filePath);

            if (BitConverter.ToInt32(buffer, 0) != 0x42545359)
            {
                throw new Exception("Not a valid YSTB file.");
            }

            _scriptBuffer = buffer;

            _command_addr = 0x20;
            _command_size = BitConverter.ToInt32(_scriptBuffer, 0x0C);

            _cmdExpr_addr = _command_addr + _command_size;
            _cmdExpr_size = BitConverter.ToInt32(_scriptBuffer, 0x10);

            _cmdData_addr = _cmdExpr_addr + _cmdExpr_size;
            _cmdData_size = BitConverter.ToInt32(_scriptBuffer, 0x14);

            _lineIdx_addr = _cmdData_addr + _cmdData_size;
            _lineIdx_size = BitConverter.ToInt32(_scriptBuffer, 0x18);

            if (ybnKey != null)
                Crypt(ybnKey);

            ReadCommands();
            ReadCommandLineNumbers();
            ReadCommandExpressions();
            ReadCommandExpressionInstructions();
            return true;
        }

        public void Save(string filePath, byte[] ybnKey = null)
        {
            if (_scriptBuffer != null)
            {
                if (ybnKey != null)
                    Crypt(ybnKey);
                File.WriteAllBytes(filePath, _scriptBuffer);
            }
        }

        void ReadCommands()
        {
            var stream = new MemoryStream(_scriptBuffer);
            var reader = new BinaryReader(stream);

            stream.Position = _command_addr;

            var count = BitConverter.ToInt32(_scriptBuffer, 8);

            _commands = new List<Command>(count);

            for (var i = 0; i < count; i++)
            {
                var pos = reader.BaseStream.Position;
                var cmd = new Command(reader.ReadByte());
                cmd.offset = pos;
                cmd.ExprCount = reader.ReadByte();
                cmd.LabelId = reader.ReadUInt16();

                _commands.Add(cmd);
            }

            Debug.Assert(stream.Position == _command_addr + _command_size);
        }

        void ReadCommandExpressions()
        {
            var stream = new MemoryStream(_scriptBuffer);
            var reader = new BinaryReader(stream);

            stream.Position = _cmdExpr_addr;

            foreach (var statement in _commands)
            {
                statement.Expressions = new List<CommandExpression>(statement.ExprCount);

                if (statement.ExprCount > 0)
                {
                    for (var i = 0; i < statement.ExprCount; i++)
                    {
                        var expr = new CommandExpression();

                        expr.Id = reader.ReadByte();
                        expr.Flag = reader.ReadByte();
                        expr.ArgLoadFn = reader.ReadByte();
                        expr.ArgLoadOp = reader.ReadByte();
                        expr.InstructionSize = reader.ReadInt32();
                        expr.InstructionOffset = reader.ReadInt32();

                        statement.Expressions.Add(expr);
                    }
                }
            }

            Debug.Assert(stream.Position == _cmdExpr_addr + _cmdExpr_size);
        }

        void ReadCommandLineNumbers()
        {
            var stream = new MemoryStream(_scriptBuffer);
            var reader = new BinaryReader(stream);

            stream.Position = _lineIdx_addr;

            foreach (var cmd in _commands)
            {
                cmd.LineNumber = reader.ReadInt32();
            }

            Debug.Assert(stream.Position == _lineIdx_addr + _lineIdx_size);
        }

        void ReadCommandExpressionInstructions()
        {
            //Expression data is stored in a "compressed" form.
            //This is because different expressions may have a part of the same byte sequence.
            //In this case, the shorter expression will be directly pointed to the longer expression in order to save space.
            /*Example:
             *  Expr.A : 0000112222256410
             *  Expr.B :         22256410
             * 
             * argData : 0000112222256410
             *           ^       ^
             *           |       Expr.B(Offset:8, Len:8)
             *           Expr.A(Offset:0, Len:16)
             */
            var span = _scriptBuffer.AsSpan(_cmdData_addr, _cmdData_size);

            for (int i = 0; i < _commands.Count; i++)
            {
                if (_commands[i].ExprCount == 0)
                {
                    continue;
                }

                for (int o = 0; o < _commands[i].Expressions.Count; o++)
                {
                    var expr = _commands[i].Expressions;
                    YSCM.ExpressionInfo info = _yscm.GetExprInfo(Convert.ToInt32(_commands[i].Id), expr[o].Id);

                    bool stringExpr = true;
                    //preprocess
                    switch (_commands[i].Id.ToString().ToUpper())
                    {
                        case "IF":
                        case "ELSE":
                        {
                            var conditionExpr = new ExprInstructionSet(_scriptId);
                            conditionExpr.GetInstructions(span.Slice(expr[o].InstructionOffset, expr[o].InstructionSize));
                            expr[o].ExprInsts = conditionExpr;

                            expr.RemoveRange(1, 2);
                            //expr[o + 1].Arg = $"&({expr[o + 1].InstructionOffset})";//branch1 dst
                            //expr[o + 2].Arg = $"&({expr[o + 2].InstructionOffset})";//branch2 dst
                            o += 2;
                            continue;
                        }
                        case "S_INT":
                        case "S_STR":
                        case "S_FLT":
                        case "G_INT":
                        case "G_STR":
                        case "G_FLT":
                        case "F_INT":
                        case "F_STR":
                        case "F_FLT":
                        case "LET":
                        {
                            var dst = new ExprInstructionSet(_scriptId);
                            var src = new ExprInstructionSet(_scriptId);
                            dst.GetInstructions(span.Slice(expr[o].InstructionOffset, expr[o].InstructionSize));
                            src.GetInstructions(span.Slice(expr[o + 1].InstructionOffset, expr[o + 1].InstructionSize));
                            expr[o].ExprInsts = dst;
                            expr[o + 1].ExprInsts = src;
                            o++;
                            continue;
                        }
                        //Local vars
                        case "STR":
                        case "INT":
                        case "FLT":
                        {
                            var dst = new ExprInstructionSet(_scriptId);
                            var src = new ExprInstructionSet(_scriptId);
                            dst.GetInstructions(span.Slice(expr[o].InstructionOffset, expr[o].InstructionSize));
                            src.GetInstructions(span.Slice(expr[o + 1].InstructionOffset, expr[o + 1].InstructionSize));
                            expr[o].ExprInsts = dst;
                            expr[o + 1].ExprInsts = src;
                            o++;

                            //declare the local array type
                            switch(dst._inst)
                            {
                                case ArrayAccess aa:
                                {
                                    aa.Variable._varInfo.Type = _commands[i].Id.ToString().ToUpper() switch
                                    {
                                        "STR" => 3,
                                        "FLT" => 2,
                                        _ => 1
                                    };
                                    var dims = new List<uint>();
                                    foreach (var d in aa.Indices)
                                    {
                                        switch (d)
                                        {
                                            case ByteLiteral b:
                                                dims.Add(Convert.ToUInt32(b.Value)); break;
                                            case ShortLiteral b:
                                                dims.Add(Convert.ToUInt32(b.Value)); break;
                                            case IntLiteral b:
                                                dims.Add(Convert.ToUInt32(b.Value)); break;
                                            case LongLiteral b:
                                                dims.Add(Convert.ToUInt32(b.Value)); break;
                                            case DecimalLiteral b:
                                                dims.Add(Convert.ToUInt32(b.Value)); break;
                                            default:
                                                dims.Add(0); break;//not supporting dynamic array
                                        }
                                    }
                                    aa.Variable._varInfo.Dimensions = dims.ToArray();
                                    break;
                                }
                                case VariableAccess va:
                                {
                                    va._varInfo.Type = _commands[i].Id.ToString().ToUpper() switch
                                    {
                                        "STR" => 3,
                                        "FLT" => 2,
                                        _ => 1
                                    };
                                    break;
                                }
                                case VariableRef vr:
                                {
                                    vr._varInfo.Type = _commands[i].Id.ToString().ToUpper() switch
                                    {
                                        "STR" => 3,
                                        "FLT" => 2,
                                        _ => 1
                                    };
                                    break;
                                }
                            }

                            continue;
                        }
                        case "RETURNCODE":
                        {
                            expr[o].ExprInsts = new ExprInstructionSet(_scriptId, new IntLiteral(expr[o].Id));
                            o++;
                            continue;
                        }
                        case "LOOP":
                        {
                            expr.RemoveAt(1);//modified var list
                            stringExpr = false;
                            break;
                        }
                        case "_":
                        {
                            stringExpr = false;
                            break;
                        }
                    }

                    if (info != null)
                    {
                        var statement = new AssignExprInstSet(_scriptId, info, expr[o].GetLoadOp());
                        expr[o].ExprInsts = statement;
                        if (expr[o].InstructionSize == 0)
                        {
                            continue;
                        }
                        var remainingSize = span.Length - expr[o].InstructionOffset;
                        var instructionSize = expr[o].InstructionSize > remainingSize ? remainingSize : expr[o].InstructionSize;
                        if (instructionSize != expr[o].InstructionSize)
                        {
                            Console.WriteLine($"Warning: Expr({expr[0].InstructionOffset})'s length dosen't match: ({instructionSize}/{expr[o].InstructionSize}). Turncating...");
                        }
                        statement.GetInstructions(span.Slice(expr[o].InstructionOffset, instructionSize), stringExpr && info.ResultType == YSCM.ExprEvalResult.Raw);
                    }
                    else
                    {
                        if (expr[o].InstructionSize == 0)
                        {
                            continue;
                        }
                        throw new Exception("Unknown abnormal command with instruction(s).");
                    }
                }
            }
        }

        public class Command
        {
            public long offset;
            public Enum Id;//int
            public int ExprCount;
            public int LabelId;
            public int LineNumber;
            public List<CommandExpression> Expressions;

            public Command(int id)
            {
                Id = CommandIDGenerator.GetID(id);
            }

            public override string ToString()
            {
                switch (Id.ToString())
                {
                    //Ignored
                    case "IFBLEND":
                    case "RETURNCODE":
                        return "";
                    case "LET":
                    {
                        return $"{Expressions[0]}{Expressions[0].GetLoadOp()}{Expressions[1]}";
                    }
                    case "STR":
                    case "INT":
                    case "FLT":
                    case "S_STR":
                    case "S_INT":
                    case "S_FLT":
                    case "G_STR":
                    case "G_INT":
                    case "G_FLT":
                        return $"{Id}[{Expressions[0]}{Expressions[0].GetLoadOp()}{Expressions[1]}]";
                    case "WORD":
                        return $"\n{Expressions[0].ToString().Trim('\"')}";
                    case "VARACT":
                    case "VARINFO":
                    {
/*                        var flag = Expressions
                            .ToList()
                            .Select(x => x.ExprInsts)
                            .Where(y => y is AssignExprInstSet)
                            .Any(z => ((AssignExprInstSet)z).exprInfo.Name.ToUpper() == "DIMSIZE");*/
                        if (false)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append($"{Id.ToString()}[");
                            foreach (var expr in Expressions)
                            {
                                if (expr.ExprInsts is AssignExprInstSet aes)
                                {
                                    var s = $"{expr}";
                                    bool flag = !s.Contains('(');
                                    if (aes.exprInfo.Keyword.ToUpper() == "SET")
                                    {
                                        sb.Append(s);
                                        if (flag && aes._inst is VariableAccess va && va._varInfo.Dimensions.Length > 0)
                                        {
                                            sb.Append("() ");
                                        }
                                        else if (flag && aes._inst is VariableRef vr && vr._varInfo.Dimensions.Length > 0)
                                        {
                                            sb.Append("() ");
                                        }
                                        else
                                        {
                                            sb.Append(' ');
                                        }
                                        continue;
                                    }
                                    
                                }
                                sb.Append($"{expr} ");
                            }
                            return $"{sb.ToString().TrimEnd()}]";
                        }
                        else
                        {
                            return $"{Id}[{string.Join(" ", Expressions)}]";
                        }
                    }
                    default:
                        return $"{Id}[{string.Join(" ", Expressions)}]";
                }
            }
        }

        public class CommandExpression
        {
            public int Id;
            public int Flag;
            public int ArgLoadFn;
            public int ArgLoadOp;
            public int InstructionSize;
            public int InstructionOffset;
            public ExprInstructionSet ExprInsts;

            public string GetLoadOp()
            {
                var loadOp = ArgLoadOp switch
                {
                    0 => "=",
                    1 => "+=",
                    2 => "-=",
                    3 => "*=",
                    4 => "/=",
                    5 => "%=",
                    6 => "&=",
                    7 => "|=",
                    8 => "^=",
                    _ => "",
                };
                return loadOp;
            }

            public override string ToString()
            {
                return $"{ExprInsts}";
            }
        }
    }
}
