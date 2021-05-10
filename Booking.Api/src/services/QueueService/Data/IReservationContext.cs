using MongoDB.Driver;
using QueueService.Entities;

namespace QueueService.Data
{
    public interface IReservationContext
    {
        IMongoCollection<Reservation> Reservations { get; }
    }
}
