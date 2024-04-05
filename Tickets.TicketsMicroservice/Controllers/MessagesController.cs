using Common.Dtos;
using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Tickets.MessagesMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Entities;

namespace Tickets.TicketsMicroservice.Controllers
{
    public class MessagesController : BaseController
    {
        #region Miembros privados

        private readonly IWebHostEnvironment _hostingEnvironment;

        #endregion

        #region Constructores

        public MessagesController(IServiceProvider serviceCollection, IWebHostEnvironment hostingEnvironment) : base(serviceCollection)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        ///     Método que obtiene todos los mensajes
        /// </summary>
        /// <returns></returns>
        [HttpGet("messages/getall")]
        public async Task<JsonResult> GetAll()
        {
            try
            {
                var messages = await IoTServiceMessages.GetAll();
                var attachments = await IoTServiceAttachments.GetAll();
                foreach (var message in messages)
                {
                    foreach (var attachment in attachments)
                    {
                        if(message.Id == attachment.MessageId)
                        {
                            message.AttachmentPaths.Add(attachment);
                        }
                    }
                }
                return new JsonResult(messages);
            }
            catch (Exception e)
            {
                return new JsonResult(new List<MessageDto>());
            }
        }

        /// <summary>
        ///     Método que obtiene un mensaje según su id
        /// </summary>
        /// <param name="id">El id del mensaje a buscar</param>
        /// <returns></returns>
        [HttpGet("messages/getbyid/{id}")]
        public async Task<JsonResult> GetById(int id)
        {
            try
            {
                var message = await IoTServiceMessages.Get(id);
                return new JsonResult(message);
            }
            catch (Exception e)
            {
                return new JsonResult(new MessageDto());
            }
        }

        /// <summary>
        ///     Método que crea un nuevo mensaje
        /// </summary>
        /// <param name="createMessage"><see cref="MessageDto"/> con los datos del mensaje</param>
        /// <returns></returns>
        [HttpPost("messages/create")]
        public async Task<IActionResult> Create([FromForm] MessageDto createMessage)
        {
            Console.WriteLine("Content: ", createMessage.Content);
            Console.WriteLine("Id del ticket: ", createMessage.TicketId);

            Message message;
            if (createMessage.Attachments.IsNullOrEmpty())
            {
                message = new Message(createMessage.Content, createMessage.Author, createMessage.TicketId);
            }
            else
            {
                message = new Message(createMessage.Content, createMessage.Author, createMessage.TicketId);
                if (!createMessage.Attachments.IsNullOrEmpty())
                {
                    foreach (var attachment in createMessage.Attachments)
                    {
                        if (attachment != null)
                        {
                            string attachmentPath = await SaveAttachmentToFileSystem(attachment, createMessage.TicketId);
                            Attachment newAttachment = new Attachment(attachmentPath, message.Id);
                            message.AttachmentPaths.Add(newAttachment);
                        }
                    }
                }
            }
            if (message == null)
            {
                return Problem("Entity 'message' is null.");
            }
            await IoTServiceMessages.Create(message);

            Ticket ticket = await IoTServiceTickets.Get(createMessage.TicketId);
            if (ticket != null)
            {
                ticket.HasNewMessages = true;
                await IoTServiceTickets.Update(createMessage.TicketId, ticket);
            }

            return Ok(message);
        }

        /// <summary>
        ///     Método que actualiza un mensaje con id proporcionado como parámetro
        /// </summary>
        /// <param name="messageId">El id del mensaje a editar</param>
        /// <param name="newMessage"><see cref="MessageDto"/> con los nuevos datos del mensaje</param>
        /// <returns></returns>
        [HttpPut("messages/update/{messageId}")]
        public async Task<IActionResult> Update(int messageId, MessageDto newMessage)
        {
            var message = await IoTServiceMessages.Get(messageId);
            if (message == null)
            {
                return BadRequest();
            }
            message.Content = newMessage.Content;

            if (!newMessage.Attachments.IsNullOrEmpty())
            {
                message.AttachmentPaths.Clear();
                foreach (var attachment in newMessage.Attachments)
                {
                    if (attachment != null)
                    {
                        string attachmentPath = await SaveAttachmentToFileSystem(attachment, message.TicketId);
                        Attachment newAttachment = new Attachment(attachmentPath, message.Id);
                        message.AttachmentPaths.Add(newAttachment);
                    }
                }
            }

            var result = await IoTServiceMessages.Update(messageId, message);

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return Problem("Error updating message.");
            }
        }

        /// <summary>
        ///     Método que elimina un mensaje cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="id">el id del mensaje a eliminar</param>
        /// <returns></returns>
        [HttpDelete("messages/remove/{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var response = new GenericResponseDto();
            try
            {
                var message = await IoTServiceMessages.Get(id);
                if (message != null)
                {
                    if (!message.AttachmentPaths.IsNullOrEmpty())
                    {
                        foreach(var attachment in message.AttachmentPaths)
                        {
                            await IoTServiceAttachments.Remove(attachment.Id);
                        }
                    }
                    var result = await IoTServiceMessages.Remove(id);
                    if (result.Errors != null && result.Errors.Any())
                    {
                        response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = result.Errors.ToList().ToDisplayList(), Location = "Messages/Remove" };
                    }
                }
            }
            catch (Exception e)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = e.Message, Location = "Messages/Remove" };
            }
            return Ok(response);
        }

        /// <summary>
        ///     Elimina los mensajes referentes a una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns></returns>
        [HttpDelete("messages/removebyticket/{ticketId}")]
        public async Task<IActionResult> RemoveByTicket(int ticketId)
        {
            var response = new GenericResponseDto();
            try
            {
                var messages = await IoTServiceMessages.GetByTicket(ticketId);

                foreach (var message in messages)
                {
                    if (!message.AttachmentPaths.IsNullOrEmpty())
                    {
                        foreach (var attachment in message.AttachmentPaths)
                        {
                            await IoTServiceAttachments.Remove(attachment.Id);
                        }
                    }
                    var result = await IoTServiceMessages.Remove(message.Id);
                    if(result.Errors != null && result.Errors.Any())
                    {
                        response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = result.Errors.ToList().ToDisplayList(), Location = "Messages/Remove" };
                    }
                }
            }
            catch(Exception e)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = e.Message, Location = "Messages/Remove" };
            }
            return Ok(response);
        }

        /// <summary>
        ///     Obtiene los mensajes referentes a una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns>un <see cref="IEnumerable{T}"/> de <see cref="Message"/> con los mensajes de la incidencia</returns>
        [HttpGet("messages/getbyticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetByTicket(int ticketId)
        {
            var messages = await IoTServiceMessages.GetByTicket(ticketId);
            if (messages == null)
            {
                return BadRequest();
            }
            return messages;
        }

        /// <summary>
        ///     Descarga un archivo cuyo nombre se pasa como parámetro
        /// </summary>
        /// <param name="attachmentPath">el nombre del archivo</param>
        /// <returns></returns>
        [HttpGet("messages/download/{ticketId}/{attachmentPath}")]
        public IActionResult DownloadAttachment(string attachmentPath, int ticketId)
        {
            string directoryPath = Path.Combine("C:/ProyectoIoT/Back/ApiTest/AttachmentStorage/", ticketId.ToString());
            string filePath = Path.Combine(directoryPath, attachmentPath);

            if (System.IO.File.Exists(filePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                string contentType = "application/octet-stream";

                return File(fileBytes, contentType, attachmentPath);
            }
            else
            {
                return NotFound("Archivo no encontrado");
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
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
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

        #endregion
    }
}
