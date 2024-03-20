using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tickets.UsersMicroservice.Helpers;
using Tickets.UsersMicroservice.Models.Dtos.EntityDto;
using Tickets.UsersMicroservice.Models.Dtos.ResponseDto;
using Tickets.UsersMicroservice.Translations;
using static Tickets.UsersMicroservice.Services.UsersService;

namespace Tickets.UsersMicroservice.Controllers
{
    public class AuthController : BaseController
    {
        #region Constructores

        public AuthController(IServiceProvider serviceCollection) : base(serviceCollection)
        {
        }

        #endregion

        #region Métodos públicos

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(LoginDto login)
        {
            try
            {
                var user = await IoTServiceUsers.GetByEmail(login.Email);
                if (user == null)
                {
                    return BadRequest(new ResponseLoginDto() { ErrorDescripcion = Translation_Account.User_not_found });
                }

                var authenticated = await IoTServiceUsers.Login(login);
                if(!authenticated)
                {
                    return BadRequest(new ResponseLoginDto() { ErrorDescripcion = Translation_Account.Incorrect_password });
                }
                if (user.LockoutEnabled)
                {
                    return BadRequest(new ResponseLoginDto() { ErrorDescripcion = Translation_UsersRoles.User_locked_message });
                }

                var role = await IoTServiceUsers.GetRoleByUserId(user.Id);
                var claims = new ClaimsIdentity(new Claim[]
                {
                    new Claim(Literals.Claim_UserId, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(Literals.Claim_Role, role.Name),
                    new Claim(Literals.Claim_FullName, user.FullName),
                    new Claim(Literals.Claim_Email, user.Email),
                    new Claim(Literals.Claim_PhoneNumber, user.PhoneNumber)
                });

                var tokenHandler = new JwtSecurityTokenHandler();

                var appSettingsSection = Configuration.GetSection("AppSettings");

                var appSettings = appSettingsSection.Get<AppSettings>();

                var key = Encoding.ASCII.GetBytes(appSettings.Secret);

                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = IoTEncoder.EncodeString(tokenHandler.WriteToken(token));

                return Ok(new ResponseLoginDto
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    LanguageId = user.Language,
                    Role = role,
                    Token = tokenString
                });
            }
            catch (UserLockedException)
            {
                return BadRequest(new ResponseLoginDto()
                {
                    ErrorDescripcion = string.Format(Translation_UsersRoles.User_locked_message, login.Email)
                });
            }
            catch (UserSessionNotValidException)
            {
                return BadRequest(new ResponseLoginDto()
                {
                    ErrorDescripcion = Translation_Errors.Login_error
                });
            }
            catch (UserNotFoundException)
            {
                return BadRequest(new ResponseLoginDto()
                {
                    ErrorDescripcion = Translation_Account.User_not_found
                });
            }
            catch (PasswordNotValidException)
            {
                return BadRequest(new ResponseLoginDto()
                {
                    ErrorDescripcion = Translation_Account.Incorrect_password
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseLoginDto()
                {
                    ErrorDescripcion = ex.Message
                });
            }
        }

        #endregion
    }
}
