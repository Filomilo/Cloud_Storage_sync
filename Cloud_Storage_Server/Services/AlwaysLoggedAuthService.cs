using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Server.Configurations;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cloud_Storage_Server.Services
{
    public class AlwaysLoggedAuthService: IAuthService
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator;
        public AlwaysLoggedAuthService(IDataBaseContextGenerator dataBaseContextGenerator)
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
        }
        public string GenerateToken(User user, Device device)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(AuthConfiguration.PrivateKey);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            );

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = GenerateClaims(user, device),
                Expires = DateTime.UtcNow.AddMonths(12),
                SigningCredentials = credentials,
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private int deviceCounter = 0;
        private ClaimsIdentity GenerateClaims(User user, Device device)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim("ClaimTypes.Name", "fakeMail"));
            claims.AddClaim(new Claim(ClaimTypes.Actor, deviceCounter++.ToString()));
            return claims;
        }
        public bool VerifyUser(string mail, string password)
        {
            return true;
        }

        public User CreateNewUserBeasedOnLoginRequest(AuthRequest loginRequest)
        {
            return new User()
            {
                id = 0,
                mail = "fakemail",
                password = "fakeapss"
            };
        }

        public bool validatePasswordFormat(string password)
        {
            return true;
        }
    }
}
