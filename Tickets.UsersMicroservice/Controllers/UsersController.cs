using Common.Dtos;
using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
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

        #endregion

        #region Constructores

        public UsersController(IServiceProvider serviceCollection,  IWebHostEnvironment hostingEnvironment) : base(serviceCollection)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        ///     Método que obtiene todos los usuarios
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
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
        [HttpGet("getbyid/{id}")]
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
        ///     Método que crea un nuevo usuario
        /// </summary>
        /// <param name="user"><see cref="CreateUserDto"/> con los datos del usuario</param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateUserDto user)
        {
            var response = new GenericResponseDto();
            try
            {
                List<string> errors = new List<string>();

                if (ModelState.IsValid)
                {
                    try
                    {
                        var result = IoTServiceUsers.Create(user).Result;
                        if (!result.Success)
                        {
                            response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = result.Errors.ToList().ToDisplayList() };
                        }
                    }
                    catch (Exception e)
                    {
                        response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = e.Message, Location = "Users/Create" };
                    }
                }
                else
                {
                    response.Error = new GenericErrorDto() { Id = ResponseCodes.InvalidModel, Description = Translation_Errors.InvalidModelState, Location = "Users/Create" };
                }
            }
            catch (Exception e)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = e.Message, Location = "Users/Create" };
            }

            return Ok(response);
        }

        /// <summary>
        ///     Método que actualiza un usuario con id proporcionado como parámetro
        /// </summary>
        /// <param name="userId">El id del usuario a editar</param>
        /// <param name="user"><see cref="CreateUserDto"/> con los nuevos datos de usuario</param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<IActionResult> Update(int userId, CreateUserDto user)
        {
            var response = new GenericResponseDto();
            try
            {
                List<string> errors = new List<string>();

                if (ModelState.IsValid)
                {
                    try
                    {
                        var result = await IoTServiceUsers.Update(user, userId);
                    }
                    catch (Exception ex)
                    {
                        response.Error = new GenericErrorDto() { Id = ResponseCodes.DataError, Description = ex.Message, Location = "Users/Edit" };
                    }
                }
                else
                {
                    response.Error = new GenericErrorDto() { Id = ResponseCodes.InvalidModel, Description = Translation_Errors.InvalidModelState, Location = "Users/Edit" };
                }
            }
            catch (Exception ex)
            {
                response.Error = new GenericErrorDto() { Id = ResponseCodes.OtherError, Description = ex.Message, Location = "Users/Edit" };
            }

            return Ok(response);
        }

        /// <summary>
        ///     Método que cambia el idioma al usuario con id pasado como parámetro
        /// </summary>
        /// <param name="userId">El id del usuario a modificar</param>
        /// <param name="changeLanguage"><see cref="ChangeLanguageDto"/> con los datos del idioma</param>
        /// <returns></returns>
        [HttpPut("changelanguage")]
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
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var response = new GenericResponseDto();
            try
            {
                var result = await IoTServiceUsers.Remove(id);
                if(result.Errors != null && result.Errors.Any())
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



        #endregion
    }
}
