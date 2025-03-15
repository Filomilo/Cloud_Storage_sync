using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Test;

public class UserRepositoryTest
{
    [Fact]
    public void saveUser()
    {
        using (var context = new DatabaseContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            int amountOFUsersBefore = context.Users.ToList().Count;

            UserRepository.saveUser(
              new User()
              {
                  mail = "mail@mail.mail",
                  password = "password"
              }
            
            );

            int amountOfUsersAfter=context.Users.ToList().Count;
            Assert.Equal(amountOFUsersBefore+1,amountOfUsersAfter);

        }
           
    }
    [Fact]
    public void saveUserIncorrectEmail()
    {

        using (var context = new DatabaseContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            Assert.Throws(typeof(ValidationException),new Action((() =>
            {
                UserRepository.saveUser(
                    new User()
                    {
                        mail = "123",
                        password = "password"
                    }

                );
            })));

        }

    }
    [Fact]
    public void saveUserWithTheSameEmail()
    {
        using (var context = new DatabaseContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            int amountOFUsersBefore = context.Users.ToList().Count;

            UserRepository.saveUser(
                new User()
                {
                    mail = "123@123.123",
                    password = "password"
                }

            );

            int amountOfUsersAfter = context.Users.ToList().Count;
            Assert.Equal(amountOFUsersBefore + 1, amountOfUsersAfter);
            Assert.Throws(typeof(DbUpdateException), new Action((() =>
            {
                UserRepository.saveUser(
                    new User()
                    {
                        mail = "123@123.123",
                        password = "password"
                    }

                );
            })));
        }

    }

}
