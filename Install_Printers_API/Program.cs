using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Install_Printers_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(opt =>
                    {
                        opt.Limits.MaxRequestBodySize = 10L * 1024L * 1024L * 1024L;
                        opt.ListenAnyIP(5100);
                    });

                    webBuilder.UseStartup<Startup>();
                }).ConfigureLogging(logging => 
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
    }
}
