using Microsoft.EntityFrameworkCore;
using Tickets.UsersMicroservice.Models.Context;
using Tickets.UsersMicroservice.Models.Entities;
using Tickets.UsersMicroservice.Models.Repositories;

namespace Tickets.UsersMicroservice.Models.UnitsOfWork
{
    public sealed class IoTUnitOfWork
    {
        #region Miembros privados

        /// <summary>
        ///     Contexto de acceso a la base de datos
        /// </summary>
        private readonly UsersDbContext _context;

        /// <summary>
        ///     Logger de la aplicación
        /// </summary>
        private readonly ILogger _logger;

        #region Repositorios

        /// <summary>
        ///     Repositorio de usuarios
        /// </summary>
        private IoTRepository<User> _usersRepository;

        /// <summary>
        ///     Repositorio de roles
        /// </summary>

        private IoTRepository<Role> _rolesRepository;

        #endregion
        #endregion

        #region Propiedades públicas

        /// <summary>
        ///     Repositorio de usuarios
        /// </summary>
        public IoTRepository<User> UsersRepository => _usersRepository ?? (_usersRepository = new IoTRepository<User>(_context, _logger));

        /// <summary>
        ///     Repositorio de roles
        /// </summary>
        public IoTRepository<Role> RolesRepository => _rolesRepository ?? (_rolesRepository = new IoTRepository<Role>(_context, _logger));

        #endregion

        #region Constructores

        /// <summary>
        ///     Constructor por defecto
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="context"><see cref="UsersDbContext"/></param>
        public IoTUnitOfWork(ILogger logger, UsersDbContext context)
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


        public void DetachLocal(User t, string entryId)
        {
            var local = _context.Set<User>()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(entryId));
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }
            _context.Entry(t).State = EntityState.Modified;
        }

        #endregion
    }
}
