using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
