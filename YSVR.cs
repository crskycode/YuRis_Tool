using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuRis_Tool
{
    class YSVR
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

            if (magic != 0x52565359)
            {
                throw new Exception("Not a valid YSVR file.");
            }

            reader.ReadInt32(); // version

            var count = reader.ReadUInt16();

            var encoding = Encoding.GetEncoding("shift_jis");

            for (var i = 0; i < count; i++)
            {
                reader.ReadByte(); // data type
                reader.ReadByte(); // field_0
                reader.ReadUInt16(); // script id

                reader.ReadUInt16(); // store id

                var dt = reader.ReadByte(); // data type
                
                var field_2 = reader.ReadByte(); // count
                for (var j = 0; j < field_2; j++)
                {
                    reader.ReadInt32();
                }

                switch (dt)
                {
                    case 0:
                        break;
                    case 1:
                        var ivar = reader.ReadInt64();
                        break;
                    case 2:
                        var fvar = reader.ReadDouble();
                        break;
                    case 3:
                        var len = reader.ReadUInt16();
                        var data = reader.ReadBytes(len);
                        if (data.Length != 0)
                        {
                            if (data[0] == 0x4D)
                            {
                                var s = encoding.GetString(data, 3, data.Length - 3);
                                Debug.WriteLine(s);
                            }
                        }
                        // loader()
                        break;
                    default:
                        throw new Exception("unknown data type");
                }
            }
        }
    }
}
