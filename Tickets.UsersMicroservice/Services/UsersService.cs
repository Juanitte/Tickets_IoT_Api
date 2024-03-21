using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using Tickets.UsersMicroservice.Models.Dtos.CreateDto;
using Tickets.UsersMicroservice.Models.Dtos.EntityDto;
using Tickets.UsersMicroservice.Models.Dtos.FilterDto;
using Tickets.UsersMicroservice.Models.Dtos.ResponseDto;
using Tickets.UsersMicroservice.Models.Entities;
using Tickets.UsersMicroservice.Models.UnitsOfWork;
using Tickets.UsersMicroservice.Translations;

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
        Task<bool> Login(LoginDto loginDto, bool? rememberUser = false);

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <returns></returns>
        Task<List<User>> GetAll();

        /// <summary>
        ///     Obtiene un usuario según su nombre de usuario
        /// </summary>
        /// <param name="userName"></param>
        /// <returns><see cref="User"/></returns>
        Task<User> GetByUserName(string userName);

        /// <summary>
        ///     Obtiene un usuario según su email
        /// </summary>
        /// <param name="email"></param>
        /// <returns><see cref="User"/></returns>
        Task<User> GetByEmail(string email);

        /// <summary>
        ///     Obtiene un usuario según su id
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="User"/></returns>
        Task<User> GetById(int id);

        /// <summary>
        ///     Elimina el usuario con el id pasado como parámetro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CreateEditRemoveResponseDto> Remove(int id);

        /// <summary>
        ///     Actualiza los datos del usuario cuyo id se pasa como parámetro
        /// </summary>
        /// <param name="ioTUser"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IdentityResult> Update(int userId, User updatedUser);

        /// <summary>
        ///     Cambia el idioma al usuario pasado como parámetro
        /// </summary>
        /// <param name="changeLanguage"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> ChangeLanguage(ChangeLanguageDto changeLanguage, int userId);

        /// <summary>
        ///     Obtiene el rol del usuario con el id pasado como parámetro
        /// </summary>
        /// <param name="userId"></param>
        /// <returns><see cref="RoleDto"/></returns>
        Task<RoleDto> GetRoleByUserId(int userId);

        /// <summary>
        ///     Método que envía un email
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        Task<bool> SendMail(MailDataDto mail);
    }
    public sealed class UsersService : BaseService, IUsersService
    {
        #region Miembros privados

        private IdentitiesService _identitiesService;

        #endregion

        #region Constructores

        public UsersService(IoTUnitOfWork ioTUnitOfWork, ILogger logger) : base(ioTUnitOfWork, logger)
        {
        }

        public UsersService(IoTUnitOfWork ioTUnitOfWork, ILogger logger, IIdentitiesService identitiesService) : base(ioTUnitOfWork, logger)
        {
            _identitiesService = (IdentitiesService)identitiesService;
        }

        public UsersService(IPrincipal user, IoTUnitOfWork ioTUnitOfWork, ILogger logger) : base (user, ioTUnitOfWork, logger)
        {
        }

        #endregion

        #region Implementación IUsersService

        /// <summary>
        ///     Cambio de idioma al usuario pasado como parámetro
        /// </summary>
        /// <param name="changeLanguage"><see cref="ChangeLanguageDto"/> con los datos del nuevo idioma</param>
        /// <param name="userId">El id del usuario</param>
        /// <returns></returns>
        public async Task<bool> ChangeLanguage(ChangeLanguageDto changeLanguage, int userId)
        {
            try
            {
                var userDb = await _unitOfWork.UsersRepository.Get(userId);
                if(userDb != null)
                {
                    userDb.Language = changeLanguage.LanguageId;
                    _unitOfWork.UsersRepository.Update(userDb);
                    await _unitOfWork.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("UsersService.ChangeLanguage => ", changeLanguage.LanguageId, e);
                throw;
            }
        }

        /// <summary>
        ///     Crea un token para un usuario y un propósito específicos
        /// </summary>
        /// <param name="userId">El id del usuario</param>
        /// <param name="purpose">El propósito</param>
        /// <returns>Token</returns>
        public async Task<string> CreatePurposeToken(int userId, string purpose)
        {
            try
            {
                var user = await _unitOfWork.UsersRepository.Get(userId);
                return await _identitiesService.GetPurposeToken(user, purpose);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        ///     Crea un token para reestablecer la contraseña
        /// </summary>
        /// <param name="userId">el id del usuario</param>
        /// <returns>Token</returns>
        public async Task<string> CreateTokenPassword(int userId)
        {
            try
            {
                var user = await _unitOfWork.UsersRepository.Get(userId);
                return await _identitiesService.GetTokenPassword(user);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todos los usuarios
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetAll()
        {
            try
            {
                return await _unitOfWork.UsersRepository.GetAll().ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("UsersService.GetAll => ", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el usuario según el email
        /// </summary>
        /// <param name="email">El email</param>
        /// <returns><see cref="User"/> con los datos del usuario</returns>
        public async Task<User> GetByEmail(string email)
        {
            try
            {
                return await Task.FromResult(_unitOfWork.UsersRepository.GetFirst(g => g.Email.Equals(email)));
            }
            catch (Exception e)
            {
                _logger.LogError("UsersService.GetByUserName =>", email, e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el usuario según su id
        /// </summary>
        /// <param name="id">El id del usuario</param>
        /// <returns><see cref="User"/> con los datos del usuario</returns>
        public async Task<User> GetById(int id)
        {
            try
            {
                return await Task.FromResult(_unitOfWork.UsersRepository.GetFirst(g => g.Id.Equals(id)));
            }
            catch (Exception e)
            {
                _logger.LogError("UsersService.GetByUserName =>", id, e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el usuario según su nombre de usuario
        /// </summary>
        /// <param name="userName">El nombre de usuario</param>
        /// <returns><see cref="User"/> con los datos del usuario</returns>
        public async Task<User> GetByUserName(string userName)
        {
            try
            {
                return await Task.FromResult(_unitOfWork.UsersRepository.GetFirst(g => g.UserName.Equals(userName)));
            }
            catch(Exception e)
            {
                _logger.LogError("UsersService.GetByUserName =>", userName, e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el rol de un usuario según su id
        /// </summary>
        /// <param name="userId">El id del usuario</param>
        /// <returns><see cref="RoleDto"/> con los datos del rol</returns>
        public async Task<RoleDto> GetRoleByUserId(int userId)
        {
            try
            {
                var user = await _unitOfWork.UsersRepository.Get(userId);
                if(user != null)
                {
                    var roleName = _identitiesService.GetUserRoles(user).Result.FirstOrDefault();
                    var roleDb = _unitOfWork.RolesRepository.GetFirst(g => g.Name == roleName);

                    return new RoleDto()
                    {
                        Id = Convert.ToInt32(roleDb.Id),
                        Name = roleDb.Name
                    };
                }
                return new RoleDto();
            }
            catch(Exception e)
            {
                _logger.LogError("UsersService.GetRoleByUserId => ", userId, e);
                throw;
            }
        }

        /// <summary>
        ///     Realización del login de la aplicación
        /// </summary>
        /// <param name="loginDto"><see cref="LoginDto"/> con los datos de inicio de sesión</param>
        /// <param name="rememberUser"></param>
        /// <returns></returns>
        public async Task<bool> Login(LoginDto loginDto, bool? rememberUser = false)
        {
            try
            {
                List<User> usersDb = await _unitOfWork.UsersRepository.GetAll().ToListAsync();
                User userDb = new User();

                foreach(var user in usersDb)
                {
                    if (user.Email.Equals(loginDto.Email))
                    {
                        userDb = user;
                    }
                }
                if (userDb != new User())
                {

                    var login = await _identitiesService.Login(userDb, loginDto.Password, rememberUser.Value);
                    if (login)
                    {
                        await _unitOfWork.SaveChanges();
                    }
                    return login;
                }
                return false;
            }
            catch(UserLockedException)
            {
                throw;
            }
            catch (UserSessionNotValidException)
            {
                throw;
            }
            catch (UserNotFoundException)
            {
                throw;
            }
            catch (PasswordNotValidException)
            {
                throw;
            }
            catch (UserWithoutPermissionException)
            {
                throw;
            }
            catch(Exception e)
            {
                _logger.LogError("UsersService.Login", e);
                return false;
            }
        }

        /// <summary>
        ///     Elimina a un usuario con el id pasado como parámetro
        /// </summary>
        /// <param name="id">El id del usuario</param>
        /// <returns></returns>
        public async Task<CreateEditRemoveResponseDto> Remove(int id)
        {
            try
            {
                var response = new CreateEditRemoveResponseDto();

                var user = _unitOfWork.UsersRepository.GetFirst(g => g.Id == id);

                if(user != null)
                {
                    await _unitOfWork.UsersRepository.Remove(id);
                    await _unitOfWork.SaveChanges();
                }
                else
                {
                    response.Errors = new List<string> { String.Format(Translation_UsersRoles.ID_no_found_description, id) };
                }
                response.Id = id;
                return response;
            }
            catch(Exception e)
            {
                _logger.LogError("UsersService.Remove => ", id);
                throw;
            }
        }

        public Task<bool> SendMail(MailDataDto mail)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Actualiza los datos de un usuario en la base de datos
        /// </summary>
        /// <param name="ioTUser"><see cref="CreateUserDto"/> con los nuevos datos de usuario</param>
        /// <param name="userId">El id del usuario</param>
        /// <returns>Una lista de errores</returns>
        public async Task<IdentityResult> Update(int userId, User updatedUser)
        { 

            _unitOfWork.UsersRepository.Update(updatedUser);
            await _unitOfWork.SaveChanges();
            return IdentityResult.Success;
        }

        /// <summary>
        ///     Valida un token para un usuario y un propósito específicos
        /// </summary>
        /// <param name="userId">El id del usuario</param>
        /// <param name="purpose">El propósito</param>
        /// <param name="token">El token</param>
        /// <returns></returns>
        public async Task<bool> ValidateUserToken(int userId, string purpose, string token)
        {
            try
            {
                var user = await _unitOfWork.UsersRepository.Get(userId);
                return await _identitiesService.VerifyUserToken(user, purpose, token);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion

        #region Métodos privados

        /// <summary>
        ///     Valida la creación de un usuario
        /// </summary>
        /// <param name="ioTUser"><see cref="CreateUserDto"/> con los datos de creación de usuario</param>
        /// <returns>Lista de errores</returns>
        private async Task<List<string>> ValidateUser(CreateUserDto ioTUser)
        {
            List<string> errorMessages = new List<string>();

            //Verificar nombre de usuario único
            var userNameUser = await _unitOfWork.UsersRepository.Any(a => a.UserName == ioTUser.UserName);
            if (userNameUser)
            {
                errorMessages.Add(string.Format(Translation_UsersRoles.NotAvailable_Username, ioTUser.UserName));
            }

            //Verificar email único
            var emailUser = await _unitOfWork.UsersRepository.Any(a => a.Email == ioTUser.Email);
            if (emailUser)
            {
                errorMessages.Add(string.Format(Translation_UsersRoles.NotAvailable_Email, ioTUser.Email));
            }

            return errorMessages;
        }

        #endregion

        #region Excepciones particulares del servicio

        public class UserApiException : Exception { }
        public class UserNotFoundException : Exception { }
        public class PasswordNotValidException : Exception { }
        public class UserLockedException : Exception { }
        public class UserWithoutVerificationException : Exception { }
        public class UserSessionNotValidException : Exception { }
        public class UserWithoutPermissionException : Exception { }

        #endregion
    }
}
