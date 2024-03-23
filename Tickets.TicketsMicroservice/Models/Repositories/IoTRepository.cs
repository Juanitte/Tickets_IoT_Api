using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Tickets.TicketsMicroservice.Models.Context;

namespace Tickets.TicketsMicroservice.Models.Repositories
{
    public class IoTRepository<T> where T : class
    {
        #region Miembros privados de solo lectura

        /// <summary>
        ///     Contexto de acceso a la base de datos
        /// </summary>
        private readonly TicketsDbContext _context;

        /// <summary>
        ///     Logger de la aplicación
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        ///     DbSet del modelo
        /// </summary>
        internal DbSet<T> _dbSet;

        #endregion

        #region Constructores

        /// <summary>
        ///     Consructor por defecto del repositorio <see cref="T"/>
        /// </summary>
        /// <param name="context"><see cref="DbContext"/>Contexto de acceso a la base de datos</param>
        /// <param name="logger"><see cref="ILogger"/>Logger de la aplicación</param>
        public IoTRepository(TicketsDbContext context, ILogger logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        ///     Añade un elemento <see cref="T"/> a la base de datos
        /// </summary>
        /// <param name="entity"><see cref="T"/></param>
        /// <returns><see cref="T"/></returns>
        public virtual T Add(T entity)
        {
            try
            {
                return _dbSet.Add(entity).Entity;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Comprueba si existe algún elemento que cumpla la función (por defecto es nula)
        /// </summary>
        /// <param name="function">Función de filtrado</param>
        /// <returns>True si hay al menos un elemento que cumpla la función</returns>
        public virtual async Task<bool> Any(Expression<Func<T, bool>> function = null)
        {
            try
            {
                if (function != null)
                {
                    return await _dbSet.AnyAsync(function);
                }
                else
                {
                    return await _dbSet.AnyAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene la lista completa de <see cref="T"/>
        /// </summary>
        /// <returns>Listado de solo lectura de <see cref="T"/></returns>
        public virtual IQueryable<T> GetAll()
        {
            try
            {
                return _dbSet.AsNoTracking();
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene la lista de <see cref="T"/> que cumplan la función pasada como parámetro 
        /// </summary>
        /// <param name="filter">Expresión que define el filtro a aplicar</param>
        /// <returns>Listado de solo lectura de <see cref="T"/></returns>
        public virtual IQueryable<T> GetAll(Expression<Func<T, bool>> filter)
        {
            try
            {
                return _dbSet.Where(filter);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene un <see cref="T"/> en base a su id
        /// </summary>
        /// <param name="id">Id a obtener</param>
        /// <returns><see cref="T"/></returns>
        public virtual async Task<T> Get(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el primer elemento que cumple el filtro de la expresión
        /// </summary>
        /// <param name="filter">Expresión que define el filtro a aplicar</param>
        /// <param name="includes">Expresión que define la propiedad de navegación que se desea incluir del modelo (por defecto null)</param>
        /// <returns><see cref="T"/></returns>
        public virtual T GetFirst(Expression<Func<T, bool>> filter, Expression<Func<T, object>>[] includes = null)
        {
            try
            {
                if (includes == null)
                {
                    return _dbSet.AsNoTracking().FirstOrDefault(filter);
                }
                else
                {
                    var objects = _dbSet.AsNoTracking().Where(filter);
                    ApplyIncludes(ref objects, includes);

                    return objects.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Elimina un elemento del tipo <see cref="T"/> de la base de datos en base a su id
        /// </summary>
        /// <param name="id">El id a eliminar</param>
        public virtual async Task Remove(int id)
        {
            try
            {
                var entity = await Get(id);
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
                _dbSet.Remove(entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Actualiza un elemento del tipo <see cref="T"/> en la base de datos
        /// </summary>
        /// <param name="entity"><see cref="T"/></param>
        public virtual void Update(T entity)
        {
            try
            {
                _dbSet.Update(entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        #endregion

        #region Métodos privados

        /// <summary>
        ///     Aplica los includes pasados como parámetro al listado de elementos pasado como referencia
        /// </summary>
        /// <param name="source">Listado de elementos</param>
        /// <param name="includes">Includes a aplicar</param>
        /// <returns>Listado de <see cref="T"/></returns>
        private IQueryable<T> ApplyIncludes(ref IQueryable<T> source, Expression<Func<T, object>>[] includes)
        {
            try
            {
                foreach (var include in includes)
                {
                    source = source.Include(include);
                }
                return source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
