using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using Tickets.TicketsMicroservice.Models.Context;
using static Common.Attributes.ModelAttributes;

namespace Tickets.TicketsMicroservice.Models.Repositories
{
    public class ResponseGetFilteredDto<T> where T : class
    {
        public int TotalFields { get; set; }

        public IQueryable<T> Items { get; set; }
    }

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
        ///     Obtiene los elementos del repositorio de tipo <see cref="T"/> que cumplan con las condiciones de filtrado pasadas como parámetro
        /// </summary>
        /// <param name="filter">Expresión de filtrado inicial</param>
        /// <param name="searchString">Texto de búsqueda</param>
        /// <param name="orderField">Campo de ordenación</param>
        /// <param name="orderType"><see cref="OrderType"/> con la dirección de ordenación</param>
        /// <returns>Collección no modificable de elementos del tipo <see cref="T"/></returns>
        public virtual ResponseGetFilteredDto<T> GetFiltered(string searchString, Expression<Func<T, bool>> filter = null, Expression<Func<T, object>>[] includes = null)
        {
            IQueryable<T> objects = null;
            ResponseGetFilteredDto<T> response = new ResponseGetFilteredDto<T>();

            if (filter != null)
                objects = _dbSet.Where(filter).AsQueryable();
            else
                objects = _dbSet;

            if (includes != null)
                ApplyIncludes(ref objects, includes);

            List<T> temp = new List<T>();
            if (!string.IsNullOrEmpty(searchString))
            {
                //Se obtienen las propiedades por las que se puede filtrar
                var filterableProperties = typeof(T).GetProperties().Where(x => x.GetCustomAttributes(typeof(FiltersAttribute)).Any());
                if (filterableProperties != null)
                {
                    foreach (var propertyInfo in filterableProperties)
                    {
                        var customAttributeData = propertyInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(FiltersAttribute));
                        var objArguments = customAttributeData.ConstructorArguments.FirstOrDefault(x => x.ArgumentType == typeof(FilterType[])).Value;

                        IQueryable<T> tempObjects = objects.Where(PropertyContains<T>(propertyInfo, searchString));
                        temp.AddRange(tempObjects);
                    }
                    objects = temp.Distinct().AsQueryable();
                }
            }

            response.Items = objects;
            response.TotalFields = objects.Count();

            return response;
        }

        /// <summary>
        ///     Obtiene los elementos del repositorio de tipo <see cref="T"/> que cumplan con las condiciones de filtrado pasadas como parámetro
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad por la que filtrar</param>
        /// <param name="value">Valor por el que filtrar</param>
        /// <param name="filterType">Condición a aplicar</param>
        /// <returns>Collección no modificable de elementos del tipo <see cref="T"/></returns>
        public virtual IReadOnlyCollection<T> GetFiltered(string propertyName, object value, FilterType? filterType = FilterType.equals)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var filterAttributes = (FiltersAttribute)propertyInfo.GetCustomAttributes(typeof(FiltersAttribute), false).FirstOrDefault();
                if (filterAttributes != null)
                {
                    switch (filterType)
                    {

                        //Operador de igualdad (por defecto)
                        case FilterType.equals:
                            return _dbSet.Where(PropertyEquals<T>(propertyInfo, value))?.ToList()?.AsReadOnly();

                        //Operador de desigualdad
                        case FilterType.notEquals:
                            return _dbSet.Where(PropertyNotEquals<T>(propertyInfo, value))?.ToList()?.AsReadOnly();

                        //Operador menor que
                        case FilterType.lessThan:
                            return _dbSet.Where(PropertyLessThan<T>(propertyInfo, value))?.ToList()?.AsReadOnly();

                        //Operador menor o igual que
                        case FilterType.lessThanEqual:
                            return _dbSet.Where(PropertyLessThanOrEqual<T>(propertyInfo, value))?.ToList()?.AsReadOnly();

                        //Operador mayor que
                        case FilterType.greatherThan:
                            return _dbSet.Where(PropertyGreaterThan<T>(propertyInfo, value))?.ToList()?.AsReadOnly();

                        //Operador mayor o igual que
                        case FilterType.greatherThanEqual:
                            return _dbSet.Where(PropertyGreaterThanOrEqual<T>(propertyInfo, value))?.ToList()?.AsReadOnly();

                        //Operador nulo
                        case FilterType.isNullOrEmpty:
                            return _dbSet.Where(PropertyEquals<T>(propertyInfo, value)).ToList()?.AsReadOnly(); //Todo:

                        //Operador contiene
                        case FilterType.contains:
                            //Si el valor es nulo, esta en blanco o solo contiene espacios se devuelven todos los elementos
                            if (value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString()))
                                return _dbSet.AsNoTracking().ToList().AsReadOnly();

                            var stringValue = value.ToString()?.ToUpper();
                            //Todo: Solo funciona con strings
                            return _dbSet.Where(PropertyContains<T>(propertyInfo, stringValue)).ToList()?.AsReadOnly();
                    }
                }
                throw new ArgumentException(string.Format("El campo {0} no posee el tipo de filtrado {1}", propertyName, filterType));
            }
            throw new ArgumentException(string.Format("El campo {0} no existe en el modelo {1}", propertyName, typeof(T).FullName));
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
        ///     Ordena los elementos pasados como parámetro en función de la propiedad y si es descendente o no
        /// </summary>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad contiene valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyContains<TItem>(PropertyInfo property, string value)
        {
            //Se obtiene la propiedad para usarla en la expresión
            var parameterExp = Expression.Parameter(typeof(TItem));
            var propertyExp = Expression.Property(parameterExp, property.Name);

            //Se obtiene el método "Contains" de la clase string
            MethodInfo methodContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constantValue = Expression.Constant(value, typeof(string));

            //Se genera la expresión a través del nombre de la propiedad, el método a ejectar y el valor pasado como parámetro
            var containsMethodExp = Expression.Call(propertyExp, methodContains, constantValue);

            return Expression.Lambda<Func<TItem, bool>>(containsMethodExp, parameterExp);
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad == valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyEquals<TItem>(PropertyInfo property, object value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.Equal(Expression.Property(param, property), Expression.Constant(value));

            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad != valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyNotEquals<TItem>(PropertyInfo property, object value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.NotEqual(Expression.Property(param, property), Expression.Constant(value));

            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad < valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyLessThan<TItem>(PropertyInfo property, object value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.LessThan(Expression.Property(param, property), Expression.Constant(value));

            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad <= valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyLessThanOrEqual<TItem>(PropertyInfo property, object value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.LessThanOrEqual(Expression.Property(param, property), Expression.Constant(value));

            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad > valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyGreaterThan<TItem>(PropertyInfo property, object value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.GreaterThan(Expression.Property(param, property), Expression.Constant(value));

            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }

        /// <summary>
        ///     Obtiene una expresión binaria (propiedad >= valor) utilizable en linq
        /// </summary>
        /// <typeparam name="TItem">Tipo de la propiedad</typeparam>
        /// <param name="property">Propiedad del valor</param>
        /// <param name="value">Valor</param>
        /// <returns>Expresión binaria generada</returns>
        private static Expression<Func<TItem, bool>> PropertyGreaterThanOrEqual<TItem>(PropertyInfo property, object value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.GreaterThanOrEqual(Expression.Property(param, property), Expression.Constant(value));

            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }

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
