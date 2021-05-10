using Booking.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booking.API.Repositories
{
    public interface IReservationRepository
    {
        // Also locks reservations for some time
        Task<IEnumerable<Reservation>> GetReservations();
        // Used to edit an reservation
        Task<Reservation> GetReservation(string id);
        // Use to check availability
        Task<bool> IsDateRangeBooked(DateTime startDate, DateTime endDate);

        Task CreateReservation(Reservation reservation);

        Task<bool> UpdateReservation(Reservation reservation);

        Task<bool> RemoveReservation(string id);
    }
}
