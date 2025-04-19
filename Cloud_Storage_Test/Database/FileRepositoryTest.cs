using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Cloud_Storage_Test.Database;

public class FileRepositoryTest
{
    private User? _savedUser;
    private AbstractDataBaseContext? context;

    [SetUp]
    public void PrepareUser()
    {
        context = new SqliteDataBaseContextGenerator().GetDbContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        _savedUser = UserRepository.saveUser(
            context,
            new User() { mail = "mail@mail.mail", password = "password" }
        );
    }

    [TearDown]
    public void removeContext()
    {
        context.Dispose();
    }

    [Test]
    public void SaveNewFile_correct()
    {
        int amountOfFilesBefore = context.Files.ToList().Count;
        FileRepository.SaveNewFile(
            context,
            new SyncFileData()
            {
                Extenstion = "jpg",
                Hash = "1234567",
                Name = "File",
                Path = "location1\\location2\\location\\3",
                SyncDate = DateTime.Now,
                OwnerId = _savedUser.id,
                Version = 0,
                DeviceOwner = new List<string>() { "123" },
            }
        );

        int amoutOfFilesAfter = context.Files.ToList().Count;
        Assert.That(amountOfFilesBefore + 1 == amoutOfFilesAfter);
    }

    [Test]
    public void SaveNewFile_UserDoesntExist()
    {
        Assert.Throws(
            typeof(DbUpdateException),
            (TestDelegate)(
                () =>
                {
                    FileRepository.SaveNewFile(
                        context,
                        new SyncFileData()
                        {
                            Extenstion = "jpg",
                            Hash = "45678",
                            Name = "File",
                            Path = "location1\\location2\\location\\3",
                            SyncDate = DateTime.Now,
                            OwnerId = -1,
                            Version = 0,
                            DeviceOwner = new List<string>() { "123" },
                        }
                    );
                }
            )
        );
    }

