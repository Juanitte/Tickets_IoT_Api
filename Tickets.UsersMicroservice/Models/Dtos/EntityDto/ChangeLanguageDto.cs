using Common.Utilities;

namespace Tickets.UsersMicroservice.Models.Dtos.EntityDto
{
    /// <summary>
    ///     Dto de cambio de idioma
    /// </summary>
    public class ChangeLanguageDto
    {
        /// <summary>
        ///     Id del usuario
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Id del idioma
        /// </summary>
        public Language LanguageId { get; set; }
    }
}
