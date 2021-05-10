using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Booking.API.Entities
{
    public class Reservation : ReservationBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}