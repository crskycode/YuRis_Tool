using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

#pragma warning disable IDE0017
#pragma warning disable IDE0063

namespace YuRis_Tool
{
    class YSTL
    {
        public class ScriptInfo
        {
            public int Id { get; set; }
            public string Source { get; set; }
        }

        List<ScriptInfo> _scripts;

        public IEnumerator<ScriptInfo> GetEnumerator() => _scripts.GetEnumerator();

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
            csvWriter.WriteRecords(_scripts);
            csvWriter.Flush();
        }

        void Read(BinaryReader reader)
        {
            var magic = reader.ReadInt32();

            if (magic != 0x4C545359)
            {
                throw new Exception("Not a valid YSTL file.");
            }

            var version = reader.ReadInt32(); // version

            var count = reader.ReadInt32();

            _scripts = new List<ScriptInfo>(count);

            for (var i = 0; i < count; i++)
            {
                var info = new ScriptInfo();

                info.Id = reader.ReadInt32();
                info.Source = reader.ReadAnsiString(reader.ReadInt32());
                //Unknown values
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                if(version > 462)
                    reader.ReadInt32();

                _scripts.Add(info);
            }

            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        }
    }
}
