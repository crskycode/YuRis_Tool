using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE0017
#pragma warning disable IDE0063

namespace YuRis_Tool
{
    class YSCM
    {
        public class CommandAction
        {
            public string Name;
            public int ArgType; // 0 - Int, 1 - String, 2 - Double
            public int ArgVaid;
        }

        public class Command
        {
            public string Name;
            public List<CommandAction> Actions;
        }

        List<Command> _commands;
        List<string> _errorMessage;
        byte[] _unknowBlock;

        public IReadOnlyList<Command> Commands
        {
            get => _commands;
        }

        public void Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }
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

            _commands = new List<Command>(count);

            for (var i = 0; i < count; i++)
            {
                var cmd = new Command();
                
                cmd.Name = reader.ReadAnsiString();

                //Debug.WriteLine($"{i:X2} {cmd.Name}");

                var actionCount = reader.ReadByte();

                cmd.Actions = new List<CommandAction>(actionCount);

                for (var j = 0; j < actionCount; j++)
                {
                    var act = new CommandAction();
                    act.Name = reader.ReadAnsiString();
                    act.ArgType = reader.ReadByte();
                    act.ArgVaid = reader.ReadByte();
                    cmd.Actions.Add(act);
                    //Debug.WriteLine($"    {act.Name} t={act.ArgType}");
                }

                _commands.Add(cmd);
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
