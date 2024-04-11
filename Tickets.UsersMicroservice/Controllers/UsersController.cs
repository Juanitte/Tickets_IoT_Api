using Common.Dtos;
using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Cryptography;
using System.Text;
using Tickets.UsersMicroservice.Models.Dtos.CreateDto;
using Tickets.UsersMicroservice.Models.Dtos.EntityDto;
using Tickets.UsersMicroservice.Models.Entities;
using Tickets.UsersMicroservice.Translations;

namespace Tickets.UsersMicroservice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : BaseController
    {
        #region Miembros privados

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<User> _userManager;

        #endregion

        #region Constructores

        public UsersController(IServiceProvider serviceCollection, IWebHostEnvironment hostingEnvironment, UserManager<User> userManager) : base(serviceCollection)
        {
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        ///     Método que obtiene todos los usuarios
        /// </summary>
        /// <returns></returns>
        [HttpGet("users/getall")]
        public async Task<JsonResult> GetAll()
        {
            try
            {
                var users = await IoTServiceUsers.GetAll();
                return new JsonResult(users);
            }
            catch (Exception e)
            {
                return new JsonResult(new List<UserDto>());
            }
        }

        /// <summary>
        ///     Método que obtiene un usuario según su id
        /// </summary>
        /// <param name="userId">El id del usuario a buscar</param>
        /// <returns></returns>
        [HttpGet("users/getbyid/{id}")]
        public async Task<JsonResult> GetById(int id)
        {
            try
            {
                var user = await IoTServiceUsers.GetById(id);
                return new JsonResult(user);
            }
            catch (Exception e)
            {
                return new JsonResult(new UserDto());
            }
        }

        /// <summary>
        ///     Método que crea un nuevo usuario con rol SupportManager
        /// </summary>
        /// <param name="user"><see cref="CreateUserDto"/> con los datos del usuario</param>
        /// <returns></returns>
        [HttpPost("users/create/manager")]
        public async Task<IActionResult> CreateManager(CreateUserDto userDto)
        {

            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                Language = userDto.Language,
                FullName = userDto.FullName
            };

            string password = HashPassword("IoT@2024");

            var createUser = await _userManager.CreateAsync(user, password);

            if (!createUser.Succeeded)
            {
                var errorMessage = string.Join(", ", createUser.Errors.Select(error => error.Description));
                return BadRequest(errorMessage);
            }

            await _userManager.AddToRoleAsync(user, "SupportManager");

            return Ok(createUser);
        }

        /// <summary>
        ///     Método que crea un nuevo usuario con rol SupportTechnician
        /// </summary>
        /// <param name="user"><see cref="CreateUserDto"/> con los datos del usuario</param>
        /// <returns></returns>
        [HttpPost("users/create/technician")]
        public async Task<IActionResult> CreateTechnician(CreateUserDto userDto)
        {

            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                Language = userDto.Language,
                FullName = userDto.FullName
            };

            string password = HashPassword("IoT@2024");

            var createUser = await _userManager.CreateAsync(user, password);

            if (!createUser.Succeeded)
            {
                var errorMessage = string.Join(", ", createUser.Errors.Select(error => error.Description));
                return BadRequest(errorMessage);
            }

            await _userManager.AddToRoleAsync(user, "SupportTechnician");

            return Ok(createUser);
        }

        /// <summary>
        ///     Método que actualiza un usuario con id proporcionado como parámetro
        /// </summary>
        /// <param name="userId">El id del usuario a editar</param>
        /// <param name="user"><see cref="CreateUserDto"/> con los nuevos datos de usuario</param>
        /// <returns></returns>
        [HttpPut("users/update/{userId}")]
        public async Task<IActionResult> Update(int userId, CreateUserDto userDto)
        {
            User user = await IoTServiceUsers.GetById(userId);
            if (user == null)
            {
                return BadRequest();
            }
            user.FullName = userDto.FullName;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.UserName = userDto.UserName;

            var result = await IoTServiceUsers.Update(userId, user);

            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return Problem("Error updating user.");
            }
        }

        /// <summary>
        ///     Método que cambia el idioma al usuario con id pasado como parámetro
        /// </summary>
        /// <param name="userId">El id del usuario a modificar</param>
        /// <param name="changeLanguage"><see cref="ChangeLanguageDto"/> con los datos del idioma</param>
        /// <returns></returns>
        [HttpPut("users/changelanguage/{userId}")]
        public async Task<IActionResult> ChangeLanguage(int userId, ChangeLanguageDto changeLanguage)
        {
            var response = new GenericResponseDto();
            try
            {
                await IoTServiceUsers.ChangeLanguage(changeLanguage, userId);
                response.ReturnData = true;
            }
            catch (Exception e)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = e.Message, Location = "Users/ChangeLanguage" };
                return Ok(response);
            }
            return Ok(response);
        }

        /// <summary>
        ///     Método que elimina un usuario cuyo id se ha pasado como parámetro
        /// </summary>
        /// <param name="id">el id del usuario a eliminar</param>
        /// <returns></returns>
        [HttpDelete("users/remove/{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var response = new GenericResponseDto();
            try
            {
                var result = await IoTServiceUsers.Remove(id);
                if (result.Errors != null && result.Errors.Any())
                {
                    response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = result.Errors.ToList().ToDisplayList(), Location = "Users/Remove" };
                }
            }
            catch (Exception e)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = e.Message, Location = "Users/Remove" };
            }
            return Ok(response);
        }

        /// <summary>
        ///     Obtiene los usuarios con rol SupportTechnician.
        /// </summary>
        /// <returns></returns>
        [HttpGet("users/gettechnicians")]
        public async Task<JsonResult> GetTechnicians()
        {
            try
            {
                var result = new List<User>();
                var users = await IoTServiceUsers.GetAll();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault();
                    if (role.Equals("SupportTechnician"))
                    {
                        result.Add(user);
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return new JsonResult(new UserDto());
            }
        }

        /// <summary>
        ///     Envía un correo de restablecer contraseña al email proporcionado
        /// </summary>
        /// <param name="username">el nombre del correo</param>
        /// <param name="domain">el dominio del correo (ej. 'gmail')</param>
        /// <param name="tld">el final del correo (ej. '.com')</param>
        /// <returns></returns>
        [HttpGet("users/sendemail/{username}/{domain}/{tld}")]
        public async Task<IActionResult> SendEmail(string username, string domain, string tld)
        {
            try
            {
                var email = string.Concat(username, "@", domain, ".", tld);
                Console.WriteLine(email);
                var user = await IoTServiceUsers.GetByEmail(email);
                Console.WriteLine(user.Email);
                string hashedEmail = Hash(email);
                if (user != null)
                {
                    IoTServiceUsers.SendMail(email, string.Concat("http://localhost:4200/recuperar/", hashedEmail, "/", username, "/", domain, "/", tld));
                    return Ok();
                }

                return BadRequest(user);
            } catch (Exception e)
            {
                return BadRequest();
            }
        }

        /// <summary>
        ///     Restablece la contraseña de un usuario
        /// </summary>
        /// <param name="resetPassword"><see cref="ResetPasswordDto"/> con los datos de restablecimiento de contraseña</param>
        /// <returns></returns>
        [HttpPost("users/resetpassword")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto resetPass)
        {
            try
            {
                var email = string.Concat(resetPass.Username, "@", resetPass.Domain, ".", resetPass.Tld);
                Console.WriteLine(email);
                var user = await IoTServiceUsers.GetByEmail(email);
                Console.WriteLine(user != null);
                if (user != null)
                {
                    if(await IoTServiceIdentity.UpdateUserPassword(user, resetPass.Password))
                    {
                        return Ok();
                    }
                }
                return BadRequest();
            }catch (Exception e)
            {
                return BadRequest();
            }
        }


        #endregion

        #region Métodos Privados

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

        /// <summary>
        ///     Hashea una contraseña igual que el frontend
        /// </summary>
        /// <param name="password">la contraseña a hashear</param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return string.Concat(builder.ToString(), "@", "A", "a");
            }
        }

        #endregion
    }
}
