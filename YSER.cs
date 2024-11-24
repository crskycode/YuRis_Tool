using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable IDE0017
#pragma warning disable IDE0063

namespace YuRis_Tool
{
    class YSER
    {
        public class ErrorMessage
        {
            public uint Code;
            public string Message;
        }

        List<ErrorMessage> _messages;

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

            if (magic != 0x52455359)
            {
                throw new Exception("Not a valid YSER file.");
            }

            reader.ReadInt32(); // version

            var count = reader.ReadInt32();

            reader.ReadInt32(); // zero

            _messages = new List<ErrorMessage>(count);

            for (var i = 0; i < count; i++)
            {
                var e = new ErrorMessage();
                e.Code = reader.ReadUInt32();
                e.Message = reader.ReadAnsiString();
                _messages.Add(e);
            }
        }

        public IReadOnlyList<ErrorMessage> Messages
        {
            get => _messages;
        }
    }
}
