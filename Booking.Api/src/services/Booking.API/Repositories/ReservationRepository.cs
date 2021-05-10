using Booking.API.Data;
using Booking.API.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly IReservationContext _context;

        public ReservationRepository(IReservationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateReservation(Reservation reservation)
        {
            await _context.Reservations.InsertOneAsync(reservation);
        }

        public async Task<Reservation> GetReservation(string id)
        {
            return await _context
                    .Reservations
                    .Find(p => p.Id == id)
                    .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDateRangeBooked(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Reservation>
                .Filter
                .Where(reservation => (startDate <= reservation.EndDate && startDate >= reservation.StartDate) ||
                                      (endDate <= reservation.EndDate && endDate >= reservation.StartDate));

            return await _context
                    .Reservations
                    .Find(filter)
                    .AnyAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            return await _context.Reservations.Find(r => true).ToListAsync();
        }

        public async Task<bool> RemoveReservation(string id)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);

            var deleteResult = await _context.Reservations.DeleteOneAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

        public async Task<bool> UpdateReservation(Reservation reservation)
        {
            var updateResult = await _context.Reservations.ReplaceOneAsync(
                filter: r => r.Id == reservation.Id,
                replacement: reservation);

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
    }
}