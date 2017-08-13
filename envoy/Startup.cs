using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Envoy
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory
                .AddConsole(LogLevel.Debug)
                .CreateLogger<Startup>();
            
            var connectionString = Program.Configuration.GetConnectionString("DefaultConnection");
            if (connectionString == null) throw new ArgumentException("No default connection string provided.");
                
            if (env.IsDevelopment())
            {
                logger.LogInformation($"Connection string: {connectionString}");
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SELECT 1", connection))
                {
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    await context.Response.WriteAsync($"SELECT 1 magically returned: {result}");
                }                
            });
        }
    }
}
