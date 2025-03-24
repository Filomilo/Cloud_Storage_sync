using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Desktop_lib;

namespace Cloud_Storage_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
  
     
        [HttpPost]
        [Route("login")]
        public IActionResult login(AuthRequest loginRequest)
        {
     
            bool verifacation = AuthService.VerifyUser(loginRequest.Email, loginRequest.Password);
            if (verifacation){
                User user = UserRepository.getUserByMail(loginRequest.Email);
                return Ok(AuthService.GenerateToken(user));
            }
            else
                return Unauthorized("Invalid email or password");
        }

        [HttpPost]
        [Route("register")]
        public IActionResult register(AuthRequest loginRequest)
        {

            User user = AuthService.CreateNewUserBeasedOnLoginRequest(loginRequest);
            User savedUser;
            try
            {
                AuthService.validatePasswordFormat(loginRequest.Password);
                savedUser = UserRepository.saveUser(user);
            }

            catch (ValidationException ex)
            {
                return BadRequest($"Inavlid format: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"User with this email already exist");
            }
            catch (Exception ex)
            {
                return BadRequest($"Unkown error happend");
            }

            return Ok(AuthService.GenerateToken(savedUser));
        }


    }
}
