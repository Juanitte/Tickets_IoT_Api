
using System.Reflection;
using static Common.Attributes.ModelAttributes;

namespace Common.Utilities
{
    public static class Extensions
    {
        /// <summary>
        ///     Mapea un modelo de base de datos de tipo <see cref="object"/> a un DTO de tipo <see cref="X"/>
        /// </summary>
        /// <returns></returns>
        public static X ConvertModel<X>(this object input, X output) where X : class, new()
        {
            if (input != null)
            {
                var modelProperties = input.GetType().GetProperties().ToList();
                foreach (var property in output.GetType().GetProperties().ToList())
                {
                    var prop = modelProperties.FirstOrDefault(p => p.Name == property.Name);
                    if (prop != null)
                    {
                        try
                        {
                            property.SetValue(output, prop.GetValue(input));
                        }
                        catch { }
                    }
                }
            }
            return output;
        }

        /// <summary>
        ///     Convierte una lista de cadenas, en una cadena separada por saltos de línea
        /// </summary>
        /// <param name="values">Valores a concatenar</param>
        /// <returns></returns>
        public static string ToDisplayList(this List<string> values)
        {
            var response = string.Empty;
            if (values.Count > 1)
            {
                response = string.Join(Environment.NewLine, values);
            }
            else if (values.Count == 1)
                response = values[0];

            return response;
        }

        /// <summary>
        ///     Obtiene los campos ordenables
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<string> GetSorteableFields<T>()
        {
            var properties = typeof(T).GetProperties().Where(x => x.GetCustomAttributes(typeof(SortableAttribute)).Any());

            return properties.Select(s => s.Name);
        }

        /// <summary>
        ///     Obtiene los campos filtrables
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<string> GetFilterablesFields<T>()
        {
            var properties = typeof(T).GetProperties().Where(x => x.GetCustomAttributes(typeof(FiltersAttribute)).Any());

            return properties.Select(s => s.Name);
        }
    }
}
