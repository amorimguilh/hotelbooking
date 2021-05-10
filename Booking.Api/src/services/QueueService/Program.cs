using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    var envVariable = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST");
                    Thread.Sleep(8000);

                    var factory = new ConnectionFactory()
                    {
                        Uri = new Uri($"amqp://user:mysecretpassword@{envVariable}")
                    };

                    services.AddSingleton<IQueueIntegration, RabbitMQIntegration>();

                    services.AddHostedService<Worker>();
                });
    }
}
