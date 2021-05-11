using Booking.API.Business;
using Booking.API.Entities;
using Booking.API.Integration;
using Booking.API.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Tests
{
    public class ReservationBusinessTests
    {
        private Mock<IReservationRepository> _repositoryMock;
        private Mock<ICacheRepository> _cacheMock;
        private Mock<IQueueIntegration> _queueIntegration;
        private Mock<ILogger<ReservationBusiness>> _loggerMock;
        private ReservationBusiness _business;
        private IEnumerable<Reservation> _preConfiguredReservations;

        [SetUp]
        public void Setup()
        {
            _preConfiguredReservations = UnitTestHelperMethods.GetPreconfiguredReservations();
            _repositoryMock = new Mock<IReservationRepository>();
            _cacheMock = new Mock<ICacheRepository>();
            _queueIntegration = new Mock<IQueueIntegration>();
            _loggerMock = new Mock<ILogger<ReservationBusiness>>();
            _business = new ReservationBusiness(_repositoryMock.Object, _cacheMock.Object, _queueIntegration.Object, _loggerMock.Object);
        }

        #region Get Reservations Tests

        [Test]
        public async Task OnRequestingAllReservations_ShouldReturnThem()
        {
            // Arrange
            var expectedNumberOfReservations = _preConfiguredReservations.Count();

            _repositoryMock.Setup(a => a.GetReservations()).ReturnsAsync(_preConfiguredReservations);

            // Act
            var reservationResult = await _business.GetReservations();

            // Assert
            Assert.IsNotNull(reservationResult);
            Assert.IsInstanceOf(typeof(IEnumerable<Reservation>), reservationResult);
            Assert.AreEqual(expectedNumberOfReservations, reservationResult.Count());
        }

        #endregion Get Reservations Tests

        #region Get Reservation by Id Tests

        [Test]
        public async Task OnRequestingAExistingReservation_ByItsId_ShouldReturnIt()
        {
            // Arrange
            var reservationId = "0003";

            var expectedReservation = _preConfiguredReservations.FirstOrDefault(reservation => reservation.Id == reservationId);

            _repositoryMock.Setup(a => a.GetReservation(reservationId)).ReturnsAsync(expectedReservation);

            // Act
            var reservation = await _business.GetReservationById(reservationId);

            // Assert
            Assert.IsNotNull(reservation);
            Assert.IsInstanceOf(typeof(Reservation), reservation);
            Assert.AreEqual(reservationId, reservation.Id);
        }

        [Test]
        public async Task OnRequestANonExistingReservation_ByItsId_ShouldReturn404()
        {
            // Arrange
            var reservationId = "0007";

            var expectedReservation = _preConfiguredReservations.FirstOrDefault(reservation => reservation.Id == reservationId);

            _repositoryMock.Setup(a => a.GetReservation(reservationId)).ReturnsAsync(expectedReservation);

            // Act
            var reservation = await _business.GetReservationById(reservationId);

            // Assert
            Assert.IsNull(reservation);
        }

        #endregion Get Reservation by Id Tests

        #region Get Reservation by date range Tests

        [Test]
        public async Task OnCheckDateRangeAvailability_AndItsAvailable_ShouldReturnAvailable()
        {
            // Arrange
            var startDate = new DateTime(2021, 04, 12);
            var endDate = new DateTime(2021, 04, 14);
            var ownerEmail = "amorim@tst.com";
            var shouldBeAvailable = true;

            _cacheMock.Setup(a => a.CheckDatesAvailability(It.IsAny<RedisKey[]>(), ownerEmail)).ReturnsAsync(true);
            _cacheMock.Setup(a => a.SetReservationDates(It.IsAny<RedisKey[]>(), ownerEmail, It.IsAny<TimeSpan>())).ReturnsAsync(true);

            // Act
            var available = await _business.PreReserve(new ReservationBase
            {
                StartDate = startDate,
                EndDate = endDate,
                ReservationOwnerEmail = ownerEmail
            });

            // Assert
            Assert.AreEqual(shouldBeAvailable, available);
        }

        [Test]
        public async Task OnCheckDateRangeAvailability_AndItsNotAvailable_ShouldReturnFalse()
        {
            // Arrange
            var startDate = new DateTime(2021, 04, 12);
            var endDate = new DateTime(2021, 04, 14);
            var ownerEmail = "amorim@tst.com";
            var shouldBeAvailable = false;

            _cacheMock.Setup(a => a.CheckDatesAvailability(It.IsAny<RedisKey[]>(), ownerEmail)).ReturnsAsync(false);

            // Act
            var available = await _business.PreReserve(new ReservationBase
            {
                StartDate = startDate,
                EndDate = endDate,
                ReservationOwnerEmail = ownerEmail
            });

            // Assert
            Assert.AreEqual(shouldBeAvailable, available);
        }

        #endregion Get Reservation by date range Tests

        #region Create Reservation Tests

        [Test]
        public async Task OnCreatingAReservation_AndReservedForHimself_ShouldReturnSuccess()
        {
            var startDate = new DateTime(2021, 04, 12);
            var endDate = new DateTime(2021, 04, 14);
            var ownerEmail = "amorim@tst.com";
            var shouldCreate = true;

            // Arrage
            var reservation = new Reservation
            {
                StartDate = startDate,
                EndDate = endDate,
                ReservationOwnerEmail = ownerEmail
            };

            var reservationsDicionary = new Dictionary<string, int>();
            reservationsDicionary.Add(ownerEmail, 3);

            _cacheMock.Setup(a => a.GetReservedDatesOwners(It.IsAny<RedisKey[]>())).ReturnsAsync(reservationsDicionary);
            _cacheMock.Setup(a => a.SetReservationDates(It.IsAny<RedisKey[]>(),ownerEmail, It.IsAny<TimeSpan>())).ReturnsAsync(true);
            _queueIntegration.Setup(a => a.PublishMessage(It.IsAny<Reservation>()));

            // Act
            var created = await _business.CreateReservation(reservation);

            // Assert
            Assert.AreEqual(shouldCreate, created);
        }

        [Test]
        public async Task OnCreatingAReservation_AndNotReservedForHimself_ShouldReturnSuccess()
        {
            var startDate = new DateTime(2021, 04, 12);
            var endDate = new DateTime(2021, 04, 14);
            var ownerEmail = "amorim@tst.com";
            var otherUserEmail = "otheruser@tsts.com";
            var shouldCreate = false;

            // Arrage
            var reservation = new Reservation
            {
                StartDate = startDate,
                EndDate = endDate,
                ReservationOwnerEmail = ownerEmail
            };

            var reservationsDicionary = new Dictionary<string, int>();
            reservationsDicionary.Add(ownerEmail, 2);
            reservationsDicionary.Add(otherUserEmail, 1);

            _cacheMock.Setup(a => a.GetReservedDatesOwners(It.IsAny<RedisKey[]>())).ReturnsAsync(reservationsDicionary);
            _cacheMock.Setup(a => a.SetReservationDates(It.IsAny<RedisKey[]>(), ownerEmail, It.IsAny<TimeSpan>())).ReturnsAsync(true);
            _queueIntegration.Setup(a => a.PublishMessage(It.IsAny<Reservation>()));

            // Act
            var created = await _business.CreateReservation(reservation);

            // Assert
            Assert.AreEqual(shouldCreate, created);
        }

        #endregion Create Reservation Tests

        #region Update Reservation Tests

        [Test]
        public async Task OnUpdatingAReservation_AndItsAvailable_ShouldReturnSuccess()
        {
            var startDate = new DateTime(2021, 04, 12);
            var previousEndDate = new DateTime(2021, 04, 14);
            var newEndDate = new DateTime(2021, 04, 13);
            var reservationId = "reservation_01";
            var ownerEmail = "amorim@tst.com";
            var shouldUpdate = true;

            // Arrange
            var previousReservation = new Reservation
            {
                Id = reservationId,
                StartDate = startDate,
                EndDate = previousEndDate,
                ReservationOwnerEmail = ownerEmail
            };

            var newReservation = new Reservation
            {
                Id = reservationId,
                StartDate = startDate,
                EndDate = newEndDate,
                ReservationOwnerEmail = ownerEmail
            };

            var reservationsDicionary = new Dictionary<string, int>();
            reservationsDicionary.Add(ownerEmail, 2);

            _cacheMock.Setup(a => a.GetReservedDatesOwners(It.IsAny<RedisKey[]>())).ReturnsAsync(reservationsDicionary);
            _cacheMock.Setup(a => a.SetReservationDates(It.IsAny<RedisKey[]>(), ownerEmail, It.IsAny<TimeSpan>())).ReturnsAsync(true);
            _cacheMock.Setup(a => a.DeleteKeys(It.IsAny<RedisKey[]>()));
            _repositoryMock.Setup(a => a.GetReservation(reservationId)).ReturnsAsync(previousReservation);
            _repositoryMock.Setup(a => a.UpdateReservation(newReservation)).ReturnsAsync(true);
            _queueIntegration.Setup(a => a.PublishMessage(It.IsAny<Reservation>()));

            // Act
            var created = await _business.UpdateReservation(newReservation);

            // Assert
            Assert.AreEqual(shouldUpdate, created);
        }

        #endregion Update Reservation Tests

        #region Delete Reservation Tests

        [Test]
        public async Task OnDeletingAReservation_AndItExists_ShouldReturnSuccess()
        {
            // Arrange
            var reservationId = "reservation_01";
            var shouldDelete = true;

            _repositoryMock.Setup(a => a.RemoveReservation(reservationId)).ReturnsAsync(true);

            // Act
            var created = await _business.DeleteReservation(reservationId);

            //Assert
            Assert.AreEqual(shouldDelete, created);
        }

        #endregion Delete Reservation Tests
    }
}