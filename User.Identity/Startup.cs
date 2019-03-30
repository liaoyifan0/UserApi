using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using User.Identity.Dtos;
using User.Identity.Infrastructure;
using User.Identity.Services;

namespace User.Identity
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
            services.Configure<PollyOptions>(Configuration.GetSection("Polly"));            
            services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var pollyOptions = sp.GetRequiredService<IOptions<PollyOptions>>().Value;

                return new ResilienceClientFactory(logger, httpContextAccessor, pollyOptions.RetryCount, pollyOptions.ExceptionCountBeforeBreaking);
            });
            //注册ResilienceHttpClient
            services.AddSingleton<IHttpClient>(sp => {
                return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient();
            });

            services.AddScoped<IAuthCodeService, TestAuthCodeService>()
                    .AddScoped<IUserService, UserService>();

            services.AddIdentityServer()
                .AddExtensionGrantValidator<Authentication.SmsAuthCodeGrantType>() //使用我们自定义的验证逻辑
                .AddDeveloperSigningCredential()
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));

            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
