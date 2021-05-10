using Booking.API.Entities;
using System;
using System.Collections.Generic;

namespace Booking.API.Tests
{
    public static class UnitTestHelperMethods
    {
        public static IEnumerable<Reservation> GetPreconfiguredReservations()
        {
            return new List<Reservation>
            {
                new Reservation
                {
                    StartDate = new DateTime(2021, 10, 18),
                    EndDate = new DateTime(2021, 10, 21),
                    ReservationOwnerEmail = "Tom@gmail.com",
                    Id = "0001"
                },
                new Reservation
                {
                    StartDate = new DateTime(2021, 7, 1),
                    EndDate = new DateTime(2021, 7, 2),
                    ReservationOwnerEmail = "Fabian@gmail.com",
                    Id = "0002"
                },
                new Reservation
                {
                    StartDate = new DateTime(2021, 5, 22),
                    EndDate = new DateTime(2021, 5, 24),
                    ReservationOwnerEmail = "Guilherme@gmail.com",
                    Id = "0003"
                },
                new Reservation
                {
                    StartDate = new DateTime(2021, 9, 10),
                    EndDate = new DateTime(2021, 9, 12),
                    ReservationOwnerEmail = "John@gmail.com",
                    Id = "0004"
                }
            };
        }
    }
}