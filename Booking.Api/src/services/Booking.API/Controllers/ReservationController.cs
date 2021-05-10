using Booking.API.Business;
using Booking.API.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/v1/{controller}")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationBusiness _business;
        public ReservationController(IReservationBusiness business)
        {
            _business = business ?? throw new ArgumentNullException(nameof(business));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Reservation>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            var reservations = await _business.GetReservations();
            return Ok(reservations);
        }

        [HttpGet("{id:length(24)}")]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Reservation>> GetReservationById(string id)
        {
            var reservation = await _business.GetReservationById(id);
            if (reservation == null)
            {
                return NotFound(reservation);
            }
            return Ok(reservation);
        }

        [HttpPost("PreReserve")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> PreReserve([FromBody] ReservationBase request)
        {
            if(_business.IsValidRequest(request.StartDate, request.EndDate))
            { 
                var success = await _business.PreReserve(request);
                if (success)
                {
                    return Ok();
                }
                return Conflict();
            }
            return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            var success = await _business.CreateReservation(reservation);
            if (success)
            {
                return Accepted();
            }
            return Conflict();
        }

        [HttpPut]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> UpdateReservation([FromBody] Reservation reservation)
        {
            if(String.IsNullOrWhiteSpace(reservation.Id))
            {
                return BadRequest();
            }

            var success = await _business.UpdateReservation(reservation);
            if(success)
            {
                return Accepted();
            }
            return Conflict();
        }

        [HttpDelete]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(Reservation), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteReservation(string id)
        {
            var removed = await _business.DeleteReservation(id);
            if (removed)
            {
                return Accepted();
            }
            return NotFound();
        }
    }
}
