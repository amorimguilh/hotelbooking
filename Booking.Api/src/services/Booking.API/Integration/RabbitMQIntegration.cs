using Booking.API.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Booking.API.Integration
{
    public class RabbitMQIntegration : IQueueIntegration
    {
        private readonly IModel _channel;
        private const string save_reservation_queue = "save_reservation_queue";
        private const string delete_reservation_queue = "delete_reservation_queue";

        /// <summary>
        /// Instantiate a object to publish a message in rabbitmq
        /// </summary>
        public RabbitMQIntegration(IModel channel)
        {
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
        }

        /// <summary>
        /// Publishes a create reservation message in rabbitmq
        /// </summary>
        public void PublishMessage(Reservation reservation)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reservation));

            _channel.BasicPublish(exchange: string.Empty,
                                routingKey: save_reservation_queue,
                                basicProperties: null,
                                body: body);
        }

        /// <summary>
        /// Publishes a delete reservation message in rabbitmq
        /// </summary>
        public void PublishMessage(string id)
        {
            var body = Encoding.UTF8.GetBytes(id);

            _channel.BasicPublish(exchange: string.Empty,
                                routingKey: delete_reservation_queue,
                                basicProperties: null,
                                body: body);
        }
    }
}
