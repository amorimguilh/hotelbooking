using Booking.API.Business;
using Booking.API.Controllers;
using Booking.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Tests
{
    public class ReservationControllerTests
    {
        private Mock<IReservationBusiness> _businessMock;
        private ReservationController _controller;
        private IEnumerable<Reservation> _preConfiguredReservations;

        [SetUp]
        public void Setup()
        {
            _preConfiguredReservations = UnitTestHelperMethods.GetPreconfiguredReservations();
            _businessMock = new Mock<IReservationBusiness>();
            _controller = new ReservationController(_businessMock.Object);
        }

        #region Get Reservations Tests

        [Test]
        public async Task OnRequestingAllReservations_ShouldReturnThem()
        {
            // Arrange
            var expectedNumberOfReservations = _preConfiguredReservations.Count();
            var expectedStatusCode = 200;

            _businessMock.Setup(a => a.GetReservations()).ReturnsAsync(_preConfiguredReservations);

            // Act
            var callResult = await _controller.GetReservations();

            // Assert
            Assert.IsNotNull(callResult);
            Assert.IsInstanceOf(typeof(OkObjectResult), callResult.Result);
            var reservationResult = callResult.Result as OkObjectResult;
            var statusCode = reservationResult.StatusCode;
            Assert.AreEqual(expectedStatusCode, statusCode);
            Assert.IsInstanceOf(typeof(IEnumerable<Reservation>), reservationResult.Value);
            var reservations = reservationResult.Value as IEnumerable<Reservation>;
            Assert.AreEqual(expectedNumberOfReservations, reservations.Count());
        }

        #endregion Get Reservations Tests

        #region Get Reservation by Id Tests

        [Test]
        public async Task OnRequestingAExistingReservation_ByItsId_ShouldReturnIt()
        {
            // Arrange
            var reservationId = "0003";
            var expectedStatusCode = 200;

            var expectedReservation = _preConfiguredReservations.FirstOrDefault(reservation => reservation.Id == reservationId);

            _businessMock.Setup(a => a.GetReservationById(reservationId)).ReturnsAsync(expectedReservation);

            // Act
            var callResult = await _controller.GetReservationById(reservationId);

            // Assert
            Assert.IsNotNull(callResult);
            Assert.IsInstanceOf(typeof(OkObjectResult), callResult.Result);
            var reservationResult = callResult.Result as OkObjectResult;
            var statusCode = reservationResult.StatusCode;
            Assert.AreEqual(expectedStatusCode, statusCode);
            Assert.IsInstanceOf(typeof(Reservation), reservationResult.Value);
            var reservation = reservationResult.Value as Reservation;
            Assert.AreEqual(reservationId, reservation.Id);
        }

        [Test]
        public async Task OnRequestANonExistingReservation_ByItsId_ShouldReturn404()
        {
            // Arrange
            var reservationId = "0007";
            var expectedStatusCode = 404;

            var expectedReservation = _preConfiguredReservations.FirstOrDefault(reservation => reservation.Id == reservationId);

            _businessMock.Setup(a => a.GetReservationById(reservationId)).ReturnsAsync(expectedReservation);

            // Act
            var callResult = await _controller.GetReservationById(reservationId);

            // Assert
            Assert.IsNotNull(callResult);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), callResult.Result);
            var reservationResult = callResult.Result as NotFoundObjectResult;
            var statusCode = reservationResult.StatusCode;
            Assert.AreEqual(expectedStatusCode, statusCode);
            Assert.AreEqual(null, reservationResult.Value);
        }

        #endregion Get Reservation by Id Tests

        #region Create Reservation Tests

        [Test]
        public async Task OnCreatingAReservation_AndItsAvailable_ShouldReturnSuccess()
        {
            // Arrange
            var expectedStatusCode = 202;
            var reservation = new Reservation
            {
                Id = "fakeId",
                StartDate = new DateTime(2021, 10, 01),
                EndDate = new DateTime(2021,10,03),
                ReservationOwnerEmail = "tst@tst.com"
            };

            _businessMock.Setup(a => a.CreateReservation(reservation)).ReturnsAsync(true);

            // Act
            var callResult = await _controller.CreateReservation(reservation);

            // Assert
            Assert.IsNotNull(callResult);
            Assert.IsInstanceOf(typeof(AcceptedResult), callResult);
            var reservationResult = callResult as AcceptedResult;
            var statusCode = reservationResult.StatusCode;
            Assert.AreEqual(expectedStatusCode, statusCode);
            Assert.AreEqual(null, reservationResult.Value);
        }

        #endregion Create Reservation Tests

        #region Update Reservation Tests

        [Test]
        public async Task OnUpdatingAReservation_AndItsAvailable_ShouldReturnSuccess()
        {
            // Arrange
            var expectedStatusCode = 202;
            var reservation = new Reservation
            {
                Id = "fakeId",
                StartDate = new DateTime(2021, 10, 01),
                EndDate = new DateTime(2021, 10, 03),
                ReservationOwnerEmail = "tst@tst.com"
            };

            _businessMock.Setup(a => a.UpdateReservation(reservation)).ReturnsAsync(true);

            // Act
            var callResult = await _controller.UpdateReservation(reservation);

            // Assert
            Assert.IsNotNull(callResult);
            Assert.IsInstanceOf(typeof(AcceptedResult), callResult);
            var reservationResult = callResult as AcceptedResult;
            var statusCode = reservationResult.StatusCode;
            Assert.AreEqual(expectedStatusCode, statusCode);
            Assert.AreEqual(null, reservationResult.Value);
        }

        #endregion Update Reservation Tests

        #region Delete Reservation Tests

        [Test]
        public async Task OnDeletingAReservation_AndItExists_ShouldReturnSuccess()
        {
            // Arrange
            var expectedStatusCode = 202;
            var reservationId = "fakeID";

            _businessMock.Setup(a => a.DeleteReservation(reservationId)).ReturnsAsync(true);

            // Act
            var callResult = await _controller.DeleteReservation(reservationId);

            // Assert
            Assert.IsNotNull(callResult);
            Assert.IsInstanceOf(typeof(AcceptedResult), callResult);
            var reservationResult = callResult as AcceptedResult;
            var statusCode = reservationResult.StatusCode;
            Assert.AreEqual(expectedStatusCode, statusCode);
            Assert.AreEqual(null, reservationResult.Value);
        }

        #endregion Delete Reservation Tests
    }
}