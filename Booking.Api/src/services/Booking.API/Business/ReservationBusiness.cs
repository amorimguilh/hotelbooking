using Booking.API.Entities;
using Booking.API.Repositories;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Business
{
    public class ReservationBusiness : IReservationBusiness
    {
        private readonly IReservationRepository _repository;
        private readonly ICacheRepository _cache;
        private readonly ILogger<ReservationBusiness> _logger;
        private readonly static TimeSpan DEFAULT_PRE_RESERVATION_TIME = new TimeSpan(0, 1, 0);
        private readonly static string DEFAULT_DATE_FORMAT = "yyyyMMdd";
        private readonly static int MAXIMUM_ADVANCE_DAYS_BOOKING = 30;
        private readonly static int MAXIMUM_DAYS_BOOKING = 3;

        public ReservationBusiness(
            IReservationRepository repository,
            ICacheRepository cache,
            ILogger<ReservationBusiness> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            try
            {
                return await _repository.GetReservations();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<Reservation> GetReservationById(string id)
        {
            try
            {
                return await _repository.GetReservation(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<bool> PreReserve(ReservationBase reservation)
        {
            try
            {
                var (_, dates) = FromStartEndDateToRedisDateArray(reservation.StartDate, reservation.EndDate);

                // Go to cache, if it's empty, go to the database, if it's ok, save cache for 1min
                var isAvailable = await _cache.CheckDatesAvailability(dates, reservation.ReservationOwnerEmail);
                var success = false;
                if (isAvailable)
                {
                    // I can lock for 1 min
                    success = await _cache.SetReservationDates(dates, reservation.ReservationOwnerEmail, DEFAULT_PRE_RESERVATION_TIME);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> CreateReservation(Reservation reservation)
        {
            try
            {
                var success = false;

                var (numberOfDays, dates) = FromStartEndDateToRedisDateArray(reservation.StartDate, reservation.EndDate);

                // Get the owners of the requested dates
                var dateOwnersDictionary = await _cache.GetReservedDatesOwners(dates);

                // In order to proceed with the reservation the requested dates must be only one owner, that owner
                // must be the same one who make this request and the number of the requested days must match 
                // the number of days in the pre-reservation
                if (CanBook(dateOwnersDictionary, reservation.ReservationOwnerEmail, numberOfDays))
                {
                    // update the cache life to the amount of days of the reservation.
                    await _cache.SetReservationDates(dates, reservation.ReservationOwnerEmail, new TimeSpan(numberOfDays, 0, 0, 0));

                    // Persist in database
                    // Maybe change it for rabbit mq after
                    await _repository.CreateReservation(reservation);
                    success = true;
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateReservation(Reservation reservation)
        {
            try
            {
                var success = false;
                var (numberOfDays, dates) = FromStartEndDateToRedisDateArray(reservation.StartDate, reservation.EndDate);

                // Get the owners of the requested dates
                var dateOwnersDictionary = await _cache.GetReservedDatesOwners(dates);

                if (CanBook(dateOwnersDictionary, reservation.ReservationOwnerEmail, numberOfDays))
                {
                    // create the new reservation
                    // update the cache life to the amount of days of the reservation.
                    await _cache.SetReservationDates(dates, reservation.ReservationOwnerEmail, new TimeSpan(numberOfDays, 0, 0, 0));

                    // remove the old keys
                    var previousReservation = await _repository.GetReservation(reservation.Id);
                    var (previousReservationNumberOfDays, previousReservationDates) = FromStartEndDateToRedisDateArray(previousReservation.StartDate, previousReservation.EndDate);

                    var keysToRemove = dates.Where(x => previousReservationDates.Contains(x)).ToArray();
                    await _cache.DeleteKeys(keysToRemove);

                    // Persist in database
                    // Maybe change it for rabbit mq after
                    await _repository.UpdateReservation(reservation);

                    success = true;
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteReservation(string id)
        {
            try
            {
                return await _repository.RemoveReservation(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        private static (int, RedisKey[]) FromStartEndDateToRedisDateArray(DateTime startDate, DateTime endDate)
        {
            var numberOfDays = endDate.Subtract(startDate).Days + 1;
            var redisDateKeyArray = Enumerable.Range(0, numberOfDays)
              .Select(offset => (RedisKey)startDate.AddDays(offset).ToString(DEFAULT_DATE_FORMAT))
              .ToArray();

            return (numberOfDays, redisDateKeyArray);
        }

        private bool CanBook(Dictionary<string, int> dateOwnersDictionary, string ownerEmail, int numberOfDays)
        {
            // In order to proceed with the reservation the requested dates must be only one owner, that owner
            // must be the same one who make this request and the number of the requested days 
            // must be smaller or equals than pre-reserved days
            return dateOwnersDictionary.Keys.Count == 1 && dateOwnersDictionary.ContainsKey(ownerEmail) && numberOfDays <= dateOwnersDictionary[ownerEmail];
        }

        /// <summary>
        /// can’t be longer than 3 days and
        /// can’t be reserved more than 30 days in advance.
        /// should start at least next day
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public bool IsValidRequest(DateTime startDate, DateTime endDate)
        {
            var success = false;
            if (startDate.Date < endDate.Date &&
               startDate.Date > DateTime.Today.Date &&
               ((endDate.Date - startDate.Date).Days + 1) <= MAXIMUM_DAYS_BOOKING &&
               ((endDate.Date - DateTime.Today.Date).Days + 1) <= MAXIMUM_ADVANCE_DAYS_BOOKING)
            {
                success = true;
            }
            return success;
        }
    }
}
