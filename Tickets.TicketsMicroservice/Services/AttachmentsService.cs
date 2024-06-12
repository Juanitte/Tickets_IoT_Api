using Microsoft.EntityFrameworkCore;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.TicketsMicroservice.Models.UnitsOfWork;

namespace Tickets.TicketsMicroservice.Services
{
    public interface IAttachmentsService
    {
        /// <summary>
        ///     Obtiene todos los archivos adjuntos
        /// </summary>
        /// <returns>un lista de archivos adjuntos <see cref="Attachment"/></returns>
        public Task<List<Attachment>> GetAll();

        /// <summary>
        ///     Obtiene el archivo adjunto cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del archivo adjunto a buscar</param>
        /// <returns><see cref="Attachment"/> con los datos del archivo adjunto</returns>
        public Task<Attachment> Get(int id);

        /// <summary>
        ///     Crea un nuevo archivo adjunto
        /// </summary>
        /// <param name="attachment"><see cref="Attachment"/> con los datos del nuevo archivo adjunto</param>
        /// <returns><see cref="Attachment"/> con los datos del archivo adjunto</returns>
        public Task<Attachment> Create(Attachment attachment);

        /// <summary>
        ///     Actualiza los datos del archivo adjunto cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="attachmentId">el id del archivo adjunto</param>
        /// <param name="attachment"><see cref="Attachment"/> con los nuevos datos del archivo adjunto</param>
        /// <returns><see cref="Attachment"/> con los datos del archivo adjunto modificado</returns>
        public Task<Attachment> Update(int attachmentId, Attachment attachment);

        /// <summary>
        ///     Elimina el archivo adjunto cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del archivo adjunto</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public Task<CreateEditRemoveResponseDto> Remove(int id);

        /// <summary>
        ///     Obtiene todos los archivos adjuntos pertenecientes al mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="messageId">el id del mensaje</param>
        /// <returns>una lista de <see cref="Attachment"/> con los datos de los archivos adjuntos</returns>
        public Task<List<Attachment?>> GetByMessage(int messageId);
    }
    public class AttachmentsService : BaseService , IAttachmentsService
    {
        #region Constructores

        public AttachmentsService(IoTUnitOfWork ioTUnitOfWork, ILogger logger) : base(ioTUnitOfWork, logger)
        {
        }

        #endregion

        #region Implementación de métodos de la interfaz

        /// <summary>
        ///     Crea un nuevo archivo adjunto
        /// </summary>
        /// <param name="attachment"><see cref="Attachment"/> con los datos del archivo adjunto</param>
        /// <returns><see cref="Attachment"/> con los datos del archivo adjunto</returns>
        public async Task<Attachment> Create(Attachment attachment)
        {
            try
            {
                var a = Task.FromResult(_unitOfWork.AttachmentsRepository.Add(attachment));
                await _unitOfWork.SaveChanges();
                return a.Result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AttachmentsService.Create => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el archivo adjunto cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del archivo adjunto</param>
        /// <returns><see cref="Attachment"/> con los datos del archivo adjunto</returns>
        public async Task<Attachment> Get(int id)
        {
            try
            {
                return await Task.FromResult(_unitOfWork.AttachmentsRepository.GetFirst(g => g.Id.Equals(id)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AttachmentsService.Get =>");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todos los archivos adjuntos
        /// </summary>
        /// <returns>una lista con los archivos adjuntos <see cref="Attachment"/></returns>
        public async Task<List<Attachment>> GetAll()
        {
            try
            {
                return await _unitOfWork.AttachmentsRepository.GetAll().ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AttachmentsService.GetAll => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene los archivos adjuntos pertenecientes a un mensaje cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="messageId">el id del mensaje</param>
        /// <returns>una lista con los archivos adjuntos <see cref="Attachment"/></returns>
        public async Task<List<Attachment?>> GetByMessage(int messageId)
        {
            try
            {
                var attachments = await _unitOfWork.AttachmentsRepository.GetAll().ToListAsync();
                var result = new List<Attachment>();
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        if (attachment != null)
                        {
                            if (attachment.MessageId == messageId)
                            {
                                result.Add(attachment);
                            }
                        }
                    }
                    return result;
                }
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AttachmentsService.GetByMessage => ");
                throw;
            }
        }

        /// <summary>
        ///     Elimina el archivo adjunto cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id">el id del archivo adjunto</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public async Task<CreateEditRemoveResponseDto> Remove(int id)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

                var attachment = _unitOfWork.AttachmentsRepository.GetFirst(g => g.Id == id);

                if (attachment != null)
                {
                    await _unitOfWork.AttachmentsRepository.Remove(id);
                    await _unitOfWork.SaveChanges();
                }
                else
                {
                    response.Errors = new List<string> { Translations.Translation_Messages.Attachment_not_found };
                }
                response.Id = id;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AttachmentsService.Remove => ");
                throw;
            }
        }

        /// <summary>
        ///     Actualiza la información de un archivo adjunto
        /// </summary>
        /// <param name="attachmentId">el id del archivo adjunto</param>
        /// <param name="newAttachment"><see cref="Attachment"/> con los nuevos datos del archivo adjunto</param>
        /// <returns><see cref="Attachment"/> con los datos del archivo adjunto</returns>
        public async Task<Attachment> Update(int attachmentId, Attachment newAttachment)
        {
            try
            {
                var attachment = await _unitOfWork.AttachmentsRepository.Get(attachmentId);
                attachment.Path = newAttachment.Path;

                _unitOfWork.AttachmentsRepository.Update(attachment);
                await _unitOfWork.SaveChanges();
                return attachment;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AttachmentsService.Update => ");
                throw;
            }
        }

        #endregion
    }
}
