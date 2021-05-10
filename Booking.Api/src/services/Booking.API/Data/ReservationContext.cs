using Booking.API.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Booking.API.Data
{
    public class ReservationContext : IReservationContext
    {
        public IMongoCollection<Reservation> Reservations { get; }

        public ReservationContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            Reservations = database.GetCollection<Reservation>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        }
    }
}
