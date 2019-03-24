using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Api.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) => {
                    builder.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("Ocelot.json");
                })
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>();
    }
}
