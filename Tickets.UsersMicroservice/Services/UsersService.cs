using Common.Utilities;
using Duende.IdentityServer.Extensions;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
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
        Task<List<UserDto>> GetAll();

        /// <summary>
        ///     Obtiene un usuario según su nombre de usuario
        /// </summary>
        /// <param name="userName"></param>
        /// <returns><see cref="UserDto"/></returns>
        Task<UserDto> GetByUserName(string userName);

        /// <summary>
        ///     Obtiene un usuario según su email
        /// </summary>
        /// <param name="email"></param>
        /// <returns><see cref="User"/></returns>
        Task<UserDto> GetByEmail(string email);

        /// <summary>
        ///     Obtiene un usuario según su id
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="User"/></returns>
        Task<UserDto> GetById(int id);

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
        Task<IdentityResult> Update(int userId, CreateUserDto userDto);

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
        /// <param name="username">el nombre del correo</param>
        /// <param name="domain">el dominio del correo (ej. 'gmail')</param>
        /// <param name="tld">el final del correo (ej. '.com')</param>
        /// <returns></returns>
        void SendMail(string username, string domain, string tld);

        /// <summary>
        ///     Obtiene todos los usuarios con SupportTechnician como rol.
        /// </summary>
        /// <returns>Una lista de <see cref="UserDto"/> con todos los técnicos</returns>
        Task<List<UserDto>> GetTechnicians();

        /// <summary>
        ///     Restablece la contraseña de un usuario
        /// </summary>
        /// <param name="resetPassword"><see cref="ResetPasswordDto"/> con los datos de restablecimiento de contraseña</param>
        /// <returns></returns>
        Task<User> ResetPassword(ResetPasswordDto resetPass);


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
                _logger.LogError(e, "UsersService.ChangeLanguage => ");
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
        public async Task<List<UserDto>> GetAll()
        {
            try
            {
                var users = await _unitOfWork.UsersRepository.GetAll().ToListAsync();
                var result = users.Select(u => Extensions.ConvertModel(u, new UserDto())).ToList();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UsersService.GetAll => ");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el usuario según el email
        /// </summary>
        /// <param name="email">El email</param>
        /// <returns><see cref="UserDto"/> con los datos del usuario</returns>
        public async Task<UserDto> GetByEmail(string email)
        {
            try
            {
                var user = await Task.FromResult(_unitOfWork.UsersRepository.GetFirst(g => g.Email.Equals(email)));
                return Extensions.ConvertModel(user, new UserDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UsersService.GetByEmail =>");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el usuario según su id
        /// </summary>
        /// <param name="id">El id del usuario</param>
        /// <returns><see cref="UserDto"/> con los datos del usuario</returns>
        public async Task<UserDto> GetById(int id)
        {
            try
            {
                var user = await Task.FromResult(_unitOfWork.UsersRepository.GetFirst(g => g.Id.Equals(id)));
                return Extensions.ConvertModel(user, new UserDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UsersService.GetByUserName =>");
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el usuario según su nombre de usuario
        /// </summary>
        /// <param name="userName">El nombre de usuario</param>
        /// <returns><see cref="UserDto"/> con los datos del usuario</returns>
        public async Task<UserDto> GetByUserName(string userName)
        {
            try
            {
                var user = await Task.FromResult(_unitOfWork.UsersRepository.GetFirst(g => g.UserName.Equals(userName)));
                return Extensions.ConvertModel(user, new UserDto());
            }
            catch(Exception e)
            {
                _logger.LogError(e, "UsersService.GetByUserName =>");
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
                _logger.LogError(e, "UsersService.GetRoleByUserId => ");
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
                _logger.LogError(e, "UsersService.Login");
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

                var user = await GetById(id);

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
                _logger.LogError(e, "UsersService.Remove => ");
                throw;
            }
        }

        /// <summary>
        ///     Actualiza los datos de un usuario en la base de datos
        /// </summary>
        /// <param name="ioTUser"><see cref="CreateUserDto"/> con los nuevos datos de usuario</param>
        /// <param name="userId">El id del usuario</param>
        /// <returns>Una lista de errores</returns>
        public async Task<IdentityResult> Update(int userId, CreateUserDto userDto)
        {
            User user = await _unitOfWork.UsersRepository.Get(userId);
            if(user == null)
            {
                return IdentityResult.Failed();
            }
            user.FullName = userDto.FullName;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.UserName = userDto.UserName;

            _unitOfWork.UsersRepository.Update(user);
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

        /// <summary>
        ///     Envía un email
        /// </summary>
        /// <param name="email">el email destino</param>
        /// <param name="link">el enlace de restablecer contraseña</param>
        /// <returns></returns>
        public async void SendMail(string username, string domain, string tld)
        {
            try
            {
                var email = string.Concat(username, "@", domain, ".", tld);
                var user = _unitOfWork.UsersRepository.GetFirst(u => u.Email == email);
                string hashedEmail = Hash(email);
                if (user != null)
                {
                    var link = string.Concat("http://localhost:4200/recuperar/", hashedEmail, "/", username, "/", domain, "/", tld);

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("IoT Incidencias", "noreply.iot.incidencias@gmail.com"));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = Translation_Account.Email_title;
                    message.Body = new TextPart("plain") { Text = string.Concat(Translation_Account.Email_body, "\n", link) };

                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                        client.Authenticate("noreply.iot.incidencias@gmail.com", "levp dwqb qacd vhle");
                        client.Send(message);
                        client.Disconnect(true);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Send Mail => ");
            }
        }

        /// <summary>
        ///     Obtiene todos los usuarios con SupportTechnician como rol.
        /// </summary>
        /// <returns>Una lista de <see cref="UserDto"/> con todos los técnicos</returns>
        public async Task<List<UserDto>> GetTechnicians()
        {
            try
            {
                var users = await _unitOfWork.UsersRepository.GetAll(user => user.Role == "SupportTechnician").ToListAsync();
                var result = users.Select(u => Extensions.ConvertModel(u, new UserDto())).ToList();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get Technicians => ");
                throw;
            }
        }

        /// <summary>
        ///     Restablece la contraseña de un usuario
        /// </summary>
        /// <param name="resetPassword"><see cref="ResetPasswordDto"/> con los datos de restablecimiento de contraseña</param>
        /// <returns></returns>
        public async Task<User> ResetPassword(ResetPasswordDto resetPass)
        {
            try
            {
                var email = string.Concat(resetPass.Username, "@", resetPass.Domain, ".", resetPass.Tld);
                return _unitOfWork.UsersRepository.GetFirst(u => u.Email == email);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Reset password => ");
                return null;
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

        #region Métodos privados

        /// <summary>
        ///     Hashea un texto
        /// </summary>
        /// <param name="text">el texto a hashear</param>
        /// <returns></returns>
        public static string Hash(string text)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #endregion
    }
}
