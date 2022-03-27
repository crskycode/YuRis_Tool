using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuRis_Tool
{
    class YSTD
    {
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

            if (magic != 0x44545359)
            {
                throw new Exception("Not a valid YSTD file.");
            }

            reader.ReadInt32(); // version
            reader.ReadInt32(); // count
            reader.ReadInt32(); // zero
        }
    }
}
