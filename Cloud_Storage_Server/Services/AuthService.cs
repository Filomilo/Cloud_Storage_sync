using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Server.Configurations;
using Cloud_Storage_Server.Controllers;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services.PasswordHasher;
using log4net;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Cloud_Storage_Server.Services
{
    public static class AuthService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AuthService));

        public static string GenerateToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(AuthConfiguration.PrivateKey);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            );

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddMonths(12),
                SigningCredentials = credentials,
            };
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private static ClaimsIdentity GenerateClaims(User user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.Name, user.mail));
            return claims;
        }

        public static bool VerifyUser(string mail, string password)
        {
            try
            {
                User dbUser = UserRepository.getUserByMail(mail);
                return BCryptPasswordHasher.VerifyHashedPassword(dbUser.password, password);
            }
            catch (Exception ex)
            {
                log.Warn($"Error while verifing user {ex}");
                return false;
            }
        }

        public static User CreateNewUserBeasedOnLoginRequest(AuthRequest loginRequest)
        {
            User user = new User
            {
                mail = loginRequest.Email,
                password = BCryptPasswordHasher.HashPassword(loginRequest.Password),
            };
            return user;
        }

        public static bool validatePasswordFormat(string password)
        {
            if (password.Length < 12)
            {
                throw new ValidationException("Password should have at least 12 characters");
            }
            return true;
        }

        internal static string GetEmailFromToken(string authorization)
        {
            authorization = authorization.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(authorization);
            return token.Claims.First(x => x.Type == "unique_name").Value;
        }
    }
}
