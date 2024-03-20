using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dtos
{
    /// <summary>
    ///     Clase genérica de respuesta de los servicios
    /// </summary>
    [Serializable]
    public class GenericResponseDto
    {
        #region Miembros privados

        private bool isValid;
        private GenericErrorDto error;

        #endregion

        #region Constructores

        /// <summary>
        ///     Constructor por defecto
        /// </summary>
        public GenericResponseDto()
        {
            isValid = true;
            error = new GenericErrorDto();
        }

        #endregion

        #region Propiedades

        /// <summary>
        ///     Propiedad que indica si han habido errores
        /// </summary>
        public bool IsValid { get { return isValid; } }

        public string? Id { get; set; }

        /// <summary>
        ///     Error a devolver al usuario <see cref="GenericErrorDto"/>
        /// </summary>
        public GenericErrorDto Error
        { 
            get {  return error; }
            set
            {
                if(value != null && value.Id != 0)
                {
                    error = value;
                    isValid = false;
                }
            }
        }

        /// <summary>
        ///     Método encargado de determinar si se debe serializar o no la propiedad error
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeError()
        {
            return !IsValid;
        }

        /// <summary>
        ///     Objeto de respuesta
        /// </summary>
        public object ReturnData { get; set; }

        #endregion
    }
}
