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
using Tickets.TicketsMicroservice.Models.Dtos.ResponseDto;
using Tickets.TicketsMicroservice.Models.Dtos.FilterDto;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Microsoft.IdentityModel.Tokens;
using Attachment = Tickets.TicketsMicroservice.Models.Entities.Attachment;
using System.Text;
using System.Security.Cryptography;

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
                        ticket.Status = Status.OPENED;
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
                                    string attachmentPath = await SaveAttachmentToFileSystem(attachment, ticket.Id);
                                    Attachment newAttachment = new Attachment(attachmentPath, message.Id);
                                    message.AttachmentPaths.Add(newAttachment);
                                }
                            }
                        }

                        ticket.Messages.Add(message);

                        _unitOfWork.TicketsRepository.Update(ticket);
                        string hashedId = Hash(ticket.Id.ToString());

                        var isSent = SendMail(ticket.Email, string.Concat("http://localhost:4200/enlace/", hashedId, "/", ticket.Id));
                    }
                }
                else
                {
                    response.Id = ticket.Id;
                    response.Errors = new List<string> { "Couldn't create ticket" };
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
                    response.Errors = new List<string> { "Ticket not found" };
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
        ///     Obtiene todas las incidencias
        /// </summary>
        /// <returns></returns>
        public async Task<List<TicketDto>> GetAll()
        {
            try
            {
                var tickets = await _unitOfWork.TicketsRepository.GetAll().ToListAsync();
                List<TicketDto> result = new List<TicketDto>();
                foreach (var ticket in tickets)
                {
                    result.Add(Extensions.ConvertModel(ticket, new TicketDto()));
                    var messages = await _unitOfWork.MessagesRepository.GetAll().Where(message => message.TicketId == ticket.Id).ToListAsync();
                    foreach (var message in messages)
                    {
                        result.Last().Messages.Add(Extensions.ConvertModel(message, new MessageDto()));
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetAll => ");
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

                //Obtener incidencias filtradas por estado
                var byStatusQuery = (int)filter.Status == -1
                    ? _unitOfWork.TicketsRepository.GetAll()
                    : _unitOfWork.TicketsRepository.GetFiltered("Status", filter.Status, FilterType.equals);
                Console.WriteLine("ByStatusQuery");
                Console.WriteLine(byStatusQuery.Count());

                // Filtrar por prioridad
                var byPriorityQuery = (int)filter.Priority == -1
                    ? _unitOfWork.TicketsRepository.GetAll()
                    : _unitOfWork.TicketsRepository.GetFiltered("Priority", filter.Priority, FilterType.equals);
                Console.WriteLine("ByPriorityQuery");
                Console.WriteLine(byPriorityQuery.Count());

                // Filtrar por id de técnico
                var byUserQuery = filter.UserId == 0
                    ? _unitOfWork.TicketsRepository.GetAll()
                    : GetByUser(filter.UserId).AsQueryable();
                Console.WriteLine("ByUserQuery");
                Console.WriteLine(byUserQuery.Count());

                // Filtrar por fecha
                var byStartDateQuery = filter.Start.Equals(new DateTime(1900, 1, 1)) && filter.End.Equals(new DateTime(3000, 1, 1))
                    ? _unitOfWork.TicketsRepository.GetAll()
                    : _unitOfWork.TicketsRepository.GetAll().Where(ticket => ticket.Timestamp <= filter.End);
                Console.WriteLine("ByStartDateQuery");
                Console.WriteLine(byStartDateQuery.Count());

                var byEndDateQuery = filter.Start.Equals(new DateTime(1900, 1, 1)) && filter.End.Equals(new DateTime(3000, 1, 1))
                    ? _unitOfWork.TicketsRepository.GetAll()
                    : _unitOfWork.TicketsRepository.GetAll().Where(ticket => ticket.Timestamp >= filter.Start);
                Console.WriteLine("ByEndDateQuery");
                Console.WriteLine(byEndDateQuery.Count());

                // Filtrar por texto introducido
                var bySearchStringQuery = string.IsNullOrEmpty(filter.SearchString)
                    ? _unitOfWork.TicketsRepository.GetAll()
                    : _unitOfWork.TicketsRepository.GetFiltered(filter.SearchString);
                Console.WriteLine("BySearchStringQuery");
                Console.WriteLine(bySearchStringQuery.Count());

                // Unir todas las consultas filtradas
                var filteredTickets = new List<List<Ticket>>
                {
                    byStatusQuery.ToList(),
                    byPriorityQuery.ToList(),
                    byUserQuery.ToList(),
                    byStartDateQuery.ToList(),
                    byEndDateQuery.ToList(),
                    bySearchStringQuery.ToList()
                };

                Console.WriteLine("FilteredTickets");
                Console.WriteLine(filteredTickets.First().First().Id);
                Console.WriteLine(filteredTickets[1].First().Id);

                // Encontrar la intersección de todas las listas filtradas
                var result = filteredTickets
                    .Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToList());

                response.Tickets = result.Select(s => s.ToResumeDto()).ToList();
                Console.WriteLine("Response");
                Console.WriteLine(response.Tickets.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo las incidencias filtradas");
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
                var tickets = _unitOfWork.TicketsRepository.GetAll().Where(ticket => ticket.UserId == userId).ToList();
                return tickets;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TicketsService.GetByUser => ");
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

                    _unitOfWork.TicketsRepository.Update(ticket);
                    await _unitOfWork.SaveChanges();
                    response.IsSuccess(ticketId);
                }
                else
                {
                    response.Id = ticketId;
                    response.Errors = new List<string> { "Ticket not found" };
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
                    return true;
                }
            } catch(Exception e)
            {
                _logger.LogError(e, "Send Mail => ");
                return false;
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
