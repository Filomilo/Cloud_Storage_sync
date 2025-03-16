using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cloud_Storage_Server.Configurations;
using Cloud_Storage_Server.Database.Models;
using Microsoft.IdentityModel.Tokens;

namespace Cloud_Storage_Server.Services
{
    public static class AuthService
    {
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
                SigningCredentials = credentials
            };
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);

        }


        private static ClaimsIdentity GenerateClaims(User user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.Name,user.mail));
            return claims;
        }

    }
}
