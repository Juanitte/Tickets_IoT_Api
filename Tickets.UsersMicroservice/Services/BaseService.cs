using System.Security.Principal;
using Tickets.UsersMicroservice.Models.UnitsOfWork;

namespace Tickets.UsersMicroservice.Services
{
    /// <summary>
    ///     Modelo de servicio base
    /// </summary>
    public class BaseService
    {
        #region Miembros privados

        /// <summary>
        ///     Logger de la aplicación
        /// </summary>
        internal readonly ILogger _logger;

        /// <summary>
        ///     Unidad de trabajo
        /// </summary>
        internal readonly IoTUnitOfWork _unitOfWork;

        /// <summary>
        ///     Usuario actual
        /// </summary>
        internal readonly IPrincipal _user;

        #endregion

        #region Constructores

        /// <summary>
        ///     Constructor del servicio sin unidad de trabajo (solo trabaja con el contexto)
        /// </summary>
        /// <param name="logger">Logger de la aplicación</param>
        public BaseService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Constructor del servicio
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo</param>
        /// <param name="logger">Logger de la aplicación</param>
        public BaseService(IoTUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        ///     Constructor del servicio
        /// </summary>
        /// <param name="user">El usuario actual</param>
        /// <param name="unitOfWork">Unidad de trabajo</param>
        /// <param name="logger">Logger de la aplicación</param>
        public BaseService(IPrincipal user, IoTUnitOfWork unitOfWork, ILogger logger)
        {
            _user = user;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #endregion
    }
}
