using Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Attributes
{
    public class ModelAttributes
    {
        /// <summary>
        ///     Atributo de campo de modelo con los posibles tipos de filtrado que tiene
        /// </summary>
        public class FiltersAttribute : Attribute
        {
            /// <summary>
            ///     Tipos de filtros aceptados
            /// </summary>
            public FilterType[] FilterTypes;

            /// <summary>
            ///     Constructor parametrizado
            /// </summary>
            public FiltersAttribute(params FilterType[] filterTypes)
            {
                FilterTypes = filterTypes;
            }
        }

        /// <summary>
        ///     Atributo de campo de modelo que determina que es un campo por el cual se puede ordenar su entidad
        /// </summary>
        public class SortableAttribute : Attribute
        {
            /// <summary>
            ///     Constructor por defecto
            /// </summary>
            public SortableAttribute()
            {
            }
        }

        /// <summary>
        ///     Atributo de campo de modelo que determina que es un campo por el cual se puede ordenar su entidad
        /// </summary>
        public class ImportAttribute : Attribute
        {
            /// <summary>
            ///     Constructor por defecto
            /// </summary>
            public ImportAttribute()
            {

            }
        }

        public class ValidEnumAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value == null)
                {
                    return ValidationResult.Success;
                }

                var type = value.GetType();

                if (!(type.IsEnum && Enum.IsDefined(type, value)))
                {
                    return new ValidationResult(ErrorMessage ?? $"{value} is not a valid value for type {type.Name}");
                }

                return ValidationResult.Success;
            }
        }
    }
}
