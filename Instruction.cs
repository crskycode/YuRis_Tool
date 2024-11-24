
using System.Diagnostics;
using System;

namespace YuRis_Tool
{
    public abstract class Instruction
    {
        public static Instruction GetInstruction(int scriptId, Span<byte> data, ref int offset)
        {
            var id = (Opcode)data[offset];
            var operandLength = BitConverter.ToInt16(data.Slice(offset + 1, sizeof(ushort)));
            var operandData = data.Slice(offset + 3, operandLength);

            offset += 3 + operandLength;

            switch (id)
            {
                case Opcode.Add:
                case Opcode.Subtract:
                case Opcode.Multiply:
                case Opcode.Divide:
                case Opcode.Modulo:
                    return new ArithmeticOperator(null, null, (ArithmeticOperator.Type)id);
                case Opcode.LogicalAnd:
                case Opcode.LogicalOr:
                case Opcode.BitwiseXor:
                case Opcode.BitwiseAnd:
                case Opcode.BitwiseOr:
                    return new LogicalOperator(null, null, (LogicalOperator.Type)id);
                case Opcode.Equal:
                case Opcode.NotEqual:
                case Opcode.Less:
                case Opcode.Greater:
                case Opcode.LessEqual:
                case Opcode.GreaterEqual:
                    return new RelationalOperator(null, null, (RelationalOperator.Type)id);
                case Opcode.Negate:
                case Opcode.ToString:
                case Opcode.ToNumber:
                    return new UnaryOperator(null, (UnaryOperator.Type)id);

                case Opcode.ArrayAccess:
                    return new ArrayAccess(null, null);

                case Opcode.PushByte:
                    Trace.Assert(operandLength == 1);
                    return new ByteLiteral((sbyte)operandData[0]);
                case Opcode.PushDouble:
                    Trace.Assert(operandLength == 8);
                    return new DecimalLiteral(BitConverter.ToDouble(operandData));
                case Opcode.PushShort:
                    Trace.Assert(operandLength == 2);
                    return new ShortLiteral(BitConverter.ToInt16(operandData));
                case Opcode.PushInt:
                    Trace.Assert(operandLength == 4);
                    return new IntLiteral(BitConverter.ToInt32(operandData));
                case Opcode.PushLong:
                    Trace.Assert(operandLength == 8);
                    return new LongLiteral(BitConverter.ToInt64(operandData));
                case Opcode.PushString:
                    return new StringLiteral(Extensions.DefaultEncoding.GetString(operandData));


                case Opcode.LoadVariable:
                    Trace.Assert(operandLength == 3);
                    return new VariableAccess(scriptId, (VariableLoadMode)operandData[0], BitConverter.ToInt16(operandData[1..]));
                case Opcode.LoadVariableRef:
                    Trace.Assert(operandLength == 3);
                    return new VariableRef(scriptId, (VariableLoadMode)operandData[0], BitConverter.ToInt16(operandData[1..]), false);
                case Opcode.LoadVariableRef2:
                    Trace.Assert(operandLength == 3);
                    return new VariableRef(scriptId, (VariableLoadMode)operandData[0], BitConverter.ToInt16(operandData[1..]), true);

                case Opcode.Nop:
                    return new Nop();
                default:
                    throw new Exception($"Invalid opcode:{id:X2}");
            }
        }
    }

