using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.TicketsMicroservice.Models.UnitsOfWork;
using MailKit.Security;
using MimeKit;
using Tickets.TicketsMicroservice.Translations;
using Tickets.TicketsMicroservice.Models.Dtos.ResponseDto;
using Tickets.TicketsMicroservice.Models.Dtos.FilterDto;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Microsoft.IdentityModel.Tokens;
using Attachment = Tickets.TicketsMicroservice.Models.Entities.Attachment;
using Tickets.TicketsMicroservice.Utilities;

namespace Tickets.TicketsMicroservice.Services
{
    /// <summary>
    ///     Interfaz del servicio de incidencias
    /// </summary>
    public interface ITicketsService
    {
        /// <summary>
        ///     Obtiene todas las incidencias
        /// </summary>
        /// <returns>una lista de incidencias <see cref="Ticket"/></returns>
        public Task<List<TicketDto>> GetAll();

        /// <summary>
        ///     Obtiene todas las incidencias con el nombre completo del técnico asignado
        ///     usando una vista
        /// </summary>
        /// <returns></returns>
        public Task<List<TicketUser>> GetAllWithNames();

        /// <summary>
        ///     Obtiene todas las incidencias sin terminar
        /// </summary>
        /// <returns>una lista de incidencias <see cref="Ticket"/></returns>
        public Task<List<TicketDto>> GetNoFinished();

        /// <summary>
        ///     Obtiene la incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="id">el id de la incidencia a buscar</param>
        /// <returns><see cref="Ticket"/> con los datos de la incidencia</returns>
        public Task<TicketDto> Get(int id);

        /// <summary>
        ///     Crea una nueva incidencia
        /// </summary>
        /// <param name="ticket"><see cref="Ticket"/> con los datos de la incidencia</param>
        /// <returns><see cref="Ticket"/> con los datos de la incidencia</returns>
        public Task<CreateEditRemoveResponseDto> Create(CreateTicketDto createTicket);

        /// <summary>
        ///     Actualiza los datos de una incidencia
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="ticket"><see cref="Ticket"/> con los datos modificados de la incidencia</param>
        /// <returns></returns>
        public Task<CreateEditRemoveResponseDto> Update(int ticketId, CreateTicketDataDto ticket);

        /// <summary>
        ///     Elimina la incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns></returns>
        public Task<CreateEditRemoveResponseDto> Remove(int ticketId);

        /// <summary>
        ///     Cambia la prioridad de una incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="priority">la nueva prioridad de la incidencia</param>
        /// <returns></returns>
        public Task<bool> ChangePriority(int ticketId, Priorities priority);

        /// <summary>
        ///     Cambia el estado de una incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="state">el nuevo estado de la incidencia</param>
        /// <returns></returns>
        public Task<bool> ChangeStatus(int ticketId, Status status);

        /// <summary>
        ///     Asigna la incidencia cuyo id se pasa como parámetro al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="userId">el id del usuario</param>
        /// <returns></returns>
        public Task<bool> AsignTicket(int ticketId, int userId);

        /// <summary>
        ///     Obtiene las incidencias asignadas al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns>una lista con las incidencias asignadas al usuario <see cref="Ticket"/></returns>
        public IEnumerable<Ticket> GetByUser(int userId);

        /// <summary>
        ///     Obtiene los tickets filtrados
        /// </summary>
        /// <returns></returns>
        Task<ResponseFilterTicketDto> GetAllFilter(TicketFilterRequestDto filter);

        /// <summary>
        ///     Envía un email
        /// </summary>
        /// <param name="email">el email destino</param>
        /// <param name="link">el enlace de seguimiento</param>
        public bool SendMail(string email, string link);

        /// <summary>
        ///     Obtiene la incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<TicketUser> GetWithName(int id);

        /// <summary>
        ///     Obtiene las incidencias asignadas al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns>una lista con las incidencias <see cref="TicketUser"/></returns>
        public IEnumerable<TicketUser> GetByUserWithNames(int userId);
    }
    public class TicketsService : BaseService , ITicketsService
    {
        #region Constructores

        public TicketsService(IoTUnitOfWork ioTUnitOfWork, ILogger logger) : base(ioTUnitOfWork, logger)
        {
        }

        #endregion

        #region Implementación de métodos de la interfaz

