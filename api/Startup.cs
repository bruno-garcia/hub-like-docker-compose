using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            var logger = loggerFactory
                .AddConsole(LogLevel.Debug)
                .CreateLogger<Startup>();

            var redisConfiguration = Program.Configuration["Redis"];
            if (redisConfiguration == null) throw new ArgumentException("No Redis configuration provided.");

            if (env.IsDevelopment())
            {
                logger.LogInformation($"Connecting to: Redis: {redisConfiguration}");
                app.UseDeveloperExceptionPage();
            }

            var redis = ConnectionMultiplexer.Connect(redisConfiguration);
            applicationLifetime.ApplicationStopping.Register(() => redis.Dispose());

            app.Run(async (context) =>
            {
                if (context.Request.Path.Value == "/favicon.ico")
                    return;

                var counter = await redis.GetDatabase(0).StringIncrementAsync("ApiHitCounter");
                await context.Response.WriteAsync($"Redis hit counter: {counter}");
            });
        }
    }
}