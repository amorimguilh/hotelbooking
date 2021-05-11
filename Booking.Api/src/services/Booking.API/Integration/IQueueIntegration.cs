using Booking.API.Entities;

namespace Booking.API.Integration
{
    public interface IQueueIntegration
    {
        void PublishMessage(string message);
        void PublishMessage(Reservation reservation);
    }
}
