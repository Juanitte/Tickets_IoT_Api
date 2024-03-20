using Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dtos
{
    /// <summary>
    ///     Clase genérica de respuesta de errores
    /// </summary>
    public class GenericErrorDto
    {
        /// <summary>
        ///     Id del error <see cref="ResponseCodes"/>
        /// </summary>
        public ResponseCodes Id { get; set; }

        /// <summary>
        ///     Localización donde ha ocurrido el error
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Descripción del error
        /// </summary>
        public string Description { get; set; }
    }
}
