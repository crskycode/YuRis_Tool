using System;
using System.IO;
using System.Text;

namespace YuRis_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var yser = new YSER();
            //yser.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yse.ybn");

            //var yscm = new YSCM();
            //yscm.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\ysc.ybn");

            //var yslb = new YSLB();
            //yslb.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\ysl.ybn");
            //yslb.Dump(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\ysl.csv");

            //var ystl = new YSTL();
            //ystl.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yst_list.ybn");
            //ystl.Dump(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yst_list.csv");

            //_scriptKey = new byte[] { 0xB1, 0x8A, 0xE9, 0x6A };
            var ybnKey = new byte[] { 0xBE, 0x81, 0x71, 0x44 };

            //var ystb = new YSTB();
            //ystb.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yst00056.ybn", ybnKey);
            //ystb.Save(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\yst00056.ybn.d");

            var yuris = new YuRisScript();
            yuris.Init(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin", ybnKey);
            yuris.Parse(54);

            //foreach (var path in Directory.EnumerateFiles(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin", "*.ybn"))
            //{
            //    var ystb = new YSTB();
            //    ystb.Load(path);
            //    ystb.Save(path + ".d");
            //}

            //var ysvr = new YSVR();
            //ysvr.Load(@"C:\Users\Shiroha\Desktop\渡り鳥のソムニウム\ysbin\ysv.ybn");

        }
    }
}
