using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Repositories
{
    public class CacheRepository : ICacheRepository
    {
        private readonly ConnectionMultiplexer _conn;

        public CacheRepository(ConnectionMultiplexer conn)
        {
            _conn = conn;
        }

        public async Task DeleteKeys(RedisKey[] dates)
        {
            var redis = _conn.GetDatabase();
            await redis.KeyDeleteAsync(dates, CommandFlags.FireAndForget);
        }

        public async Task<bool> CheckDatesAvailability(RedisKey[] dates, string ownerEmail)
        {
            var isAvailable = true;
            var redis = _conn.GetDatabase();
            var redisValues = await redis.StringGetAsync(dates);
            if (redisValues.Any(email => email.HasValue && (string)email != ownerEmail))
            {
                //if redis find any date, it's impossible to reserve it for now.
                isAvailable = false;
            }
            return isAvailable;
        }

        public async Task<Dictionary<string, int>> GetReservedDatesOwners(RedisKey[] dates)
        {
            var redis = _conn.GetDatabase();
            var redisValues = await redis.StringGetAsync(dates);
            var reservationsDicionary = new Dictionary<string, int>();
            foreach (var redisValue in redisValues)
            {
                if (redisValue.HasValue)
                {
                    var reservationOwner = (string)redisValue;
                    if(reservationsDicionary.ContainsKey(reservationOwner))
                    {
                        reservationsDicionary[reservationOwner]++;
                    }
                    else
                    {
                        reservationsDicionary.Add(reservationOwner, 1);
                    }
                }
            }
            return reservationsDicionary;
        }

        public async Task<bool> SetReservationDates(RedisKey[] dates, string email, TimeSpan expireTime)
        {
            var redis = _conn.GetDatabase();
            var tran = redis.CreateTransaction();

            foreach(var date in dates)
            {
                tran.AddCondition(Condition.KeyNotExists(date));
                tran.StringSetAsync(date, email);
                tran.KeyExpireAsync(date, expireTime);
            }
            return await tran.ExecuteAsync();
        }
    }
}