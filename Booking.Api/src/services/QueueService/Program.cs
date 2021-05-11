using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueService.Data;
using QueueService.Integration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var envVariable = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? "localhost";
                    Thread.Sleep(8000);

                    var factory = new ConnectionFactory()
                    {
                        Uri = new Uri($"amqp://user:mysecretpassword@{envVariable}")
                    };

                    services.AddScoped<IReservationContext, ReservationContext>();
                    services.AddScoped<IReservationRepository, ReservationRepository>();
                    var channel = factory.CreateConnection().CreateModel();
                    services.AddSingleton(channel);
                    services.AddSingleton<IQueueIntegration, RabbitMQIntegration>();
                    services.AddHostedService<Worker>();
                });
    }
}
