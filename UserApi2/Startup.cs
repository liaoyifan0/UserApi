using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserApi2.Data;
using UserApi2.Dtos;
using UserApi2.Filters;

namespace UserApi2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<UserContext>(options =>
            {
                options.UseMySQL(Configuration.GetConnectionString("Mysql"));
            });

            services.AddMvc(options => {
                options.Filters.Add(typeof(GlobalExceptionFilter)); 
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                if (!string.IsNullOrWhiteSpace(serviceConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            IApplicationLifetime applicationLifetime,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)     
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            applicationLifetime.ApplicationStarted.Register(() => {
                RegisterService(app, serviceOptions, consul);
            });
            applicationLifetime.ApplicationStopped.Register(() => {
                DeregisterService(app, serviceOptions, consul);
            });

            app.UseMvc();

        }

        private void RegisterService(IApplicationBuilder app, 
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {   
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach(var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = address.Host,
                    Port = address.Port,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName
                };

                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();

            }
        }

        private void DeregisterService(
            IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }
    }
}
//app.Use(next =>
//{
//    Console.WriteLine("A");
//    return async (context) =>
//    {
//        Console.WriteLine("A handle request.");
//        await next(context);
//        Console.WriteLine("A handle response.");
//    };
//});

//app.Use(next =>
//{
//    Console.WriteLine("B");
//    return async (context) =>
//    {
//        Console.WriteLine("B handle request.");
//        //await context.Response.WriteAsync("Hello ASP.NET Core!");
//        await next(context);
//        Console.WriteLine("B handle response.");
//    };
//});

//app.Run(async context =>
//{
//    await context.Response.WriteAsync("Hello ASP.NET Core!");
//});