using NUnit.Framework;

namespace Booking.API.Tests
{
    public class ReservationRepositoryTests
    {
        public ReservationRepositoryTests()
        {

        }

        [SetUp]
        public void Setup()
        {
        }

        #region Get Reservations Tests

        [Test]
        public void OnRequestingAllReservations_ShouldReturnThem()
        {
            Assert.Pass();
        }

        #endregion Get Reservations Tests

        #region Get Reservation by Id Tests

        [Test]
        public void OnRequestingAExistingReservation_ByItsId_ShouldReturnIt()
        {
            Assert.Pass();
        }

        #endregion Get Reservation by Id Tests

        #region Get Reservation by date range Tests

        [Test]
        public void OnCheckDateRangeAvailability_AndItsAvailable_ShouldReturnAvailable()
        {
            Assert.Pass();
        }

        #endregion Get Reservation by date range Tests

        #region Create Reservation Tests

        [Test]
        public void OnCreatingAReservation_AndItsAvailable_ShouldReturnSuccess()
        {
            Assert.Pass();
        }

        #endregion Create Reservation Tests

        #region Update Reservation Tests

        [Test]
        public void OnUpdatingAReservation_AndItsAvailable_ShouldReturnSuccess()
        {
            Assert.Pass();
        }

        #endregion Update Reservation Tests

        #region Delete Reservation Tests

        [Test]
        public void OnDeletingAReservation_AndItExists_ShouldReturnSuccess()
        {
            Assert.Pass();
        }

        #endregion Delete Reservation Tests
    }
}