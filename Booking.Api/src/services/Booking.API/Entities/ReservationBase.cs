using System;
using System.ComponentModel.DataAnnotations;

namespace Booking.API.Entities
{
    public class ReservationBase
    {
        [Required]
        public string ReservationOwnerEmail { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
}