        /// <summary>
        ///     Asigna la incidencia cuyo id se pasa como parámetro al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="userId">el id del usuario</param>
        /// <returns></returns>
        public async Task<bool> AsignTicket(int ticketId, int userId)
        {
            try
            {
                var ticket = await _unitOfWork.TicketsRepository.Get(ticketId);
                if (ticket != null)
                {
                    if (!ticket.IsAssigned)
                    {
                        ticket.Status = Status.OPENED;
                        ticket.IsAssigned = true;
                    }
                    ticket.UserId = userId;
                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.AsignTicket => ");
                throw;
            }
        }

        /// <summary>
        ///     Cambia la prioridad de la incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="priority">la nueva prioridad de la incidencia</param>
        /// <returns></returns>
        public async Task<bool> ChangePriority(int ticketId, Priorities priority)
        {
            try
            {
                var ticket = await _unitOfWork.TicketsRepository.Get(ticketId);
                if (ticket != null)
                {
                    ticket.Priority = priority;
                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.ChangePriority => ");
                throw;
            }
        }

        /// <summary>
        ///     Cambia el estado de una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="state">el nuevo estado de la incidencia</param>
        /// <returns></returns>
        public async Task<bool> ChangeStatus(int ticketId, Status status)
        {
            try
            {
                var ticket = await _unitOfWork.TicketsRepository.Get(ticketId);
                if (ticket != null)
                {
                    ticket.Status = status;
                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.ChangeState => ");
                throw;
            }
        }

        /// <summary>
        ///     Crea una nueva incidencia
        /// </summary>
        /// <param name="ticket"><see cref="Ticket"/> con los datos de la incidencia</param>
        /// <returns><see cref="Ticket"/> con los datos de la incidencia</returns>
        public async Task<CreateEditRemoveResponseDto> Create(CreateTicketDto createTicket)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

                var ticket = new Ticket(createTicket.TicketDto.Title, createTicket.TicketDto.Name, createTicket.TicketDto.Email);

                if (createTicket.MessageDto != null)
                {

                    if (_unitOfWork.TicketsRepository.Add(ticket) != null)
                    {
                        await _unitOfWork.SaveChanges();
                        response.IsSuccess(ticket.Id);
                        var message = new Message(createTicket.MessageDto.Content, createTicket.MessageDto.Author, ticket.Id);



                        if (!createTicket.MessageDto.Attachments.IsNullOrEmpty())
                        {
                            foreach (var attachment in createTicket.MessageDto.Attachments)
                            {
                                if (attachment != null)
                                {
                                    string attachmentPath = await Utils.SaveAttachmentToFileSystem(attachment, ticket.Id);
                                    Attachment newAttachment = new Attachment(attachmentPath, message.Id);
                                    message.AttachmentPaths.Add(newAttachment);
                                }
                            }
                        }

                        ticket.Messages.Add(message);
                        ticket.NewMessagesCount++;

                        _unitOfWork.TicketsRepository.Update(ticket);
                        await _unitOfWork.SaveChanges();
                        string hashedId = Utils.Hash(ticket.Id.ToString());

                        var isSent = SendMail(ticket.Email, string.Concat(Literals.Link_Review, hashedId, "/", ticket.Id));
                    }
                }
                else
                {
                    response.Id = ticket.Id;
                    response.Errors = new List<string> { Translation_Tickets.Error_create_ticket };
                }
                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, "TicketsService.Create => ");
                throw;
            }
        }

        /// <summary>
        ///     Elimina una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <returns><see cref="CreateEditRemoveResponseDto"/></returns>
        public async Task<CreateEditRemoveResponseDto> Remove(int ticketId)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

                var user = _unitOfWork.TicketsRepository.GetFirst(g => g.Id == ticketId);

