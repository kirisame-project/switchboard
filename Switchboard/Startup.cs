using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using InfluxDB.LineProtocol.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Switchboard.Common;
using Switchboard.Controllers.WebSocketized;
using Switchboard.Metrics;
using Switchboard.Metrics.Collector;
using Switchboard.Services.Upstream;

namespace Switchboard
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseCors(builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/api/v1/lambda/socket", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var scope = app.ApplicationServices.CreateScope();
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await scope.ServiceProvider.GetService<WebSocketController>().AcceptAsync(socket,
                            CancellationToken.None);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                });
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddLogging(builder => builder.AddConsole());
            services.AddMvc();

            ConfigureMetrics(_configuration, services);
            RegisterComponents(services);
            RegisterConfigurations(_configuration, services);
        }

        private static void RegisterConfigurations(IConfiguration config, IServiceCollection services)
        {
            var upstream = new UpstreamServiceConfiguration();
            config.GetSection("upstream").Bind(upstream);
            services.AddSingleton(upstream);

            var websocket = new WebSocketSessionConfiguration();
            config.GetSection("websocket").Bind(websocket);
            services.AddSingleton(websocket);
        }

        private static void ConfigureMetrics(IConfiguration configuration, IServiceCollection services)
        {
            var config = new MetricsConfiguration();
            configuration.GetSection("metrics").Bind(config);
            var influx = config.InfluxDb;

            var serverBaseAddress = new Uri(influx.BaseUri);
            var client = new LineProtocolClient(serverBaseAddress, influx.Database, influx.Username, influx.Password);

            var collector = new QueuedMetricsCollector(client);
            services.AddHostedService(provider =>
            {
                var timer = new HostedQueueTimer(collector, TimeSpan.FromSeconds(config.FlushInterval));
                timer.OnError += e => Console.Error.WriteLine(e);
                return timer;
            });

            var predefinedTags = new Dictionary<string, string>
            {
                {"hostname", Environment.MachineName}
            };
            services.AddSingleton(new MeasurementWriterFactory(predefinedTags, collector));
        }

        private static void AddComponent(Type type, ComponentAttribute attribute, IServiceCollection services)
        {
            var singletons = type.GetCustomAttributes<DependsSingletonAttribute>();
            foreach (var dependency in singletons)
                services.AddSingleton(dependency.Type);

            var serviceType = attribute.Implements ?? type;

            switch (attribute.Lifestyle)
            {
                case ComponentLifestyle.Singleton:
                    services.AddSingleton(serviceType, type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute.Lifestyle), attribute.Lifestyle, null);
            }
        }

        private void RegisterComponents(IServiceCollection services)
        {
            var types = GetType().Assembly.DefinedTypes;
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ComponentAttribute>();
                if (attribute != null)
                    AddComponent(type, attribute, services);
            }
        }
    }
}