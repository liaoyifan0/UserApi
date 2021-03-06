﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using ContactApi.Data;
using ContactApi.Dtos;
using ContactApi.Infrastructure;
using ContactApi.Service;
using DnsClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;

namespace ContactApi
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>();
            services.AddScoped<IContactRepository, MongoContactRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ContactContext>();
            services.Configure<MongoSettings>(Configuration.GetSection("Mongo"));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "contact_api";
                    options.Authority = "http://localhost:5000";
                    options.SaveToken = true;
                });

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.Configure<PollyOptions>(Configuration.GetSection("Polly"));
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IDnsQuery>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(options.Consul.DnsEndpoint.ToIPEndPoint());
            });
            services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAceessor = sp.GetRequiredService<IHttpContextAccessor>();
                var pollyOptions = sp.GetRequiredService<IOptions<PollyOptions>>().Value;
                return new ResilienceClientFactory(logger, httpContextAceessor, pollyOptions.RetryCount, pollyOptions.ExceptionCountBeforeBreaking);
            });
            services.AddSingleton<IHttpClient>(sp =>
            {
                var factory = sp.GetRequiredService<ResilienceClientFactory>();
                return factory.GetResilienceHttpClient();
            });            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
