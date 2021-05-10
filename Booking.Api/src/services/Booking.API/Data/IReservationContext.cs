using Booking.API.Entities;
using MongoDB.Driver;

namespace Booking.API.Data
{
    public interface IReservationContext
    {
        IMongoCollection<Reservation> Reservations { get; }
    }
}
