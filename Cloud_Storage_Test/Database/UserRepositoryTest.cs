using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Cloud_Storage_Test.Database;

[TestFixture]
public class UserRepositoryTest
{
    [Test]
    public void saveUser()
    {
        using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            int amountOFUsersBefore = context.Users.ToList().Count;

            UserRepository.saveUser(
                context,
                new User() { mail = "mail@mail.mail", password = "password" }
            );

            int amountOfUsersAfter = context.Users.ToList().Count;
            Assert.That(amountOfUsersAfter == amountOFUsersBefore + 1);
        }
    }

    [Test]
    public void saveUserIncorrectEmail()
    {
        using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            Assert.Catch(
                expectedExceptionType: typeof(ValidationException),
                code: () =>
                {
                    UserRepository.saveUser(
                        context,
                        new User() { mail = "123", password = "password" }
                    );
                }
            );
        }
    }

    [Test]
    public void saveUserWithTheSameEmail()
    {
        using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            int amountOFUsersBefore = context.Users.ToList().Count;

            UserRepository.saveUser(
                context,
                new User() { mail = "123@123.123", password = "password" }
            );

            int amountOfUsersAfter = context.Users.ToList().Count;
            Assert.That(amountOFUsersBefore + 1 == amountOfUsersAfter);
            Assert.Catch(
                typeof(AggregateException),
                () =>
                {
                    UserRepository.saveUser(
                        context,
                        new User() { mail = "123@123.123", password = "password" }
                    );
                }
            );
        }
    }
}
