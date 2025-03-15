using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Server.Database.Models;
using Microsoft.AspNetCore.Routing;

namespace Cloud_Storage_Server.Database.Repositories
{

    public static class UserRepository
    {
        public static User saveUser(User user)
        {
            User usersaved = null;
            using (DatabaseContext context = new DatabaseContext())
            {
                var validationContext = new ValidationContext(user);
                Validator.ValidateObject(user, validationContext, true);
                usersaved= context.Add(user).Entity;
                context.SaveChanges();
            }

            return usersaved;
        }


    }
}
