using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tickets.TicketsMicroservice.Services;

namespace Tickets.TicketsMicroservice.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Miembros privados

        public readonly IServiceProvider _serviceCollection;

        #endregion

        #region Miembros internos

        internal ILogger IoTLogger => (ILogger)_serviceCollection.GetService(typeof(ILogger));
        internal IConfiguration Configuration => (IConfiguration)_serviceCollection.GetService(typeof(IConfiguration));
        internal ITicketsService IoTServiceTickets => (ITicketsService)_serviceCollection.GetService(typeof(ITicketsService));
        internal IMessagesService IoTServiceMessages => (IMessagesService)_serviceCollection.GetService(typeof(IMessagesService));
        internal IAttachmentsService IoTServiceAttachments => (IAttachmentsService)_serviceCollection.GetService(typeof(IAttachmentsService));

        #endregion

        #region Constructores

        /// <summary>
        ///     Constructor base, almacena el service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        public BaseController(IServiceProvider serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        #endregion
    }
}
