using Cloud_Storage_Server.Database.Models;
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;



namespace Cloud_Storage_Server.Services.PasswordHasher
{

    public static class BCryptPasswordHasher 
    {
        public static string HashPassword(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            return passwordHash;
        }

        public static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
    }
}
