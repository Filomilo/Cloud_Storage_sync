using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Server.Database.Repositories
{

    public static class UserRepository
    {

        public static void saveUser(User user)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                context.Database.EnsureCreated();
                var validationContext = new ValidationContext(user);
                Validator.ValidateObject(user, validationContext, true);
                context.Add(user);
                context.SaveChanges();
            }
        }


    }
}