    public enum Opcode : byte
    {
        RawOperand = (byte)' ',//20
        NotEqual = (byte)'!',//21
        //"
        //# VariableLoadMode.Pound
        //$
        Modulo = (byte)'%',//25
        LogicalAnd = (byte)'&',//26
        //'
        //(
        ArrayAccess = (byte)')',//29
        Multiply = (byte)'*',//2A
        Add = (byte)'+',//2B
        Nop = (byte)',',//2C
        Subtract = (byte)'-',//2D
        //.
        Divide = (byte)'/',//2F
        //0~9
        //:
        //;
        Less = (byte)'<',//3C
        Equal = (byte)'=',//3D
        Greater = (byte)'>',//3E
        //?
        //@ VariableLoadMode.At
        BitwiseAnd = (byte)'A',//41
        PushByte = (byte)'B',//42
        //C
        //D
        //E
        PushDouble = (byte)'F',//46
        //G
        LoadVariable = (byte)'H',//48
        PushInt = (byte)'I',//49
        //J
        //K
        PushLong = (byte)'L',//4C
        PushString = (byte)'M',//4D
        //N
        BitwiseOr = (byte)'O',//4F
        //P
        //Q
        Negate = (byte)'R',//52
        LessEqual = (byte)'S',//53
        //T
        //U
        LoadVariableRef = (byte)'V',//56
        PushShort = (byte)'W',//57
        //X
        //Y
        GreaterEqual = (byte)'Z',//5A
        //[
        //\
        //]
        BitwiseXor = (byte)'^',//5E
        //_
        //` VariableLoadMode.Badtick
        //a~h
        ToNumber = (byte)'i',//69
        //j~r
        ToString = (byte)'s',//73
        //t
        //u
        LoadVariableRef2 = (byte)'v',//76
        //w
        //x
        //y
        //z
        //{
        LogicalOr = (byte)'|',//7C
        //}
        //~
    }

    public enum VariableLoadMode : byte
    {
        Backtick = (byte)'`',
        Pound = (byte)'#',
        At = (byte)'@',
    }

    public abstract class BinaryOperator<T> : Instruction where T : Enum
    {
        public Instruction Left;
        public Instruction Right;
        public T Operator;

        public BinaryOperator(Instruction left, Instruction right, T op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
        public abstract string GetOperator(T type);

        public override string ToString()
        {
            return $"{Left}{GetOperator(Operator)}{Right}";
        }
    }

    public class ArithmeticOperator : BinaryOperator<ArithmeticOperator.Type>
    {
        public enum Type : byte
        {
            Add = Opcode.Add,
            Subtract = Opcode.Subtract,
            Multiply = Opcode.Multiply,
            Divide = Opcode.Divide,
            Modulo = Opcode.Modulo,
        }

        public bool Negate = false;

        public ArithmeticOperator(Instruction left, Instruction right, Type op) : base(left,right, op) { }

        public override string GetOperator(Type type)
        {
            switch (type)
            {
                case Type.Add:
                    return "+";
                case Type.Subtract:
                    return "-";
                case Type.Multiply:
                    return "*";
                case Type.Divide:
                    return "/";
                case Type.Modulo:
                    return "%";
                default:
                    return type.ToString();
            }
        }

        public override string ToString()
        {
            if (Negate)
                return $"-({base.ToString()})";
            return base.ToString();
        }
    };

    public class RelationalOperator : BinaryOperator<RelationalOperator.Type>
    {
        public enum Type : byte
        {
            Equal = Opcode.Equal,
            NotEqual = Opcode.NotEqual,
            Less = Opcode.Less,
            Greater = Opcode.Greater,
            LessEqual = Opcode.LessEqual,
            GreaterEqual = Opcode.GreaterEqual,
        }

        public RelationalOperator(Instruction left, Instruction right, Type op) : base(left, right, op) { }

        public override string GetOperator(Type type)
        {
            switch (type)
            {
                case Type.Equal:
                    return "==";
                case Type.NotEqual:
                    return "!=";
                case Type.Less:
                    return "<";
                case Type.Greater:
                    return ">";
                case Type.LessEqual:
                    return "<=";
                case Type.GreaterEqual:
                    return ">=";
                default:
                    return type.ToString();
            }
        }
    };

    public class LogicalOperator : BinaryOperator<LogicalOperator.Type>
    {
        public enum Type : byte
        {
            LogicalAnd = Opcode.LogicalAnd,
            LogicalOr = Opcode.LogicalOr,
            BitwiseXor = Opcode.BitwiseXor,
            BitwiseAnd = Opcode.BitwiseAnd,
            BitwiseOr = Opcode.BitwiseOr,
        }

