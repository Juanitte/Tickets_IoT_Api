using Microsoft.AspNetCore.Identity;
using Tickets.TicketsMicroservice.Models.Entities;
using Tickets.UsersMicroservice.Models.Dtos;

namespace Tickets.UsersMicroservice.Models.Entities
{
    /// <summary>
    ///     Definición de los usuarios de la aplicación
    /// </summary>
    public class User : IdentityUser<int>
    {
        #region Constructores

        /// <summary>
        ///     Constructor por defecto
        /// </summary>
        public User()
        {
            this.tickets = new List<Ticket?>();
        }

        #endregion

        public List<Ticket?> tickets { get; set; }
    }
}
