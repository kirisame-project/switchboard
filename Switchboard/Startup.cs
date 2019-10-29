using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Switchboard.Common;
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
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddLogging(builder => builder.AddConsole());
            services.AddMvc();

            RegisterComponents(services);
            RegisterConfigurations(_configuration, services);
        }

        private static void RegisterConfigurations(IConfiguration config, IServiceCollection services)
        {
            var upstream = new UpstreamServiceConfiguration();
            config.GetSection("upstream").Bind(upstream);
            services.AddSingleton(upstream);
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