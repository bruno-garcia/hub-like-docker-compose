using System;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
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
            var mongoConfiguration = Program.Configuration["Mongo"];
            if (mongoConfiguration == null) throw new ArgumentException("No Mongo configuration provided.");
            var envoyConfiguration = Program.Configuration["Envoy"];
            if (envoyConfiguration == null) throw new ArgumentException("No Envoy configuration provided.");

            if (env.IsDevelopment())
            {
                logger.LogInformation($"Connecting to: Redis: {redisConfiguration}");
                logger.LogInformation($"Connecting to: Mongo: {mongoConfiguration}");
                logger.LogInformation($"Connecting to: Envoy: {envoyConfiguration}");
                app.UseDeveloperExceptionPage();
            }

            var redis = ConnectionMultiplexer.Connect(redisConfiguration);
            applicationLifetime.ApplicationStopping.Register(() => redis.Dispose());

            var mongoClient = new MongoClient(mongoConfiguration);
            var mongoDb = mongoClient.GetDatabase("ApiDb");

            var envoyClient = new HttpClient
            {
                BaseAddress = new Uri(envoyConfiguration)
            };
            applicationLifetime.ApplicationStopping.Register(() => envoyClient.Dispose());

            app.Run(async (context) =>
            {
                if (context.Request.Path.Value == "/favicon.ico")
                    return;

                var counterTask = redis.GetDatabase(0).StringIncrementAsync("ApiHitCounter");
                var mongoPing = mongoDb.RunCommandAsync((Command<BsonDocument>) "{ping:1}");
                var envoySaidTask = envoyClient.GetAsync("/");

                await counterTask;
                await mongoPing;
                await envoySaidTask;

                await context.Response.WriteAsync($@"Redis hit counter: {counterTask.Result}
MongoDB ping: {mongoPing.Result}
Envoy said: {await envoySaidTask.Result.Content.ReadAsStringAsync()}");
            });
        }
    }
}