using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuRis_Tool
{
    static class Extensions
    {
        static readonly Lazy<Encoding> DefaultEncoding = new(
            () => Encoding.GetEncoding("shift_jis")
        );

        public static string ReadAnsiString(this BinaryReader reader, Encoding encoding = null)
        {
            var buffer = new List<byte>(256);

            for (var b = reader.ReadByte(); b != 0; b = reader.ReadByte())
            {
                buffer.Add(b);
            }

            if (buffer.Count == 0)
            {
                return string.Empty;
            }

            if (encoding == null)
            {
                encoding = DefaultEncoding.Value;
            }

            return encoding.GetString(buffer.ToArray());
        }

        public static string ReadAnsiString(this BinaryReader reader, int length, Encoding encoding = null)
        {
            var buffer = reader.ReadBytes(length);

            if (encoding == null)
            {
                encoding = DefaultEncoding.Value;
            }

            return encoding.GetString(buffer);
        }
    }
}
