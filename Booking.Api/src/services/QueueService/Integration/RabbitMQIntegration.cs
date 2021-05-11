using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;
using QueueService.Entities;

namespace QueueService.Integration
{
    public class RabbitMQIntegration : IQueueIntegration
    {
        private const string save_reservation_queue = "save_reservation_queue";
        private const string delete_reservation_queue = "delete_reservation_queue";
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQIntegration(IModel channel, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            _channel = channel;
            _channel.QueueDeclare(queue: save_reservation_queue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.QueueDeclare(queue: delete_reservation_queue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ProcessMessage;

            _channel.BasicConsume(queue: save_reservation_queue,
                                  autoAck: true,
                                  consumer: consumer);

            _channel.BasicConsume(queue: delete_reservation_queue,
                                  autoAck: true,
                                  consumer: consumer);
        }

        /// <summary>
        /// Reads a message in both queues and directs to the correct processor
        /// </summary>
        public void ProcessMessage(object model, BasicDeliverEventArgs ea)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var body = ea.Body.ToArray();
            var repository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();

            switch (ea.RoutingKey)
            {
                
                case save_reservation_queue:
                    {
                        var reservation = JsonConvert.DeserializeObject<Reservation>(Encoding.UTF8.GetString(body));
                        if(String.IsNullOrWhiteSpace(reservation.Id))
                        {
                            repository.CreateReservation(reservation);
                        }
                        else
                        {
                            repository.UpdateReservation(reservation);
                        }
                    }
                    break;
                case delete_reservation_queue:
                    {
                        repository.RemoveReservation(Encoding.UTF8.GetString(body));
                    }
                    break;
            }
        }
    }
}
