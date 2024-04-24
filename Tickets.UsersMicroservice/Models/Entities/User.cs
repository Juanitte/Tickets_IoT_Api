using Common.Utilities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Security.Principal;

namespace Tickets.UsersMicroservice.Models.Entities
{
    /// <summary>
    ///     Definición de los usuarios de la aplicación
    /// </summary>
    [Table("Users")]
    public class User : IdentityUser<int>
    {
        #region Constructores

        /// <summary>
        ///     Constructor por defecto
        /// </summary>
        public User()
        {
            FullName = string.Empty;
            Language = Language.English;
        }

        #endregion

        public string FullName { get; set; }
        public Language Language { get; set; }
        public string Role { get; set; }
    }

    /// <summary>
    ///     Definición de los roles de la aplicación
    /// </summary>
    public class Role : IdentityRole
    {
        /// <summary>
        ///     Descripción del rol
        /// </summary>
        [StringLength(120)]
        public string Description { get; set; }

        /// <summary>
        ///     Nivel del rol
        /// </summary>
        public int Level { get; set; }
    }

    #region Extensiones
    /// <summary>
    ///     Clase estática que que extiende los métodos al usuario logeado,
    ///     que obtienen datos de los Claims que se le han asignado al usuario
    /// </summary>
    public static class UserExtended
    {
        /// <summary>
        ///     Obtiene el idioma del usuario logeado
        /// </summary>
        /// <param name="user"><see cref="IPrincipal"/> con los datos del usuario logeado</param>
        /// <returns>id del idioma del usuario</returns>
        public static int GetLanguageId(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(Literals.Claim_LanguageId);
            return claim == null ? 0 : Convert.ToInt32(claim.Value);
        }

        /// <summary>
        ///     Obtiene el rol del usuario logeado
        /// </summary>
        /// <param name="user"><see cref="IPrincipal"/> con los datos del usuario logeado</param>
        /// <returns>Cadena con el nombre del rol del usuario</returns>
        public static string GetRole(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(Literals.Claim_Role);
            return claim?.Value;
        }

        /// <summary>
        ///     Obtiene el id del usuario logeado
        /// </summary>
        /// <param name="user"><see cref="IPrincipal"/> con los datos del usuario logeado</param>
        /// <returns>El id del usuario</returns>
        public static int GetUserId(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(Literals.Claim_UserId);
            return claim == null ? 0 : Convert.ToInt32(claim.Value);
        }

    }

    #endregion

}