                if (user != null)
                {
                    await _unitOfWork.TicketsRepository.Remove(ticketId);
                    await _unitOfWork.SaveChanges();
                    response.IsSuccess(ticketId);
                }
                else
                {
                    response.Id = ticketId;
                    response.Errors = new List<string> { Translation_Tickets.Ticket_not_found };
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(ticketId, "UsersService.Remove => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene la incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TicketDto> Get(int id)
        {
            try
            {
                return await Task.FromResult(Extensions.ConvertModel(_unitOfWork.TicketsRepository.GetFirst(g => g.Id.Equals(id)), new TicketDto()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.Get =>");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene la incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TicketUser> GetWithName(int id)
        {
            try
            {
                return await Task.FromResult(_unitOfWork.TicketUserRepository.GetFirst(g => g.Id.Equals(id)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetWithName =>");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todas las incidencias
        /// </summary>
        /// <returns></returns>
        public async Task<List<TicketDto>> GetAll()
        {
            try
            {
                var tickets = await _unitOfWork.TicketsRepository.GetAll().ToListAsync();
                List<TicketDto> result = tickets.Select(t => Extensions.ConvertModel(t, new TicketDto())).ToList();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetAll => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todas las incidencias junto con el nombre del técnico asignado.
        /// </summary>
        /// <returns></returns>
        public async Task<List<TicketUser>> GetAllWithNames()
        {
            try
            {
                var tickets = await _unitOfWork.TicketUserRepository.GetAll().ToListAsync();
                return tickets;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketService.GetAllWithNames => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todas las incidencias sin terminar
        /// </summary>
        /// <returns></returns>
        public async Task<List<TicketDto>> GetNoFinished()
        {
            try
            {
                var tickets = await _unitOfWork.TicketsRepository.GetAll(t => t.Status != Status.FINISHED).ToListAsync();
                List<TicketDto> result = tickets.Select(t => Extensions.ConvertModel(t, new TicketDto())).ToList();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetNoFinished => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene las incidencias filtradas
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseFilterTicketDto> GetAllFilter(TicketFilterRequestDto filter)
        {
            try
            {
                var response = new ResponseFilterTicketDto();

                //Filtrar por estado
                var byStatusQuery = (int)filter.Status == -1
                    ? _unitOfWork.TicketUserRepository.GetAll()
                    : _unitOfWork.TicketUserRepository.GetFiltered("Status", filter.Status, FilterType.equals);

                // Filtrar por prioridad
                var byPriorityQuery = (int)filter.Priority == -1
                    ? _unitOfWork.TicketUserRepository.GetAll()
                    : _unitOfWork.TicketUserRepository.GetFiltered("Priority", filter.Priority, FilterType.equals);

                // Filtrar por id de técnico
                var byUserQuery = filter.UserId == 0
                    ? _unitOfWork.TicketUserRepository.GetAll()
                    : GetByUserWithNames(filter.UserId).AsQueryable();

                // Filtrar por fecha
                var byStartDateQuery = filter.Start.Equals(new DateTime(1900, 1, 1)) && filter.End.Equals(new DateTime(3000, 1, 1))
                    ? _unitOfWork.TicketUserRepository.GetAll()
                    : _unitOfWork.TicketUserRepository.GetAll(ticket => ticket.Timestamp <= filter.End);

                var byEndDateQuery = filter.Start.Equals(new DateTime(1900, 1, 1)) && filter.End.Equals(new DateTime(3000, 1, 1))
                    ? _unitOfWork.TicketUserRepository.GetAll()
                    : _unitOfWork.TicketUserRepository.GetAll(ticket => ticket.Timestamp >= filter.Start);

                // Filtrar por texto introducido
                var bySearchStringQuery = string.IsNullOrEmpty(filter.SearchString)
                    ? _unitOfWork.TicketUserRepository.GetAll()
                    : _unitOfWork.TicketUserRepository.GetFiltered(filter.SearchString);

                // Unir todas las consultas filtradas
                var filteredTickets = new List<List<TicketUser>>
                {
                    byStatusQuery.ToList(),
                    byPriorityQuery.ToList(),
                    byUserQuery.ToList(),
                    byStartDateQuery.ToList(),
                    byEndDateQuery.ToList(),
                    bySearchStringQuery.ToList()
                };

                // Encontrar la intersección de todas las listas filtradas
                var result = filteredTickets
                    .Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToList());

                response.Tickets = result.Select(s => s.ToResumeDto()).ToList();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Translation_Tickets.Error_ticket_filter);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene las incidencias asignadas al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns>una lista con las incidencias <see cref="Ticket"/></returns>
        public IEnumerable<Ticket> GetByUser(int userId)
        {
            try
            {
                var tickets = _unitOfWork.TicketsRepository.GetAll(ticket => ticket.UserId == userId).ToList();
                return tickets;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetByUser => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene las incidencias asignadas al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns>una lista con las incidencias <see cref="TicketUser"/></returns>
        public IEnumerable<TicketUser> GetByUserWithNames(int userId)
        {
            try
            {
                var tickets = _unitOfWork.TicketUserRepository.GetAll(ticket => ticket.UserId == userId).Distinct().ToList();
                return tickets;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetByUserWithNames => ");
                throw;
            }
        }

        /// <summary>
        ///     Actualiza la información de una incidencia
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="ticket"><see cref="Ticket"/> con los datos de la nueva incidencia</param>
        /// <returns></returns>
        public async Task<CreateEditRemoveResponseDto> Update(int ticketId, CreateTicketDataDto newTicket)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();
                Ticket ticket = await _unitOfWork.TicketsRepository.Get(ticketId);
                if (ticket != null)
                {
                    ticket.Email = newTicket.Email;
                    ticket.Title = newTicket.Title;
                    ticket.Name = newTicket.Name;
                    ticket.HasNewMessages = newTicket.HasNewMessages;
                    ticket.NewMessagesCount = newTicket.NewMessagesCount;

                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    response.IsSuccess(ticketId);
                }
                else
                {
                    response.Id = ticketId;
                    response.Errors = new List<string> { Translation_Tickets.Ticket_not_found };
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.Update => ");
                throw;
            }
        }

        /// <summary>
        ///     Envía un email
        /// </summary>
        /// <param name="email">el email destino</param>
        /// <param name="link">el enlace de seguimiento</param>
        /// <returns></returns>
        public bool SendMail(string email, string link)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(Literals.Email_Name, Literals.Email_Address));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = Translation_Tickets.Email_title;
                message.Body = new TextPart("plain") { Text = string.Concat(Translations.Translation_Tickets.Email_body, "\n", link) };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(Literals.Email_Service, Literals.Email_Port, SecureSocketOptions.StartTls);
                    client.Authenticate(Literals.Email_Address, Literals.Email_Auth);
                    client.Send(message);
                    client.Disconnect(true);
                    return true;
                }
            } catch(Exception e)
            {
                _logger.LogError(e, "Send Mail => ");
                return false;
            }
        }

        #endregion
    }
}
