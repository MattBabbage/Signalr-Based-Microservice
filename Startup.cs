using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.ResponseCompression;
using Hubs;
using DaprSignalrBeta.Config;
using StackExchange.Redis;

namespace DaprSignalrBeta
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
            //Configure the Redis Backpane
            var redisConfigSection = Configuration.GetSection(RedisConfig.RedisConfigKey);
            var redisConfig = redisConfigSection.Get<RedisConfig>();

            var builder = services.AddSignalR();
            Console.WriteLine($"Use redis:{redisConfig.UseAsBackplane}");
            if (redisConfig.UseAsBackplane)
            {
                Console.WriteLine($"Connecting to redis at {redisConfig.Settings.Host}.");
                builder.AddStackExchangeRedis(o =>
                {
                    o.ConnectionFactory = async writer =>
                    {
                        var config = new ConfigurationOptions
                        {
                            AbortOnConnectFail = false,
                            Password = redisConfig.Settings.Password
                        };
                        config.EndPoints.Add(redisConfig.Settings.Host, 0);
                        config.SetDefaultPorts();
                        var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                        connection.ConnectionFailed += (_, e) =>
                        {
                            Console.WriteLine("Connection to Redis failed.");
                        };

                        if (!connection.IsConnected)
                        {
                            Console.WriteLine("Did not connect to Redis.");
                        }

                        return connection;
                    };
                });
            }
            services.AddControllersWithViews();

            services.AddSignalR();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DaprSignalrBeta", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DaprSignalrBeta v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/ChatHub");
            });
        }
    }
}
