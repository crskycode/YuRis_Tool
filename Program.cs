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

            string yscom;
            string ysroot;
            if (args.Length >= 3)
            {
                yscom = args[1];
                ysroot = args[2];
            }
            else
            {
                yscom = "";
                ysroot = args[1];
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var ybnKey = BitConverter.GetBytes(CheckSum.CRC32(Encoding.ASCII.GetBytes(args[0])));
            Array.Reverse(ybnKey);

            //解析错误字符串文件
            //var yser = new YSER();
            //yser.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yse.ybn");

            //解析系统变量定义
            if(!string.IsNullOrEmpty(yscom))
                YSCD.Load(yscom);

            var yuris = new YuRisScript();
            yuris.Init(ysroot, ybnKey);

            yuris.DecompileProject();
        }
    }
}
