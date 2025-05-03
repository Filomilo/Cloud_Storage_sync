using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDataBaseContextGenerator _dataBaseContextGenerator;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> logger;

        public AuthController(
            IDataBaseContextGenerator dataBaseContextGenerator,
            IAuthService authService,
            ILogger<AuthController> logger
        )
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
            _authService = authService;
            this.logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult login(AuthRequest loginRequest)
        {
            bool verifacation = _authService.VerifyUser(loginRequest.Email, loginRequest.Password);
            if (verifacation)
            {
                using (var context = _dataBaseContextGenerator.GetDbContext())
                {
                    User user = UserRepository.getUserByMail(context, loginRequest.Email);
                    Device device = DeviceRepository.AddNewDevice(context, user);
                    string token = _authService.GenerateToken(user, device);
                    return Ok(token);
                }
            }
            else
                return Unauthorized("Invalid email or password");
        }

        [HttpPost]
        [Route("register")]
        public IActionResult register(AuthRequest loginRequest)
        {
            logger.LogInformation($"Registering: {loginRequest.Email}");
            User user = _authService.CreateNewUserBeasedOnLoginRequest(loginRequest);
            User savedUser;
            try
            {
                using (var context = _dataBaseContextGenerator.GetDbContext())
                {
                    _authService.validatePasswordFormat(loginRequest.Password);
                    savedUser = UserRepository.saveUser(context, user);
                    Device device = DeviceRepository.AddNewDevice(context, savedUser);
                    string token = _authService.GenerateToken(savedUser, device);
                    return Ok(token);
                }
            }
            catch (ValidationException ex)
            {
                return BadRequest($"Inavlid format: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"User with this email already exist");
            }
            catch (AggregateException ex)
            {
                string allMessages = string.Join(" | ", ex.InnerExceptions.Select(e => e.Message));
                return BadRequest($"Unkown error happend [[{allMessages}]]");
            }
            catch (Exception ex)
            {
                return BadRequest($"Unkown error happend [[{ex.Message}]]");
            }
        }
    }
}
