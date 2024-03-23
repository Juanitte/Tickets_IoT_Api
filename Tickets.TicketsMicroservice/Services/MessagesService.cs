using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Sockets;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.TicketsMicroservice.Models.UnitsOfWork;

namespace Tickets.TicketsMicroservice.Services
{
    public interface IMessagesService
    {
        /// <summary>
        ///     Obtiene todos los mensajes
        /// </summary>
        /// <returns>un lista de mensajes <see cref="Message"/></returns>
        public Task<List<Message>> GetAll();

        /// <summary>
        ///     Obtiene el mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del mensaje a buscar</param>
        /// <returns><see cref="Message"/> con los datos del mensaje</returns>
        public Task<Message> Get(int id);

        /// <summary>
        ///     Crea un nuevo mensaje
        /// </summary>
        /// <param name="message"><see cref="Message"/> con los datos del nuevo mensaje</param>
        /// <returns><see cref="Message"/> con los datos del mensaje</returns>
        public Task<Message> Create(Message message);

        /// <summary>
        ///     Actualiza los datos del mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="messageId">el id del mensaje</param>
        /// <param name="message"><see cref="Message"/> con los nuevos datos del mensaje</param>
        /// <returns><see cref="Message"/> con los datos del mensaje modificado</returns>
        public Task<Message> Update(int messageId, Message message);

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
        public Task<List<Message?>> GetByTicket(int ticketId);

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
        /// <returns><see cref="Message"/> con los datos del mensaje</returns>
        public async Task<Message> Create(Message message)
        {
            try
            {
                if (!message.AttachmentPaths.IsNullOrEmpty())
                {
                    foreach (var attachmentPath in message.AttachmentPaths)
                    {
                        _unitOfWork.AttachmentsRepository.Add(attachmentPath);
                    }
                }
                var m = Task.FromResult(_unitOfWork.MessagesRepository.Add(message));
                await _unitOfWork.SaveChanges();
                return m.Result;
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
                }
                else
                {
                    response.Errors = new List<string> { "Message not found" };
                }
                response.Id = id;
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
        /// <returns><see cref="Message"/> con la información del mensaje</returns>
        public async Task<Message> Get(int id)
        {
            try
            {
                var message = _unitOfWork.MessagesRepository.GetFirst(g => g.Id.Equals(id));
                message.AttachmentPaths = await _unitOfWork.AttachmentsRepository.GetAll().Where(a => a.MessageId == message.Id).ToListAsync();
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
        public async Task<List<Message>> GetAll()
        {
            try
            {
                return await _unitOfWork.MessagesRepository.GetAll().ToListAsync();
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
        /// <returns>una lista con los mensajes <see cref="Message"/></returns>
        public async Task<List<Message?>> GetByTicket(int ticketId)
        {
            try
            {
                var messages = await _unitOfWork.MessagesRepository.GetAll().ToListAsync();
                var result = new List<Message>();
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        if (message != null)
                        {
                            if (message.TicketId == ticketId)
                            {
                                message.AttachmentPaths = await _unitOfWork.AttachmentsRepository.GetAll().Where(a => a.MessageId == message.Id).ToListAsync();
                                result.Add(message);
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
        /// <returns><see cref="Message"/> con los datos del mensaje</returns>
        public async Task<Message> Update(int messageId, Message newMessage)
        {
            try
            {
                var message = await _unitOfWork.MessagesRepository.Get(messageId);
                message.Content = newMessage.Content;
                message.AttachmentPaths = newMessage.AttachmentPaths;

                _unitOfWork.MessagesRepository.Update(message);
                await _unitOfWork.SaveChanges();
                return message;
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

                var messages = _unitOfWork.MessagesRepository.GetAll();

                if (messages != null)
                {
                    foreach(var message in messages)
                    {
                        if(message.TicketId == ticketId)
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
                        else
                        {
                            response.Errors = new List<string> { "Message not found" };
                        }
                        response.Id = message.Id;
                    }
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
    }
}
