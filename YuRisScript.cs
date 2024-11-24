using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YuRis_Tool
{
    class YuRisScript
    {
        string _dirPath;
        YSCM _yscm;
        YSLB _yslb;
        YSTL _ystl;
        byte[] _ybnKey;

        public void Init(string dirPath, byte[] ybnKey)
        {
            _dirPath = dirPath;

            _yscm = new YSCM();
            _yscm.Load(Path.Combine(dirPath, "ysc.ybn"));

            _yslb = new YSLB();
            _yslb.Load(Path.Combine(dirPath, "ysl.ybn"));

            _ystl = new YSTL();
            _ystl.Load(Path.Combine(dirPath, "yst_list.ybn"));

            YSVR.Load(Path.Combine(dirPath, "ysv.ybn"));

            _ybnKey = ybnKey;
        }

        public bool Decompile(int scriptIndex, TextWriter outputStream = null)
        {
            Console.Write($"Decompiling yst{scriptIndex:D5}.ybn ...");
            var ystb = new YSTB(_yscm, _yslb);
            if (!ystb.Load(Path.Combine(_dirPath, $"yst{scriptIndex:D5}.ybn"), scriptIndex, _ybnKey))
                return false;


            outputStream ??= Console.Out;

            var commands = ystb.Commands;
            var nestDepth = 0;

            for (var i = 0; i < commands.Count; i++)
            {
                var label = _yslb.Find(scriptIndex, i);

                if (label != null)
                {
                    outputStream.WriteLine($"#={label.Name}");
                }

                var cmd = commands[i];
                switch (cmd.Id.ToString())
                {
                    case "IF":
                    case "LOOP":
                    {
                        outputStream.Write("".PadLeft(nestDepth * 4, ' '));
                        nestDepth++;
                        break;
                    }
                    case "ELSE":
                    {
                        outputStream.Write("".PadLeft((nestDepth - 1) * 4, ' '));
                        break;
                    }
                    case "IFEND":
                    case "LOOPEND":
                    {
                        nestDepth--;
                        outputStream.Write("".PadLeft(nestDepth * 4, ' '));
                        break;
                    }
                    default:
                    {
                        outputStream.Write("".PadLeft(nestDepth * 4, ' '));
                        break;
                    }
                }
                outputStream.WriteLine(cmd);
            }
            return true;
        }

        public void DecompileProject()
        {
            List<string> sourcePaths = new List<string>();

            foreach(var script in _ystl)
            {
                var sourcePath = Path.Combine(_dirPath, script.Source);
                Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
                sourcePaths.Add(sourcePath);

                using var textWriter = new StringWriter();
                if(Decompile(script.Id, textWriter))
                {
                    File.WriteAllText(sourcePath, textWriter.ToString());
                    Console.Write($" -> {sourcePath}");
                }
                else
                {
                    Console.Write($" -> Failed. No such file.");
                }
                Console.WriteLine("");
            }

            string[] longestCommonPathComponents = sourcePaths
                .Select(path => path.Split(Path.DirectorySeparatorChar))
                .Transpose()
                .Select(parts => parts.Distinct(StringComparer.OrdinalIgnoreCase))
                .TakeWhile(distinct => distinct.Count() == 1)
                .Select(distinct => distinct.First())
                .Append("globalVarDecl.txt")
                .ToArray();

            using var globalVarWriter = new StringWriter();
            YSVR.WriteGlobalVarDecl(globalVarWriter);
            File.WriteAllText(Path.Combine(longestCommonPathComponents), globalVarWriter.ToString());
        }
    }
}
