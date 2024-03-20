using Common.Utilities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Tickets.UsersMicroservice.Models.Dtos.EntityDto;
using Tickets.UsersMicroservice.Models.Entities;
using Tickets.UsersMicroservice.Models.UnitsOfWork;
using static Tickets.UsersMicroservice.Services.UsersService;

namespace Tickets.UsersMicroservice.Services
{
    /// <summary>
    ///     Interfaz que define los métodos referentes al usuario actual
    /// </summary>
    public interface IIdentitiesService
    {
        /// <summary>
        ///     Método para el inicio de sesión del usuario en la aplicación
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="rememberUser"></param>
        /// <returns></returns>
        Task<bool> Login(string email, string password, bool rememberUser);

        /// <summary>
        ///     Crea un nuevo usuario en el sistema
        /// </summary>
        /// <param name="user"><see cref="User"/></param>
        /// <param name="roleName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<IdentityResult> CreateUser(User user, string roleName, string password);

        /// <summary>
        ///     Cierra la sesión del usuario
        /// </summary>
        /// <returns></returns>
        Task Logoff();

        /// <summary>
        ///     Obtiene un token de verificación de email
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<string> GetTokenPassword(User user);

        /// <summary>
        ///     Crea un nuevo rol
        /// </summary>
        /// <param name="roleDto"></param>
        /// <returns></returns>
        Task<IdentityResult> CreateRole(RoleDto roleDto);

        /// <summary>
        ///     Obtiene los roles del usuario pasado como parámetro
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IList<string>> GetUserRoles(User user);

        Task<User> DefaultUser();
    }

    /// <summary>
    ///     Implementación de la interfaz IIdentitiesService<see cref="IIdentitiesService"/>
    /// </summary>
    public class IdentitiesService : BaseService, IIdentitiesService
    {
        #region Miembros privados

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="ioTUnitOfWork"></param>
        /// <param name="logger"></param>
        public IdentitiesService(UserManager<User> userManager,
            SignInManager<User> signInManager, RoleManager<Role> roleManager,
            IoTUnitOfWork ioTUnitOfWork, ILogger logger) : base(ioTUnitOfWork, logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;

            DefaultRoles().Wait();
            DefaultUser().Wait();
        }

        #endregion

        #region Implementación interfaz IIdentitiesService

