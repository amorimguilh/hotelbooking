using Booking.API.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booking.API.Repositories
{
    public interface ICacheRepository
    {
        Task<bool> CheckDatesAvailability(RedisKey[] dates, string ownerEmail);
        Task<Dictionary<string, int>> GetReservedDatesOwners(RedisKey[] dates);
        Task<bool> SetReservationDates(RedisKey[] dates, string email, TimeSpan expireTime);
        Task DeleteKeys(RedisKey[] dates);
    }
}
