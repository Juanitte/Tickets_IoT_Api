﻿using Common.Dtos;
using Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Dtos.FilterDto;
using Tickets.TicketsMicroservice.Models.Dtos.ResponseDto;
using Tickets.TicketsMicroservice.Models.Entities;

namespace Tickets.TicketsMicroservice.Controllers
{
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
        public async Task<JsonResult> GetAll()
        {
            try
            {
                var tickets = await IoTServiceTickets.GetAll();
                foreach (var ticket in tickets)
                {
                    ticket.Messages = await IoTServiceMessages.GetByTicket(ticket.Id);
                }
                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new List<TicketDto>());
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
                if (ticket != null)
                {
                    ticket.Messages = await IoTServiceMessages.GetByTicket(id);
                }
                return new JsonResult(ticket);
            }
            catch (Exception e)
            {
                return new JsonResult(new TicketDto());
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

            var ticket = new Ticket(createTicket.TicketDto.Title, createTicket.TicketDto.Name, createTicket.TicketDto.Email);


            var result = await IoTServiceTickets.Create(ticket);


            if (createTicket.MessageDto != null)
            {
                var message = new Message(createTicket.MessageDto.Content, createTicket.MessageDto.Author, result.Id);



                if (!createTicket.MessageDto.Attachments.IsNullOrEmpty())
                {
                    foreach (var attachment in createTicket.MessageDto.Attachments)
                    {
                        if (attachment != null)
                        {
                            string attachmentPath = await SaveAttachmentToFileSystem(attachment, result.Id);
                            Attachment newAttachment = new Attachment(attachmentPath, message.Id);
                            message.AttachmentPaths.Add(newAttachment);
                        }
                    }
                }
                result.Messages.Add(message);

                result = await IoTServiceTickets.Update(result.Id, result);
                string hashedId = Hash(result.Id.ToString());

                var isSent = IoTServiceTickets.SendMail(result.Email, string.Concat("http://localhost:4200/enlace/", hashedId, "/", result.Id));
            }

            return Ok(result);
        }

        /// <summary>
        ///     Método que actualiza una incidencia con id proporcionado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a editar</param>
        /// <param name="ticket"><see cref="TicketDto"/> con los nuevos datos de la incidencia</param>
        /// <returns></returns>
        [HttpPut("tickets/update/{ticketId}")]
        public async Task<IActionResult> Update(int ticketId, [FromBody] TicketDto newTicket)
        {
            Ticket ticket = await IoTServiceTickets.Get(ticketId);
            if (ticket == null)
            {
                return BadRequest();
            }
            ticket.Title = newTicket.Title;
            ticket.Name = newTicket.Name;
            ticket.Email = newTicket.Email;
            ticket.HasNewMessages = newTicket.HasNewMessages;
            ticket.newMessagesCount = newTicket.NewMessagesCount;

            var result = await IoTServiceTickets.Update(ticketId, ticket);

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return Problem("Error updating user.");
            }
        }

        /// <summary>
        ///     Método que cambia la prioridad a la incidencia con id pasado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a modificar</param>
        /// <param name="priority">el valor de la prioridad</param>
        /// <returns></returns>
        [HttpPut("tickets/changepriority/{ticketId}/{priority}")]
        public async Task<IActionResult> ChangePriority(int ticketId, int priority)
        {
            
            var result = await IoTServiceTickets.ChangePriority(ticketId, (Priorities)priority);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        /// <summary>
        ///     Método que cambia el estado a la incidencia con id pasado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a modificar</param>
        /// <param name="state">el valor del estado</param>
        /// <returns></returns>
        [HttpPut("tickets/changestate/{ticketId}/{state}")]
        public async Task<IActionResult> ChangeState(int ticketId, int state)
        {

            var result = await IoTServiceTickets.ChangeState(ticketId, (States)state);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        /// <summary>
        ///     Método que asigna una incidencia con id pasado como parámetro a un usuario con id pasado como parámetro
        /// </summary>
        /// <param name="ticketId">El id de la incidencia a asignar</param>
        /// <param name="userId">el id del usuario</param>
        /// <returns></returns>
        [HttpPut("tickets/asign/{ticketId}/{userId}")]
        public async Task<IActionResult> AsignTicket(int ticketId, int userId)
        {

            var result = await IoTServiceTickets.AsignTicket(ticketId, userId);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
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
                var tickets = await IoTServiceTickets.GetByUser(userId);
                return new JsonResult(tickets);
            }
            catch (Exception e)
            {
                return new JsonResult(new TicketDto());
            }
        }

        #endregion

        #region Métodos privados

        /// <summary>
        ///     Guarda un archivo adjunto en el sistema de archivos
        /// </summary>
        /// <param name="attachment"><see cref="IFormFile"/> con los datos del archivo adjunto a guardar</param>
        /// <returns>la ruta del archivo guardado</returns>
        private async Task<string> SaveAttachmentToFileSystem(IFormFile attachment, int ticketId)
        {
            var fileName = Path.GetFileNameWithoutExtension(attachment.FileName) + Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
            string directoryPath = Path.Combine("C:/ProyectoIoT/Back/ApiTest/AttachmentStorage/", ticketId.ToString());
            string filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(stream);
            }

            return filePath;
        }

        /// <summary>
        ///     Hashea un texto
        /// </summary>
        /// <param name="text">el texto a hashear</param>
        /// <returns></returns>
        public static string Hash(string text)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #endregion
    }
}
