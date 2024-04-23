using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Sockets;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.TicketsMicroservice.Models.UnitsOfWork;

namespace Tickets.TicketsMicroservice.Services
{
    public interface IMessagesService
    {
        /// <summary>
        ///     Obtiene todos los mensajes
        /// </summary>
        /// <returns>un lista de mensajes <see cref="MessageDto"/></returns>
        public Task<List<MessageDto>> GetAll();

        /// <summary>
        ///     Obtiene el mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del mensaje a buscar</param>
        /// <returns><see cref="MessageDto"/> con los datos del mensaje</returns>
        public Task<MessageDto> Get(int id);

        /// <summary>
        ///     Crea un nuevo mensaje
        /// </summary>
        /// <param name="message"><see cref="Message"/> con los datos del nuevo mensaje</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public Task<CreateEditRemoveResponseDto> Create(CreateMessageDto createMessage);

        /// <summary>
        ///     Actualiza los datos del mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="messageId">el id del mensaje</param>
        /// <param name="message"><see cref="Message"/> con los nuevos datos del mensaje</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public Task<CreateEditRemoveResponseDto> Update(int messageId, CreateMessageDto newMessage);

        /// <summary>
        ///     Elimina el mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del mensaje</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public Task<CreateEditRemoveResponseDto> Remove(int id);

        /// <summary>
        ///     Obtiene todos los mensajes pertenecientes al ticket cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id del ticket</param>
        /// <returns>una lista de <see cref="Message"/> con los datos de los mensajes</returns>
        public Task<List<MessageDto?>> GetByTicket(int ticketId);

        /// <summary>
        ///     Elimina los mensajes pertenecientes a una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public Task<CreateEditRemoveResponseDto> RemoveByTicket(int ticketId);
    }
    public class MessagesService : BaseService, IMessagesService
    {
        #region Constructores

        public MessagesService(IoTUnitOfWork ioTUnitOfWork, ILogger logger) : base(ioTUnitOfWork, logger)
        {
        }

        #endregion

        #region Implementación de métodos de la interfaz

        /// <summary>
        ///     Crea un nuevo mensaje
        /// </summary>
        /// <param name="message"><see cref="Message"/> con los datos del nuevo mensaje</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public async Task<CreateEditRemoveResponseDto> Create(CreateMessageDto createMessage)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

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

                if (!message.AttachmentPaths.IsNullOrEmpty())
                {
                    foreach (var attachmentPath in message.AttachmentPaths)
                    {
                        _unitOfWork.AttachmentsRepository.Add(attachmentPath);
                    }
                }
                if (_unitOfWork.MessagesRepository.Add(message) != null)
                {
                    await _unitOfWork.SaveChanges();
                    response.IsSuccess(message.Id);
                }
                else
                {
                    response.Id = message.Id;
                    response.Errors = new List<string> { "Couldn't create message" };
                }

