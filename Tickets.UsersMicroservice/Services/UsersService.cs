using Tickets.UsersMicroservice.Models.Dtos;

namespace Tickets.UsersMicroservice.Services
{

    public interface IUsersService
    {
        /// <summary>
        ///     Crea un token para reestablecer la contraseña
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <returns></returns>
        public Task<string> CreateTokenPassword(int userId);

        /// <summary>
        ///     Crea un token para un usuario y con un propósito específicos
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <param name="purpose">Propósito del token</param>
        /// <returns></returns>
        Task<string> CreatePurposeToken(int userId, string purpose);

        /// <summary>
        ///     Valida un token para un usuario y con un propósito específicos
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <param name="purpose">Propósito del token</param>
        /// <param name="token">Token a validar</param>
        /// <returns></returns>
        Task<bool> ValidateUserToken(int userId, string purpose, string token);

        /// <summary>
        ///     Realiza el login en la aplicación
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        Task<bool> Login(LoginDto loginDto);
    }
    public class UsersService
    {
    }
}
