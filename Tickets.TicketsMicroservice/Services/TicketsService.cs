using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Tickets.TicketsMicroservice.Models.Dtos.CreateDto;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.TicketsMicroservice.Models.UnitsOfWork;
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using Tickets.TicketsMicroservice.Translations;

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
        public Task<List<Ticket>> GetAll();

        /// <summary>
        ///     Obtiene la incidencia cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="id">el id de la incidencia a buscar</param>
        /// <returns><see cref="Ticket"/> con los datos de la incidencia</returns>
        public Task<Ticket> Get(int id);

        /// <summary>
        ///     Crea una nueva incidencia
        /// </summary>
        /// <param name="ticket"><see cref="Ticket"/> con los datos de la incidencia</param>
        /// <returns><see cref="Ticket"/> con los datos de la incidencia</returns>
        public Task<Ticket> Create(Ticket ticket);

        /// <summary>
        ///     Actualiza los datos de una incidencia
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="ticket"><see cref="Ticket"/> con los datos modificados de la incidencia</param>
        /// <returns></returns>
        public Task<Ticket> Update(int ticketId, Ticket ticket);

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
        public Task<bool> ChangeState(int ticketId, States state);

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
        public Task<List<Ticket?>> GetByUser(int userId);

        /// <summary>
        ///     Envía un email
        /// </summary>
        /// <param name="email">el email destino</param>
        /// <param name="link">el enlace de seguimiento</param>
        public void SendMail(string email, string link);
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
                    if (!ticket.IsAsigned)
                    {
                        ticket.State = States.OPENED.ToString();
                        ticket.IsAsigned = true;
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
                _logger.LogError("TicketsService.AsignTicket => ", e);
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
                    ticket.Priority = priority.ToString();
                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError("TicketsService.ChangePriority => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Cambia el estado de una incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="state">el nuevo estado de la incidencia</param>
        /// <returns></returns>
        public async Task<bool> ChangeState(int ticketId, States state)
        {
            try
            {
                var ticket = await _unitOfWork.TicketsRepository.Get(ticketId);
                if (ticket != null)
                {
                    ticket.State = state.ToString();
                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError("TicketsService.ChangeState => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Crea una nueva incidencia
        /// </summary>
        /// <param name="ticket"><see cref="Ticket"/> con los datos de la incidencia</param>
        /// <returns><see cref="Ticket"/> con los datos de la incidencia</returns>
        public async Task<Ticket> Create(Ticket ticket)
        {
            try
            {
                var t = Task.FromResult(_unitOfWork.TicketsRepository.Add(ticket));
                await _unitOfWork.SaveChanges();
                return t.Result;
            }
            catch(Exception e)
            {
                _logger.LogError("TicketsService.Create => ", e);
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
                }
                else
                {
                    response.Errors = new List<string> { "Ticket not found" };
                }
                response.Id = ticketId;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("UsersService.Remove => ", ticketId);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene la incidencia cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Ticket> Get(int id)
        {
            try
            {
                return await Task.FromResult(_unitOfWork.TicketsRepository.GetFirst(g => g.Id.Equals(id)));
            }
            catch (Exception e)
            {
                _logger.LogError("TicketsService.Get =>", id, e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todas las incidencias
        /// </summary>
        /// <returns></returns>
        public async Task<List<Ticket>> GetAll()
        {
            try
            {
                return await _unitOfWork.TicketsRepository.GetAll().ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("TicketsService.GetAll => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene las incidencias asignadas al usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns>una lista con las incidencias <see cref="Ticket"/></returns>
        public async Task<List<Ticket?>> GetByUser(int userId)
        {
            try
            {
                var tickets = await _unitOfWork.TicketsRepository.GetAll().ToListAsync();
                var result = new List<Ticket>();
                if (tickets != null)
                {
                    foreach (var ticket in tickets)
                    {
                        if (ticket != null)
                        {
                            if (ticket.UserId == userId)
                            {
                                result.Add(ticket);
                            }
                        }
                    }
                    return result;
                }
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError("TicketsService.GetByUser => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Actualiza la información de una incidencia
        /// </summary>
        /// <param name="ticketId">el id de la incidencia</param>
        /// <param name="ticket"><see cref="Ticket"/> con los datos de la nueva incidencia</param>
        /// <returns></returns>
        public async Task<Ticket> Update(int ticketId, Ticket newTicket)
        {
            try
            {
                Ticket ticket = await _unitOfWork.TicketsRepository.Get(ticketId);
                ticket.Email = newTicket.Email;
                ticket.Title = newTicket.Title;
                ticket.Name = newTicket.Name;
                ticket.Priority = newTicket.Priority;
                ticket.State = newTicket.State;
                ticket.UserId = newTicket.UserId;
                ticket.IsAsigned = newTicket.IsAsigned;
                ticket.HasNewMessages = newTicket.HasNewMessages;
                ticket.newMessagesCount++;

                _unitOfWork.TicketsRepository.Update(ticket);
                await _unitOfWork.SaveChanges();
                return ticket;
            }
            catch (Exception e)
            {
                _logger.LogError("TicketsService.Update => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Envía un email
        /// </summary>
        /// <param name="email">el eamil destino</param>
        /// <param name="link">el enlace de seguimiento</param>
        /// <returns></returns>
        public async void SendMail(string email, string link)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("IoT Incidencias", "noreply.iot.incidencias@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = Translation_Tickets.Email_title;
                message.Body = new TextPart("plain") { Text = string.Concat(Translations.Translation_Tickets.Email_body, "\n", link) };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    client.Authenticate("noreply.iot.incidencias@gmail.com", "levp dwqb qacd vhle");
                    client.Send(message);
                    client.Disconnect(true);
                }
            } catch(Exception e)
            {
                _logger.LogError("Send Mail => ", e);
            }
        }

        #endregion
    }
}
