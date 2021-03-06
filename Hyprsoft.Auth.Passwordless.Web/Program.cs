﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Hyprsoft.Auth.Passwordless.Tests")]
namespace Hyprsoft.Auth.Passwordless.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseApplicationInsights()
                .UseStartup<Startup>();
    }
}
