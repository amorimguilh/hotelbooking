using Booking.API.Business;
using Booking.API.Data;
using Booking.API.Integration;
using Booking.API.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Booking.API
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking.API", Version = "v1" });
            });

            var envVariable = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? "localhost";

            Thread.Sleep(8000);

            var factory = new ConnectionFactory()
            {
                Uri = new Uri($"amqp://user:mysecretpassword@{envVariable}")
            };

            var channel = factory.CreateConnection().CreateModel();
            services.AddSingleton(channel);
            services.AddSingleton<IQueueIntegration, RabbitMQIntegration>();
            services.AddSingleton<ConnectionMultiplexer>(ConnectionMultiplexer.Connect(Configuration.GetValue<string>("CacheSettings:ConnectionString")));
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddScoped<IReservationContext, ReservationContext>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IReservationBusiness, ReservationBusiness>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
