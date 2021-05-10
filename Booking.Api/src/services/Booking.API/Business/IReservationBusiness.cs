using Booking.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booking.API.Business
{
    public interface IReservationBusiness
    {
        Task<IEnumerable<Reservation>> GetReservations();
        Task<Reservation> GetReservationById(string id);
        Task<bool> PreReserve(ReservationBase reservation);
        Task<bool> CreateReservation(Reservation reservation);
        Task<bool> UpdateReservation(Reservation reservation);
        Task<bool> DeleteReservation(string id);
        bool IsValidRequest(DateTime startDate, DateTime endDate);
    }
}
