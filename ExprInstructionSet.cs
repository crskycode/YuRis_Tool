using System;
using System.Collections.Generic;
using System.Linq;

namespace YuRis_Tool
{
    class ExprInstructionSet
    {
        int _scriptId = -1;
        public List<Instruction> _insts = new List<Instruction>();
        public Instruction _inst;

        public ExprInstructionSet(int scriptId)
        {
            _scriptId = scriptId;
        }

        public ExprInstructionSet(int scriptId, Instruction instruction)
        {
            _scriptId = scriptId;
            _inst = instruction;
        }

        public virtual void GetInstructions(Span<byte> data, bool rawExpr = false)
        {
            int offset = 0;

            while (offset < data.Length)
            {
                _insts.Add(Instruction.GetInstruction(_scriptId, data, ref offset));
            }
            Evaluate();
        }

        public void Evaluate()
        {
            var stack = new Stack<Instruction>();
            foreach (var t in _insts)
            {
                switch (t)
                {
                    case ArithmeticOperator ao:
                    {
                        ao.Right = stack.Pop();
                        ao.Left = stack.Pop();
                        stack.Push(ao);
                        break;
                    }
                    case RelationalOperator ro:
                    {
                        ro.Right = stack.Pop();
                        ro.Left = stack.Pop();
                        stack.Push(ro);
                        break;
                    }
                    case LogicalOperator lo:
                    {
                        lo.Right = stack.Pop();
                        lo.Left = stack.Pop();
                        stack.Push(lo);
                        break;
                    }
                    case UnaryOperator unaryOp:
                    {
                        if (unaryOp.Operator == UnaryOperator.Type.Negate)
                        {
                            switch (stack.Peek())
                            {
                                case ByteLiteral bl:
                                {
                                    bl.Value = Convert.ToSByte(-bl.Value);
                                    break;
                                }
                                case ShortLiteral sl:
                                {
                                    sl.Value = Convert.ToInt16(-sl.Value);
                                    break;
                                }
                                case IntLiteral il:
                                {
                                    il.Value = -il.Value;
                                    break;
                                }
                                case LongLiteral ll:
                                {
                                    ll.Value = -ll.Value;
                                    break;
                                }
                                case ArrayAccess aa:
                                {
                                    aa.Negate ^= true;
                                    break;
                                }
                                case VariableAccess va:
                                {
                                    va.Negate ^= true;
                                    break;
                                }
                                case ArithmeticOperator ao:
                                {
                                    ao.Negate ^= true;
                                    break;
                                }
                                default:
                                {
                                    throw new InvalidOperationException($"Selected object ({stack.Peek()}) does not support negate operator!");
                                }
                            }
                        }
                        else
                        {
                            unaryOp.Operand = stack.Pop();
                            stack.Push(unaryOp);
                        }
                        break;
                    }
                    case ArrayAccess aa:
                    {
                        var indices = new List<Instruction>();
                        var top = stack.Pop();
                        while (top is not VariableRef)
                        {
                            indices.Add(top);
                            top = stack.Pop();
                        }
                        indices.Reverse();
                        stack.Push(new ArrayAccess((VariableRef)top, indices.ToArray()));
                        break;
                    }
                    case Nop:
                        continue;
                    default:
                    {
                        stack.Push(t);
                        break;
                    }
                }
            }

            _inst = stack.Single();
        }

        public override string ToString()
        {
            return $"{_inst}";
        }
    }

    class AssignExprInstSet : ExprInstructionSet
    {
        public YSCM.ExpressionInfo exprInfo;
        public AssignExprInstSet(int scriptId, YSCM.ExpressionInfo info) : base(scriptId)
        {
            exprInfo = info;
        }

        public override void GetInstructions(Span<byte> data, bool rawExpr)
        {
            if (rawExpr)
            {
                _insts.Add(new RawStringLiteral(Extensions.DefaultEncoding.GetString(data)));//FIXME
                Evaluate();
                return;
            }
            base.GetInstructions(data);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(exprInfo.Name))
            {
                return $"{_inst}";
            }
            return $"{exprInfo.Name}={_inst}";
        }
    }
}
