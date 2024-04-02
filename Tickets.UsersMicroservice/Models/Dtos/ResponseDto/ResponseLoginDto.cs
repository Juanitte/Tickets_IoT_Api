using Common.Utilities;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Tickets.UsersMicroservice.Models.Dtos.EntityDto;

namespace Tickets.UsersMicroservice.Models.Dtos.ResponseDto
{
    public class ResponseLoginDto
    {
        /// <summary>
        ///     Código de error de la respuesta
        /// </summary>
        [DataMember]
        [JsonProperty("error")]
        public int Error { get; set; }

        /// <summary>
        ///     Descripción de error de la respuesta
        /// </summary>
        [DataMember]
        [JsonProperty("error_description")]
        public string ErrorDescripcion { get; set; } = string.Empty;

        /// <summary>
        ///     Token de acceso obtenido
        /// </summary>
        [DataMember]
        [JsonProperty("access_token")]
        public string Token { get; set; }

        /// <summary>
        ///     Nombre del usuario logeado
        /// </summary>
        [DataMember]
        [JsonProperty("userName")]
        public string UserName { get; set; }

        /// <summary>
        ///     Identificador del usuario logeado
        /// </summary>
        [DataMember]
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        ///     Nombre completo del usuario logeado
        /// </summary>
        [DataMember]
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        /// <summary>
        ///     Teléfono del usuario logeado
        /// </summary>
        [DataMember]
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     Email del usuario logeado
        /// </summary>
        [DataMember]
        [JsonProperty("email")]
        public string Email { get; set; }

        public Guid? ClientId { get; set; }
        public Guid? CompanyId { get; set; }

        /// <summary>
        ///     Rol del usuario logeado
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        ///     Identificador del idioma del usuario logeado
        /// </summary>
        [DataMember]
        [JsonProperty("languageId")]
        public Language LanguageId { get; set; }
    }
}
