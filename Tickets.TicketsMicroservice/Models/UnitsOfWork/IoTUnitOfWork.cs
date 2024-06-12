using Tickets.TicketsMicroservice.Models.Context;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.TicketsMicroservice.Models.Repositories;

namespace Tickets.TicketsMicroservice.Models.UnitsOfWork
{
    public sealed class IoTUnitOfWork
    {
        #region Miembros privados

        /// <summary>
        ///     Contexto de acceso a la base de datos
        /// </summary>
        private readonly TicketsDbContext _context;

        /// <summary>
        ///     Logger de la aplicación
        /// </summary>
        private readonly ILogger _logger;

        #region Repositorios

        /// <summary>
        ///     Repositorio de incidencias
        /// </summary>
        private IoTRepository<Ticket> _ticketsRepository;

        /// <summary>
        ///     Repositorio de mensajes
        /// </summary>

        private IoTRepository<Message> _messagesRepository;

        /// <summary>
        ///     Repositorio de archivos adjuntos
        /// </summary>
        private IoTRepository<Attachment> _attachmentsRepository;

        /// <summary>
        ///     Repositorio de incidencias + nombre de técnico
        /// </summary>
        private IoTRepository<TicketUser> _ticketUserRepository;

        #endregion

        #endregion

        #region Propiedades públicas

        /// <summary>
        ///     Repositorio de incidencias
        /// </summary>
        public IoTRepository<Ticket> TicketsRepository => _ticketsRepository ?? (_ticketsRepository = new IoTRepository<Ticket>(_context, _logger));

        /// <summary>
        ///     Repositorio de mensajes
        /// </summary>
        public IoTRepository<Message> MessagesRepository => _messagesRepository ?? (_messagesRepository = new IoTRepository<Message>(_context, _logger));

        /// <summary>
        ///     Repositorio de archivos adjuntos
        /// </summary>
        public IoTRepository<Attachment> AttachmentsRepository => _attachmentsRepository ?? (_attachmentsRepository = new IoTRepository<Attachment>(_context, _logger));

        /// <summary>
        ///     Repositorio de incidencias + nombre de técnico
        /// </summary>
        public IoTRepository<TicketUser> TicketUserRepository => _ticketUserRepository ?? (_ticketUserRepository = new IoTRepository<TicketUser>(_context, _logger));

        #endregion

        #region Constructores

        /// <summary>
        ///     Constructor por defecto
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="context"><see cref="TicketsDbContext"/></param>
        public IoTUnitOfWork(ILogger logger, TicketsDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        ///     Guarda los cambios pendientes en los contextos de base de datos
        /// </summary>
        /// <returns></returns>
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
