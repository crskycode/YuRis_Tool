using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE0017

namespace YuRis_Tool
{
    class YSTB
    {
        byte[] _scriptBuffer;

        int _sec0_addr; // command
        int _sec0_size;
        int _sec1_addr; // action
        int _sec1_size;
        int _sec2_addr; // argument
        int _sec2_size;
        int _sec3_addr; // line number
        int _sec3_size;

        List<Command> _commands;

        public IReadOnlyList<Command> Commands
        {
            get => _commands;
        }

        public byte[] Buffer
        {
            get => _scriptBuffer;
        }

        public void Load(string filePath, byte[] ybnKey)
        {
            var buffer = File.ReadAllBytes(filePath);

            if (BitConverter.ToInt32(buffer, 0) != 0x42545359)
            {
                throw new Exception("Not a valid YSTB file.");
            }

            _scriptBuffer = buffer;

            _sec0_addr = 0x20;
            _sec0_size = BitConverter.ToInt32(_scriptBuffer, 0x0C);

            _sec1_addr = _sec0_addr + _sec0_size;
            _sec1_size = BitConverter.ToInt32(_scriptBuffer, 0x10);

            _sec2_addr = _sec1_addr + _sec1_size;
            _sec2_size = BitConverter.ToInt32(_scriptBuffer, 0x14);

            _sec3_addr = _sec2_addr + _sec2_size;
            _sec3_size = BitConverter.ToInt32(_scriptBuffer, 0x18);

            if (ybnKey != null)
            {
                for (var i = 0; i < _sec0_size; i++)
                {
                    _scriptBuffer[_sec0_addr + i] ^= ybnKey[i & 3];
                }

                for (var i = 0; i < _sec1_size; i++)
                {
                    _scriptBuffer[_sec1_addr + i] ^= ybnKey[i & 3];
                }

                for (var i = 0; i < _sec2_size; i++)
                {
                    _scriptBuffer[_sec2_addr + i] ^= ybnKey[i & 3];
                }

                for (var i = 0; i < _sec3_size; i++)
                {
                    _scriptBuffer[_sec3_addr + i] ^= ybnKey[i & 3];
                }
            }

            ReadCommands();
            ReadCommandActions();
        }

        public void Save(string filePath)
        {
            if (_scriptBuffer != null)
            {
                File.WriteAllBytes(filePath, _scriptBuffer);
            }
        }

        void ReadCommands()
        {
            var stream = new MemoryStream(_scriptBuffer);
            var reader = new BinaryReader(stream);

            stream.Position = _sec0_addr;

            var count = BitConverter.ToInt32(_scriptBuffer, 8);

            _commands = new List<Command>(count);

            for (var i = 0; i < count; i++)
            {
                var cmd = new Command();

                cmd.Id = reader.ReadByte();
                cmd.ActionCount = reader.ReadByte();
                cmd.LabelId = reader.ReadUInt16();

                _commands.Add(cmd);
            }

            Debug.Assert(stream.Position == _sec0_addr + _sec0_size);
        }

        void ReadCommandActions()
        {
            var stream = new MemoryStream(_scriptBuffer);
            var reader = new BinaryReader(stream);

            stream.Position = _sec1_addr;

            foreach (var cmd in _commands)
            {
                cmd.Actions = new List<CommandAction>(cmd.ActionCount);

                if (cmd.ActionCount > 0)
                {
                    for (var i = 0; i < cmd.ActionCount; i++)
                    {
                        var act = new CommandAction();

                        act.Id = reader.ReadByte();
                        act.Flag = reader.ReadByte();
                        act.ArgLoadFn = reader.ReadByte();
                        act.ArgLoadOp = reader.ReadByte();
                        act.ArgSize = reader.ReadInt32();
                        act.ArgAddr = reader.ReadInt32();

                        act.ArgAddr += _sec2_addr;

                        cmd.Actions.Add(act);
                    }
                }
            }

            Debug.Assert(stream.Position == _sec1_addr + _sec1_size);
        }

        public class Command
        {
            public int Id;
            public int ActionCount;
            public int LabelId;
            public List<CommandAction> Actions;
        }

        public class CommandAction
        {
            public int Id;
            public int Flag;
            public int ArgLoadFn;
            public int ArgLoadOp;
            public int ArgSize;
            public int ArgAddr;
        }
    }
}
