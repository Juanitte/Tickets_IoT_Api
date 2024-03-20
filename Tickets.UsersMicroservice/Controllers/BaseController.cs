using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tickets.UsersMicroservice.Services;

namespace Tickets.UsersMicroservice.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Miembros privados

        public readonly IServiceProvider _serviceCollection;

        #endregion

        #region Miembros internos

        internal ILogger IoTLogger => (ILogger)_serviceCollection.GetService(typeof(ILogger));
        internal IConfiguration Configuration => (IConfiguration)_serviceCollection.GetService(typeof(IConfiguration));
        internal IUsersService IoTServiceUsers => (IUsersService)_serviceCollection.GetService(typeof(IUsersService));
        internal IIdentitiesService IoTServiceIdentity => (IIdentitiesService)_serviceCollection.GetService(typeof(IIdentitiesService));

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
