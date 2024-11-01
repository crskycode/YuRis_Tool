using System;
using System.Text;

namespace YuRis_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var ybnKey = BitConverter.GetBytes(CheckSum.CRC32(Encoding.ASCII.GetBytes(args[0])));
            Array.Reverse(ybnKey);

            //解析错误字符串文件
            //var yser = new YSER();
            //yser.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yse.ybn");

            var yuris = new YuRisScript();
            yuris.Init(args[1], ybnKey);
            yuris.DecompileProject();
        }
    }
}