        /// <summary>
        ///     Método para el inicio de sesión del usuario de la aplicación
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="rememberUser">Define si se debe mantener la sesión iniciada en el equipo</param>
        /// <returns>Resultado de la operación</returns>
        /// <exception cref="UserLockedException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="PasswordNotValidException"></exception>
        public async Task<bool> Login(string email, string password, bool rememberUser)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberUser, lockoutOnFailure: true);
                if(result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    var roles = await GetUserRoles(user);

                    await SetUserClaims(user);

                    return true;
                }
                else if(result.IsLockedOut)
                {
                    throw new UserLockedException();
                }
                else
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if(user == null)
                    {
                        throw new UserNotFoundException();
                    }
                    else
                    {
                        throw new PasswordNotValidException();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "IdentitiesService.Login");
                throw;
            }
        }

        /// <summary>
        ///     Crea un nuevo usuario en el sistema
        /// </summary>
        /// <param name="user">Datos del usuario</param>
        /// <param name="roleName">Rol del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Resultado de la operación</returns>
        public async Task<IdentityResult> CreateUser(User user, string roleName, string password)
        {
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if(result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                    return roleResult;
                }
                return result;
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.CreateUser", e);
                throw;
            }
        }

        /// <summary>
        ///     Cierra la sesión actual del usuario
        /// </summary>
        /// <returns></returns>
        public async Task Logoff()
        {
            try
            {
                await _signInManager.SignOutAsync();
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.Logoff", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene el token para resetear la contraseña del usuario
        /// </summary>
        /// <param name="user">El usuario</param>
        /// <returns>El token</returns>
        public async Task<string> GetTokenPassword(User user)
        {
            try
            {
                return await _userManager.GeneratePasswordResetTokenAsync(user);
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.GetTokenPassword", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene un token para un usuario y un propósito específicados
        /// </summary>
        /// <param name="user">El usuario</param>
        /// <param name="purpose">El propósito</param>
        /// <returns>El token</returns>
        public async Task<string> GetPurposeToken(User user, string purpose)
        {
            try
            {
                return await _userManager.GenerateUserTokenAsync(user, "Default", purpose);
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.GetPurposeToken", e);
                throw;
            }
        }

        /// <summary>
        ///     Valida un token para un usuario y propósito especificados
        /// </summary>
        /// <param name="user">El usuario</param>
        /// <param name="purpose">El propósito</param>
        /// <param name="token">El token</param>
        /// <returns></returns>
        public async Task<bool> VerifyUserToken(User user, string purpose, string token)
        {
            try
            {
                return await _userManager.VerifyUserTokenAsync(user, "Default", purpose, token);
            }
            catch (Exception e)
            {
                _logger.LogError("IdentitiesService.VerifyUserToken", e);
                throw;
            }
        }

        /// <summary>
        ///     Crea un nuevo rol
        /// </summary>
        /// <param name="roleDto"><see cref="RoleDto"/></param>
        /// <returns></returns>
        public async Task<IdentityResult> CreateRole(RoleDto roleDto)
        {
            try
            {
                var role = new Role()
                {
                    Name = roleDto.Name,
                    Description = roleDto.Description
                };
                return await _roleManager.CreateAsync(role);
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.CreateRole", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene los roles del usuario pasado como parámetro
        /// </summary>
        /// <param name="user"><see cref="User"/> con los datos del usuario</param>
        /// <returns>Roles del usuario</returns>
        public async Task<IList<string>> GetUserRoles(User user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.GetUserRoles", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene todos los usuarios que pertenezcan al rol pasado como parámetro
        /// </summary>
        /// <param name="roleName">El nombre del rol</param>
        /// <returns>Los usuarios pertenecientes a ese rol</returns>
        public async Task<IList<User>> GetUsersByRole(string roleName)
        {
            try
            {
                return await _userManager.GetUsersInRoleAsync(roleName);
            }
            catch (Exception e)
            {
                _logger.LogError("IdentitiesService.GetUsersByRole", e);
                throw;
            }
        }

        /// <summary>
        ///     Establece o actualiza los claims del usuario pasado como parámetro
        /// </summary>
        /// <param name="user"><see cref="User"/> con los datos del usuario</param>
        /// <param name="refreshSession">Determina si se ha de refrescar la sesión del usuario actual (por defecto false)</param>
        /// <returns></returns>
        public async Task SetUserClaims(User user, bool refreshSession = false)
        {
            try
            {
                var claimsForUser = await _userManager.GetClaimsAsync(user);
                var rolesForUser = await _userManager.GetRolesAsync(user);

                //Claim nombre completo del usuario
                if(claimsForUser.Any(c => c.Type == Literals.Claim_FullName))
                {
                    await _userManager.ReplaceClaimAsync(user, claimsForUser.FirstOrDefault(c => c.Type == Literals.Claim_FullName), new Claim(Literals.Claim_FullName, user.FullName));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new Claim(Literals.Claim_FullName, user.FullName));
                }

                //Claim del id del idioma del usuario
                if (claimsForUser.Any(c => c.Type == Literals.Claim_LanguageId))
                {
                    await _userManager.ReplaceClaimAsync(user, claimsForUser.FirstOrDefault(c => c.Type == Literals.Claim_LanguageId), new Claim(Literals.Claim_LanguageId, user.Language.ToString()));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new Claim(Literals.Claim_LanguageId, user.Language.ToString()));
                }

                //Claim del rol del usuario
                if (claimsForUser.Any(c => c.Type == Literals.Claim_Role))
                {
                    await _userManager.ReplaceClaimAsync(user, claimsForUser.FirstOrDefault(c => c.Type == Literals.Claim_Role), new Claim(Literals.Claim_Role, rolesForUser != null ? rolesForUser.FirstOrDefault() : string.Empty));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new Claim(Literals.Claim_Role, rolesForUser != null ? rolesForUser.FirstOrDefault() : string.Empty));
                }

                //Claim del id del usuario
                if (claimsForUser.Any(c => c.Type == Literals.Claim_UserId))
                {
                    await _userManager.ReplaceClaimAsync(user, claimsForUser.FirstOrDefault(c => c.Type == Literals.Claim_UserId), new Claim(Literals.Claim_UserId, user.Id.ToString()));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new Claim(Literals.Claim_UserId, user.Id.ToString()));
                }

                if(refreshSession)
                {
                    await RefreshUserSession(user);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("IdentitiesService.SetUserClaims", e);
                throw;
            }
        }

        /// <summary>
        ///     Obtiene los claims del usuario pasado como parámetro
        /// </summary>
        /// <param name="user"><see cref="User"/> con los datos del usuario</param>
        /// <returns></returns>
        public async Task<IList<Claim>> GetClaims(User user)
        {
            try
            {
                return await _userManager.GetClaimsAsync(user);
            }
            catch (Exception e)
            {
                _logger.LogError("IdentitiesService.GetClaims", e);
                throw;
            }
        }

        /// <summary>
        ///     Actualiza la contraseña del usuario
        /// </summary>
        /// <param name="user"><see cref="User"/> con los datos del usuario</param>
        /// <param name="password">Nueva contraseña</param>
        /// <returns></returns>
        public async Task<bool> UpdateUserPassword(User user, string password)
        {
            try
            {
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, password);
                await _userManager.UpdateAsync(user);

                return result.Succeeded;
            }
            catch (Exception e)
            {
                _logger.LogError("IdentitiesService.UpdateUserPassword", e);
                throw;
            }
        }

        /// <summary>
        ///     Actualiza el rol de un usuario
        /// </summary>
        /// <param name="user"><see cref="User"/> con los datos del usuario</param>
        /// <param name="roleName">El nombre del nuevo rol</param>
        /// <returns></returns>
        public async Task<bool> UpdateUserRole(User user, string roleName)
        {
            try
            {
                if(await _userManager.IsInRoleAsync(user, roleName))
                {
                    return true;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);

                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                return roleResult.Succeeded;
            }
            catch(Exception e)
            {
                _logger.LogError("IdentitiesService.UpdateUserRole", e);
                throw;
            }
        }

        #endregion

        #region Métodos privados

        /// <summary>
        ///     Genera los roles de la aplicación
        /// </summary>
        /// <returns></returns>
        private async Task DefaultRoles()
        {
            try
            {
                var roles = new List<Role>
                {
                    new Role() {Id = "1", Name = Literals.Role_SupportManager, Description = "Manager of the technician group", Level = 0},
                    new Role() {Id = "2", Name = Literals.Role_SupportTechnician, Description = "Support technician", Level = 1}
                };

                foreach(var role in roles)
                {
                    if(_roleManager.FindByNameAsync(role.Name).Result == null)
                    {
                        var result = await _roleManager.CreateAsync(role);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("IdentitiesService.DefaultRoles", e);
                throw;
            }
        }

        /// <summary>
        ///     Genera el usuario SupportManager
        /// </summary>
        /// <returns></returns>
        public async Task<User> DefaultUser()
        {
            const string userName = "SupportManager";
            const string password = "IoT@2024";

            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    user = new User()
                    {
                        UserName = userName,
                        Email = "supportmanager@company.com",
                        PhoneNumber = "123456789",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        LockoutEnabled = false,
                        LockoutEnd = null,
                    };

                    var result = _userManager.CreateAsync(user, password).Result;
                    if(result.Succeeded) {
                        var rolesForUser = _userManager.GetRolesAsync(user).Result;
                        if(!rolesForUser.Contains(Literals.Role_SupportManager))
                        {
                            await _userManager.AddToRoleAsync(user, Literals.Role_SupportManager);
                        }

                        await SetUserClaims(user);

                        await _unitOfWork.SaveChanges();
                    }
                }
                return user;
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message, "IdentitiesService => DefaultUser");
                throw;
            }
        }

        /// <summary>
        ///     Vuelve a iniciar sesión del usuario actual para refrescar los claims
        /// </summary>
        /// <param name="user">Usuario logeado</param>
        /// <returns></returns>
        private async Task RefreshUserSession(User user)
        {
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, true);
        }

        #endregion
    }
}
