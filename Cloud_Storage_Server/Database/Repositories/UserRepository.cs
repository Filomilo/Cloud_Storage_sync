using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class UserRepository
    {
        public static User saveUser(AbstractDataBaseContext context, User user)
        {
            User usersaved = null;

            var validationContext = new ValidationContext(user);
            Validator.ValidateObject(user, validationContext, true);
            usersaved = context.Add(user).Entity;
            context.SaveChanges();

            return usersaved;
        }

        internal static User getUserByMail(AbstractDataBaseContext context, string mail)
        {
            User user = context.Users.FirstOrDefault(x => x.mail == mail);
            if (user == null)
                throw new KeyNotFoundException("not user with that email in database");
            return user;
        }

        public static bool DoesUserWithMailExist(AbstractDataBaseContext context, string mail)
        {
            try
            {
                User user = getUserByMail(context, mail);
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                return false;
            }
        }
    }
}