    [TestCase("123")]
    [TestCase("///.sadasd//as//")]
    public void SaveNewFile_IncorrectPAth(string inocorrectPathName)
    {
        using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            int amountOfFilesBefore = context.Files.ToList().Count;
            Assert.Throws(
                typeof(ValidationException),
                (TestDelegate)(
                    () =>
                    {
                        FileRepository.SaveNewFile(
                            context,
                            new SyncFileData()
                            {
                                Extenstion = "jpg",
                                Hash = "",
                                Name = "File",
                                Path = inocorrectPathName,
                                SyncDate = DateTime.Now,
                                OwnerId = _savedUser.id,
                            }
                        );
                    }
                )
            );

            int amoutOfFilesAfter = context.Files.ToList().Count;
            Assert.That(amountOfFilesBefore == amoutOfFilesAfter);
        }
    }

    [Test]
    public void SaveNewFile_fileAlreadyExist()
    {
        SyncFileData fileToSave = new SyncFileData()
        {
            Id = Guid.NewGuid(),
            Extenstion = "jpg",
            Hash = "3546",
            Name = "File",
            Path = ".",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
            Version = 0,
            DeviceOwner = new List<string>() { "123" },
        };
        SyncFileData fileToSaveCopy = new SyncFileData()
        {
            Id = fileToSave.Id,
            Extenstion = "jpg",
            Hash = "3456",
            Name = "File",
            Path = ".",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
            Version = 0,
            DeviceOwner = new List<string>() { "123" },
        };

        int amountOfFilesBefore = context.Files.ToList().Count;
        FileRepository.SaveNewFile(context, fileToSave);
        context.SaveChanges();
        context.Dispose();
        using (var ctx = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            Assert.Throws(
                typeof(DbUpdateException),
                () =>
                {
                    FileRepository.SaveNewFile(ctx, fileToSaveCopy);
                }
            );
            int amoutOfFilesAfter = ctx.Files.ToList().Count;
            Assert.That(amountOfFilesBefore + 1 == amoutOfFilesAfter);
        }
    }

    [Test]
    public void UpdateFile_correct()
    {
        SyncFileData fileToSave = new SyncFileData()
        {
            Extenstion = "jpg",
            Hash = "23re",
            Name = "File",
            Path = ".",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
            Version = 0,
            DeviceOwner = new List<string>() { "123" },
        };

        SyncFileData fileUpdateData = new SyncFileData()
        {
            Extenstion = "png",
            Hash = "234567",
            Name = "File88",
            Path = "newFolder",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
            Version = 0,
            DeviceOwner = new List<string>() { "123" },
        };

        SyncFileData savedFile = FileRepository.SaveNewFile(context, fileToSave);

        FileRepository.UpdateFile(context, fileToSave, fileUpdateData);

        SyncFileData fileInRepository = FileRepository.GetFileOfID(context, savedFile.Id);
        Assert.That(fileUpdateData.Extenstion == fileInRepository.Extenstion);
        Assert.That(fileUpdateData.Name == fileInRepository.Name);
        Assert.That(fileUpdateData.Path == fileInRepository.Path);
    }

    [Test]
    public void UpdateFile_fileDoesntExist()
    {
        using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SyncFileData fileToSave = new SyncFileData()
            {
                Extenstion = "jpg",
                Hash = "",
                Name = "File",
                Path = "/",
                SyncDate = DateTime.Now,
                OwnerId = _savedUser.id,
                Version = 0,
                DeviceOwner = new List<string>() { "123" },
            };

            SyncFileData fileUpdateData = new SyncFileData()
            {
                Extenstion = "png",
                Hash = "234567",
                Name = "File88",
                Path = "newFolder",
                SyncDate = DateTime.Now,
                OwnerId = _savedUser.id,
                Version = 0,
                DeviceOwner = new List<string>() { "123" },
            };
            Assert.Throws(
                typeof(KeyNotFoundException),
                () =>
                {
                    FileRepository.UpdateFile(context, fileToSave, fileUpdateData);
                }
            );
        }
    }

    [Test]
    public void GetFileOfID_correct()
    {
        SyncFileData fileToSave = new SyncFileData()
        {
            Extenstion = ".jpg",
            Name = "File",
            Path = "folder",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
            Hash = "123",
            Version = 0,
            DeviceOwner = new List<string>() { "123" },
        };

        SyncFileData savedFile = FileRepository.SaveNewFile(context, fileToSave);

        SyncFileData fileInRepository = FileRepository.GetFileOfID(context, savedFile.Id);
        Assert.That(fileToSave.Extenstion == fileInRepository.Extenstion);
        Assert.That(fileToSave.Hash == fileInRepository.Hash);
        Assert.That(fileToSave.Name == fileInRepository.Name);
        Assert.That(fileToSave.Path == fileInRepository.Path);
        Assert.That(fileToSave.SyncDate == fileInRepository.SyncDate);
    }

    [Test]
    public void GetFileOfID_FileDoesntExist()
    {
        Assert.Throws(
            typeof(KeyNotFoundException),
            () =>
            {
                SyncFileData fileInRepository = FileRepository.GetFileOfID(context, new Guid());
            }
        );
    }

    [Test]
    public void getFileByPathNameAndExtension_correct()
    {
        SyncFileData fileToSave = new SyncFileData()
        {
            Extenstion = "jpg",
            Hash = "567890-",
            Name = "File",
            Path = "123\\123",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
            Version = 0,
            DeviceOwner = new List<string>() { "123" },
        };

        SyncFileData savedFile = FileRepository.SaveNewFile(context, fileToSave);

        SyncFileData fileInRepository = FileRepository.getNewestFileByPathNameExtensionAndUser(
            context,
            path: savedFile.Path,
            name: savedFile.Name,
            extenstion: savedFile.Extenstion,
            ownerId: _savedUser.id
        );
        Assert.That(fileToSave.Extenstion == fileInRepository.Extenstion);
        Assert.That(fileToSave.Hash == fileInRepository.Hash);
        Assert.That(fileToSave.Name == fileInRepository.Name);
        Assert.That(fileToSave.Path == fileInRepository.Path);
        Assert.That(fileToSave.SyncDate == fileInRepository.SyncDate);
    }

    [Test]
    public void getFileByPathNameAndExtension_fileDousntExist()
    {
        using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SyncFileData fileToSave = new SyncFileData()
            {
                Extenstion = "jpg",
                Hash = "",
                Name = "File",
                Path = "/123/123",
                SyncDate = DateTime.Now,

                Version = 0,
                DeviceOwner = new List<string>() { "123" },
            };
            Assert.That(
                FileRepository.getNewestFileByPathNameExtensionAndUser(
                    context,
                    path: fileToSave.Path,
                    name: fileToSave.Name,
                    extenstion: fileToSave.Extenstion,
                    ownerId: _savedUser.id
                ) == null
            );
        }
    }

    [Test]
    public void GetAllUserFiles_correct()
    {
        int amountOfFile = 10;
        for (int i = 0; i < amountOfFile; i++)
        {
            SyncFileData fileToSave = new SyncFileData()
            {
                Extenstion = "jpg",
                Hash = "1234",
                Name = $"File_{i}",
                Path = "123\\1234",
                SyncDate = DateTime.Now,
                OwnerId = _savedUser.id,
                Version = 0,
                DeviceOwner = new List<string>() { "123" },
            };

            SyncFileData savedFile = FileRepository.SaveNewFile(context, fileToSave);
        }

        List<SyncFileData> files = FileRepository.GetAllUserFiles(context, _savedUser.id);
        Assert.That(files.Count == amountOfFile);
    }
}
