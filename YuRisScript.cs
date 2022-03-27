using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuRis_Tool
{
    class YuRisScript
    {
        string _dirPath;
        YSCM _yscm;
        YSLB _yslb;
        byte[] _ybnKey;

        public void Init(string dirPath, byte[] ybnKey)
        {
            _dirPath = dirPath;

            _yscm = new YSCM();
            _yscm.Load(Path.Combine(dirPath, "ysc.ybn"));

            _yslb = new YSLB();
            _yslb.Load(Path.Combine(dirPath, "ysl.ybn"));

            _ybnKey = ybnKey;
        }

        public void Parse(int scriptId)
        {
            var ystb = new YSTB();
            ystb.Load(Path.Combine(_dirPath, $"yst{scriptId:D5}.ybn"), _ybnKey);

            var stream = new MemoryStream(ystb.Buffer);
            var reader = new BinaryReader(stream);

            var commands = ystb.Commands;

            for (var i = 0; i < commands.Count; i++)
            {
                var label = _yslb.Find(scriptId, i);

                if (label != null)
                {
                    Debug.WriteLine($"{i:D8} #{label.Name}");
                }

                var cmd = commands[i];

                Debug.WriteLine($"{i:D8} {_yscm.Commands[cmd.Id].Name}");

                switch (cmd.Id)
                {
                    case 0: // sub_42315C
                        break;
                    case 1: // sub_423940
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    // sub_43D428 ELSE
                    case 0x0B:
                    {
                        break;
                    }
                    // sub_43DB20 END
                    case 0x0D:
                    {
                        break;
                    }
                    case 0x2B: // sub_4429A0 GOSUB
                    {
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    }
                    case 0x2C: // sub_4432CC IF
                    {
                        var condExpr = new Expression();
                        condExpr.Execute(reader, cmd.Actions[0].ArgAddr, cmd.Actions[0].ArgSize);
                        break;
                    }
                    // sub_4433C4 IFBLEND
                    case 0x2D:
                    {
                        break;
                    }
                    case 0x30: // sub_443514 IFEND
                    {
                        break;
                    }
                    case 0x32: // sub_443618
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    case 0x35: // sub_4438E8 LET
                    {
                        var targetExpr = new Expression();
                        targetExpr.Execute(reader, cmd.Actions[0].ArgAddr, cmd.Actions[0].ArgSize);

                        var sourceExpr = new Expression();
                        sourceExpr.Execute(reader, cmd.Actions[1].ArgAddr, cmd.Actions[1].ArgSize);

                        break;
                    }
                    // sub_445AE0 LOOP
                    case 0x37:
                    {
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    }
                    // sub_445C7C LOOPBREAK
                    case 0x38:
                    {
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    }
                    // sub_445D34 LOOPCONTINUE
                    case 0x39:
                    {
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    }
                    // sub_445DC8 LOOPEND
                    case 0x3A:
                    {
                        break;
                    }
                    // sub_44B4EC RETURN
                    case 0x4F:
                    {
                        break;
                    }
                    case 0x52: // sub_42315C
                    case 0x53: // sub_42315C
                    case 0x54: // sub_42315C
                        break;
                    case 0x5C: // sub_44F8D8
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    // sub_455174 VARINFO
                    case 0x67:
                    {
                        PrepareCommand(ystb, reader, cmd);
                        break;
                    }
                    default:
                        throw new Exception("Unknow command.");
                }
            }
        }

        void PrepareCommand(YSTB ystb, BinaryReader reader, YSTB.Command cmd)
        {
            for (var i = 0; i < cmd.ActionCount; i++)
            {
                var act = cmd.Actions[i];

                Debug.WriteLine($"         .{_yscm.Commands[cmd.Id].Actions[act.Id].Name}");

                var targetExpr = new Expression();
                targetExpr.Execute(reader, cmd.Actions[i].ArgAddr, cmd.Actions[i].ArgSize);

            }
        }


    }
}
