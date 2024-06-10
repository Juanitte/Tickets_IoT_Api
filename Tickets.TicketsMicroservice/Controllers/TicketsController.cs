using Common.Dtos;
using Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Dtos.FilterDto;
using Tickets.TicketsMicroservice.Models.Dtos.ResponseDto;
using Tickets.TicketsMicroservice.Models.Entities;

namespace Tickets.TicketsMicroservice.Controllers
{
    [ApiController]
    public class TicketsController : BaseController
    {
        #region Miembros privados

        private readonly IWebHostEnvironment _hostingEnvironment;

        #endregion

        #region Constructores

        public TicketsController(IServiceProvider serviceCollection, IWebHostEnvironment hostingEnvironment) : base(serviceCollection)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        ///     Método que obtiene todas las incidencias
        /// </summary>
        /// <returns></returns>
        [HttpGet("tickets/getall")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tickets = await IoTServiceTickets.GetAll();

                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new List<CreateTicketDataDto>());
            }
        }

        /// <summary>
        ///     Método que obtiene todas las incidencias con el nombre del técnico asignado
        /// </summary>
        /// <returns></returns>
        [HttpGet("tickets/getallwithnames")]
        public async Task<IActionResult> GetAllWithNames()
        {
            try
            {
                var tickets = await IoTServiceTickets.GetAllWithNames();

                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new List<CreateTicketDataDto>());
            }
        }

        /// <summary>
        ///     Método que obtiene todas las incidencias sin terminar
        /// </summary>
        /// <returns></returns>
        [HttpGet("tickets/getnofinished")]
        public async Task<IActionResult> GetNoFinished()
        {
            try
            {
                var tickets = await IoTServiceTickets.GetNoFinished();

                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new List<CreateTicketDataDto>());
            }
        }

        /// <summary>
        ///     Método que obtiene todas las incidencias filtradas
        /// </summary>
        /// <returns></returns>
        [HttpGet("tickets/getallfilter")]
        public async Task<JsonResult> GetAllFilter([FromQuery] TicketFilterRequestDto filter)
        {
            try
            {
                var tickets = await IoTServiceTickets.GetAllFilter(filter);
                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new ResponseFilterTicketDto());
            }
        }

        /// <summary>
        ///     Método que obtiene una incidencia según su id
        /// </summary>
        /// <param name="id">El id de la incidencia a buscar</param>
        /// <returns></returns>
        [HttpGet("tickets/getbyid/{id}")]
        public async Task<JsonResult> GetById(int id)
        {
            try
            {
                var ticket = await IoTServiceTickets.Get(id);
                return new JsonResult(ticket);
            }
            catch (Exception e)
            {
                return new JsonResult(new CreateTicketDataDto());
            }
        }

        /// <summary>
        ///     Método que obtiene una incidencia según su id
        /// </summary>
        /// <param name="id">El id de la incidencia a buscar</param>
        /// <returns></returns>
        [HttpGet("tickets/getbyidwithname/{id}")]
        public async Task<JsonResult> GetByIdWithName(int id)
        {
            try
            {
                var ticket = await IoTServiceTickets.GetWithName(id);
                return new JsonResult(ticket);
            }
            catch (Exception e)
            {
                return new JsonResult(new CreateTicketDataDto());
            }
        }

        /// <summary>
        ///     Método que crea una nueva incidencia
        /// </summary>
        /// <param name="createTicket"><see cref="CreateTicketDto"/> con los datos de la incidencia</param>
        /// <returns></returns>
        [HttpPost("tickets/create")]
        public async Task<IActionResult> Create([FromForm] CreateTicketDto createTicket)
        {
            try
            {
                var result = await IoTServiceTickets.Create(createTicket);

                return Ok(true);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        /// <summary>
        ///     Método que actualiza una incidencia con id proporcionado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a editar</param>
        /// <param name="ticket"><see cref="CreateTicketDataDto"/> con los nuevos datos de la incidencia</param>
        /// <returns></returns>
        [HttpPost("tickets/update/{ticketId}")]
        public async Task<IActionResult> Update(int ticketId, [FromBody] CreateTicketDataDto newTicket)
        {
            try
            {
                var result = await IoTServiceTickets.Update(ticketId, newTicket);

                if (result != null)
                {
                    return Ok(true);
                }
                else
                {
                    return Problem("Error updating user.");
                }
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        /// <summary>
        ///     Método que cambia la prioridad a la incidencia con id pasado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a modificar</param>
        /// <param name="priority">el valor de la prioridad</param>
        /// <returns></returns>
        [HttpPost("tickets/changepriority/{ticketId}/{priority}")]
        public async Task<IActionResult> ChangePriority(int ticketId, int priority)
        {
            
            var result = await IoTServiceTickets.ChangePriority(ticketId, (Priorities)priority);
            if (result)
            {
                return Ok(true);
            }
            return BadRequest(false);
        }

        /// <summary>
        ///     Método que cambia el estado a la incidencia con id pasado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a modificar</param>
        /// <param name="status">el valor del estado</param>
        /// <returns></returns>
        [HttpPost("tickets/changestatus/{ticketId}/{status}")]
        public async Task<IActionResult> ChangeStatus(int ticketId, int status)
        {

            var result = await IoTServiceTickets.ChangeStatus(ticketId, (Status)status);
            if (result)
            {
                return Ok(true);
            }
            return BadRequest(false);
        }

        /// <summary>
        ///     Método que asigna una incidencia con id pasado como parámetro a un usuario con id pasado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a asignar</param>
        /// <param name="userId">el id del usuario</param>
        /// <returns></returns>
        [HttpPost("tickets/asign/{ticketId}/{userId}")]
        public async Task<IActionResult> AsignTicket(int ticketId, int userId)
        {

            var result = await IoTServiceTickets.AsignTicket(ticketId, userId);
            if (result)
            {
                return Ok(true);
            }
            return BadRequest(false);
        }

        /// <summary>
        ///     Método que elimina una incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="id">el id de la incidencia a eliminar</param>
        /// <returns></returns>
        [HttpDelete("tickets/remove/{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var response = new GenericResponseDto();
            try
            {
                await IoTServiceMessages.RemoveByTicket(id);
                var result = await IoTServiceTickets.Remove(id);
                if (result.Errors != null && result.Errors.Any())
                {
                    response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = result.Errors.ToList().ToDisplayList(), Location = "Tickets/Remove" };
                }
            }
            catch (Exception e)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = e.Message, Location = "Tickets/Remove" };
            }
            return Ok(response);
        }

        /// <summary>
        ///     Obtiene las incidencias pertenecientes a un usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns><see cref="JsonResult"/> con los datos de los tickets</returns>
        [HttpGet("/tickets/getbyuser/{userId}")]
        public async Task<JsonResult> GetByUser(int userId)
        {
            try
            {
                var tickets = IoTServiceTickets.GetByUser(userId);
                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new CreateTicketDataDto());
            }
        }

        /// <summary>
        ///     Obtiene las incidencias pertenecientes a un usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns><see cref="JsonResult"/> con los datos de los tickets</returns>
        [HttpGet("/tickets/getbyuserwithnames/{userId}")]
        public async Task<JsonResult> GetByUserWithNames(int userId)
        {
            try
            {
                var tickets = IoTServiceTickets.GetByUserWithNames(userId);
                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new CreateTicketDataDto());
            }
        }

        #endregion
    }
}
