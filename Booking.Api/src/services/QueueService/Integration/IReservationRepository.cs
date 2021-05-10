using QueueService.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QueueService.Integration
{
    public interface IReservationRepository
    {
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
