using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Common
{
    public class JwtHelpers
    {
        public static string GetEmailFromToken(string authorization)
        {
            if (authorization == null)
            {
                throw new ArgumentException($"jwt token can not be null");
            }
            authorization = authorization.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(authorization);
            return token.Claims.First(x => x.Type == "unique_name").Value;
        }

        public static string GetDeviceIDFromAuthString(string authorization)
        {
            if (authorization == null)
            {
                throw new ArgumentException($"jwt token can not be null");
            }
            authorization = authorization.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(authorization);
            return token.Claims.First(x => x.Type == "actort").Value;
        }

        public static string GetDeviceIDFromToken(string tokenString)
        {
            if (tokenString.Length == 0)
                return "";
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);
            return token.Claims.First(x => x.Type == "actort").Value;
        }
    }
}