        public LogicalOperator(Instruction left, Instruction right, Type op) : base(left, right, op) { }

        public override string GetOperator(Type type)
        {
            switch(type)
            {
                case Type.LogicalAnd:
                    return "&&";
                case Type.LogicalOr:
                    return "||";
                case Type.BitwiseXor:
                    return "^";
                case Type.BitwiseAnd:
                    return "&";
                case Type.BitwiseOr:
                    return "|";
                default:
                    return type.ToString();
            }
        }
    };

    public abstract class Literal<T> : Instruction
    {
        public T Value;
        public override string ToString()
        {
            if (Value is string s)
                return s;
            return Value.ToString();
        }
    }

    public class UnaryOperator : Instruction
    {
        public enum Type : byte
        {
            Negate = Opcode.Negate,
            String = Opcode.ToString,
            Number = Opcode.ToNumber,
        }

        public Instruction Operand;
        public Type Operator;
        public UnaryOperator(Instruction operand, Type op)
        {
            Operand = operand;
            Operator = op;
        }

        public override string ToString()
        {
            switch(Operator)
            {
                case Type.String:
                {
                    return $"$({Operand})";
                }
                case Type.Number:
                {
                    return $"(Number)({Operand})";
                }
            }
            return $"({Operator}){Operand}";
        }
    }

    public class RawStringLiteral : Instruction
    {
        public string Value;
        public RawStringLiteral(string value)
        {
            Value = value;
        }
        public override string ToString()
        {
            return Value;
        }
    }

    public class ByteLiteral : Literal<sbyte>
    {
        public ByteLiteral(sbyte value)
        {
            Value = value;
        }
    }

    public class ShortLiteral : Literal<short>
    {
        public ShortLiteral(short value)
        {
            Value = value;
        }
    }

    public class IntLiteral : Literal<int>
    {
        public IntLiteral(int value)
        {
            Value = value;
        }
    }

    public class LongLiteral : Literal<long>
    {
        public LongLiteral(long value)
        {
            Value = value;
        }
    }

    public class DecimalLiteral : Literal<double>
    {
        public DecimalLiteral(double value)
        {
            Value = value;
        }
    }

    public class StringLiteral : Literal<string>
    {
        public StringLiteral(string value)
        {
            Value = value;
        }
    }

    public abstract class AccessInstruction : Instruction
    {
        public bool Negate = false;
    }

    public class VariableAccess : AccessInstruction
    {
        int _scriptIndex;
        public VariableLoadMode Mode;
        public short Index;
        public VariableAccess(int scriptIndex, VariableLoadMode mode, short index)
        {
            _scriptIndex = scriptIndex;
            Mode = mode;
            Index = index;
        }

        public override string ToString()
        {
            return $"{(Negate ? "-" : "")}{(char)Mode}{YSVR.GetDecompiledVarName(_scriptIndex, Index)}";
        }
    }

    public class VariableRef: Instruction
    {
        int _scriptIndex;
        public VariableLoadMode Mode;
        public short Index;
        public bool SmallV;

        public VariableRef(int scriptIndex, VariableLoadMode mode, short index, bool smallV)
        {
            _scriptIndex = scriptIndex;
            Mode = mode;
            Index = index;
            SmallV = smallV;
        }

        public override string ToString()
        {
            return $"{(char)Mode}{YSVR.GetDecompiledVarName(_scriptIndex, Index)}";
        }
    }

    public class ArrayAccess : AccessInstruction
    {
        public VariableRef Variable;
        public Instruction[] Indices;
        public ArrayAccess(VariableRef variable, Instruction[] indices)
        {
            Variable = variable;
            Indices = indices;
        }

        public override string ToString()
        {
            return $"{(Negate ? "-" : "")}{Variable}({string.Join<Instruction>(",", Indices)})";
        }
    }

    public class Nop : Instruction { }
}
