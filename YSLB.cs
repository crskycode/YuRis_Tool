using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE0017
#pragma warning disable IDE0063

namespace YuRis_Tool
{
    class YSLB
    {
        public class Label
        {
            public string Name { get; set; }
            public uint NameHash { get; set; }
            public uint CommandIndex { get; set; }
            public uint ScriptId { get; set; }
            public byte Unk2 { get; set; }
            public byte Unk3 { get; set; }
        }

        List<Label> _labels;

        public void Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }
        }

        public void Dump(string filePath)
        {
            using var textWriter = File.CreateText(filePath);
            using var csvWriter = new CsvHelper.CsvWriter(textWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(_labels);
            csvWriter.Flush();
        }

        void Read(BinaryReader reader)
        {
            var magic = reader.ReadInt32();

            if (magic != 0x424C5359)
            {
                throw new Exception("Not a valid YSTB file.");
            }

            reader.ReadInt32(); // version

            var count = reader.ReadInt32();

            for (var i = 0; i < 256; i++)
            {
                reader.ReadInt32();
            }

            _labels = new List<Label>(count);

            for (var i = 0; i < count; i++)
            {
                var lab = new Label();

                lab.Name = reader.ReadAnsiString(reader.ReadByte());
                lab.NameHash = reader.ReadUInt32();
                lab.CommandIndex = reader.ReadUInt32();
                lab.ScriptId = reader.ReadUInt16();
                lab.Unk2 = reader.ReadByte();
                lab.Unk3 = reader.ReadByte();

                _labels.Add(lab);
            }

            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        }

        public Label Find(int scriptId, int commandIndex)
        {
            return _labels.FirstOrDefault(
                a => (a.ScriptId == scriptId) && (a.CommandIndex == commandIndex)
            );
        }
    }
}