                Ticket ticket = await _unitOfWork.TicketsRepository.Get(createMessage.TicketId);
                if (ticket != null)
                {
                    ticket.HasNewMessages = true;
                    _unitOfWork.TicketsRepository.Update(ticket);
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.Create => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Elimina el mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del mensaje a eliminar</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public async Task<CreateEditRemoveResponseDto> Remove(int id)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

                var message = _unitOfWork.MessagesRepository.GetFirst(g => g.Id == id);

                if (message != null)
                {
                    if(!message.AttachmentPaths.IsNullOrEmpty())
                    {
                        foreach(var attachmentPath in message.AttachmentPaths)
                        {
                            await _unitOfWork.AttachmentsRepository.Remove(attachmentPath.Id);
                        }
                    }
                    await _unitOfWork.MessagesRepository.Remove(id);
                    await _unitOfWork.SaveChanges();
                    response.IsSuccess(id);
                }
                else
                {
                    response.Id = id;
                    response.Errors = new List<string> { "Message not found" };
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.Remove => ", id);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id a buscar</param>
        /// <returns><see cref="MessageDto"/> con la información del mensaje</returns>
        public async Task<MessageDto> Get(int id)
        {
            try
            {
                var message = Extensions.ConvertModel(_unitOfWork.MessagesRepository.GetFirst(g => g.Id.Equals(id)), new MessageDto());
                var attachments = await _unitOfWork.AttachmentsRepository.GetAll().Where(a => a.MessageId == message.Id).ToListAsync();
                foreach (var attachment in attachments)
                {
                    message.AttachmentPaths.Add(Extensions.ConvertModel(attachment, new AttachmentDto()));
                }
                return message;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.Get =>", id, e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todos los mensajes
        /// </summary>
        /// <returns>una lista con los mensajes <see cref="Message"/></returns>
        public async Task<List<MessageDto>> GetAll()
        {
            try
            {
                var messages = await _unitOfWork.MessagesRepository.GetAll().ToListAsync();
                List<MessageDto> result = new List<MessageDto>();
                foreach (var message in messages)
                {
                    result.Add(Extensions.ConvertModel(message, new MessageDto()));
                    var attachments = await _unitOfWork.AttachmentsRepository.GetAll().Where(attachment => attachment.MessageId == message.Id).ToListAsync();
                    foreach (var attachment in attachments)
                    {
                        result.Last().AttachmentPaths.Add(Extensions.ConvertModel(attachment, new AttachmentDto()));
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.GetAll => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene los mensajes pertenecientes a una incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns>una lista con los mensajes <see cref="MessageDto"/></returns>
        public async Task<List<MessageDto?>> GetByTicket(int ticketId)
        {
            try
            {
                var messages = await _unitOfWork.MessagesRepository.GetAll().Where(message => message.TicketId == ticketId).ToListAsync();
                var result = new List<MessageDto?>();
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        result.Add(Extensions.ConvertModel(message, new MessageDto()));
                            
                        message.AttachmentPaths = await _unitOfWork.AttachmentsRepository.GetAll().Where(a => a.MessageId == message.Id).ToListAsync();
                        if (!message.AttachmentPaths.IsNullOrEmpty())
                        {
                            foreach (var attachment in message.AttachmentPaths)
                            {
                                result.Last().AttachmentPaths.Add(Extensions.ConvertModel(attachment, new AttachmentDto()));
                            }
                        }
                    }
                    return result;
                }
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.GetByTicket => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Actualiza los datos de un mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="messageId">el id del mensaje</param>
        /// <param name="newMessage"><see cref="Message"/> con los nuevos datos del mensaje</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/> con los datos del mensaje</returns>
        public async Task<CreateEditRemoveResponseDto> Update(int messageId, CreateMessageDto newMessage)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();
                var message = await _unitOfWork.MessagesRepository.Get(messageId);
                if (message != null)
                {
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
                    message.Content = newMessage.Content;
                    message.AttachmentPaths = message.AttachmentPaths;

                    _unitOfWork.MessagesRepository.Update(message);
                    await _unitOfWork.SaveChanges();
                    response.IsSuccess(messageId);
                }
                else
                {
                    response.Id = messageId;
                    response.Errors = new List<string> { "Message not found" };
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.Update => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Elimina los mensajes pertenecientes a una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public async Task<CreateEditRemoveResponseDto> RemoveByTicket(int ticketId)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

                var messages = _unitOfWork.MessagesRepository.GetAll().Where(message => message.TicketId == ticketId);

                if (messages != null)
                {
                    foreach(var message in messages)
                    {
                        if (!message.AttachmentPaths.IsNullOrEmpty())
                        {
                            foreach (var attachmentPath in message.AttachmentPaths)
                            {
                                await _unitOfWork.AttachmentsRepository.Remove(attachmentPath.Id);
                            }
                        }
                        await _unitOfWork.MessagesRepository.Remove(message.Id);
                        await _unitOfWork.SaveChanges();
                    }
                    response.IsSuccess(ticketId);
                }
                else
                {
                    response.Id = ticketId;
                    response.Errors = new List<string> { "Messages not found" };
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("MessagesService.RemoveByTicket => ", ticketId);
                throw;
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
            var fileName = Path.GetFileNameWithoutExtension(attachment.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
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
