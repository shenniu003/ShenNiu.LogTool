using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace ShenNiu.LogTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding("GB2312");

            //Console.WriteLine("输入服务绑定的Ip：");
            //var strHost = Console.ReadLine(); if (string.IsNullOrWhiteSpace(strHost)) { strHost = "127.0.0.1"; }
            //Console.WriteLine("输入服务绑定的端口：");
            //var strPort = Console.ReadLine(); if (string.IsNullOrWhiteSpace(strPort)) { strPort = "12345"; }

            //var hostAndPort = $"http://{strHost}:{strPort}";
            var hostAndPort = "http://127.0.0.1:12345";

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(hostAndPort)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
