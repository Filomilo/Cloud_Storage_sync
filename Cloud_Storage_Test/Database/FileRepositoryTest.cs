using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using FileData = Cloud_Storage_Common.Models.FileData;

namespace Cloud_Storage_Test.Database;

public class FileRepositoryTest
{
    private User? _savedUser;
    private DatabaseContext? context;

    [SetUp]
    public void PrepareUser()
    {
        context = new DatabaseContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        _savedUser = UserRepository.saveUser(
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
            new SyncFileData()
            {
                Extenstion = "jpg",
                Hash = "1234567",
                Name = "File",
                Path = "location1\\location2\\location\\3",
                SyncDate = DateTime.Now,
                OwnerId = _savedUser.id,
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
                        new SyncFileData()
                        {
                            Extenstion = "jpg",
                            Hash = "45678",
                            Name = "File",
                            Path = "location1\\location2\\location\\3",
                            SyncDate = DateTime.Now,
                            OwnerId = -1,
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
        using (var context = new DatabaseContext())
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
            Extenstion = "jpg",
            Hash = "3546",
            Name = "File",
            Path = ".",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
        };
        SyncFileData fileToSaveCopy = new SyncFileData()
        {
            Extenstion = "jpg",
            Hash = "3456",
            Name = "File",
            Path = ".",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
        };

        int amountOfFilesBefore = context.Files.ToList().Count;
        FileRepository.SaveNewFile(fileToSave);
        Assert.Throws(
            typeof(DbUpdateException),
            () =>
            {
                FileRepository.SaveNewFile(fileToSaveCopy);
            }
        );

        int amoutOfFilesAfter = context.Files.ToList().Count;
        Assert.That(amountOfFilesBefore + 1 == amoutOfFilesAfter);
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
        };

        SyncFileData fileUpdateData = new SyncFileData()
        {
            Extenstion = "png",
            Hash = "234567",
            Name = "File88",
            Path = "newFolder",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
        };

        SyncFileData savedFile = FileRepository.SaveNewFile(fileToSave);

        FileRepository.UpdateFile(
            (Cloud_Storage_Common.Models.FileData)fileToSave,
            (Cloud_Storage_Common.Models.FileData)fileUpdateData
        );

        SyncFileData fileInRepository = FileRepository.GetFileOfID(savedFile.Id);
        Assert.That(fileUpdateData.Extenstion == fileInRepository.Extenstion);
        Assert.That(fileUpdateData.Name == fileInRepository.Name);
        Assert.That(fileUpdateData.Path == fileInRepository.Path);
    }

    [Test]
    public void UpdateFile_fileDoesntExist()
    {
        using (var context = new DatabaseContext())
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
            };

            SyncFileData fileUpdateData = new SyncFileData()
            {
                Extenstion = "png",
                Hash = "234567",
                Name = "File88",
                Path = "newFolder",
                SyncDate = DateTime.Now,
                OwnerId = _savedUser.id,
            };
            Assert.Throws(
                typeof(KeyNotFoundException),
                () =>
                {
                    FileRepository.UpdateFile((FileData)fileToSave, (FileData)fileUpdateData);
                }
            );
        }
    }

    [Test]
    public void GetFileOfID_correct()
    {
        SyncFileData fileToSave = new SyncFileData()
        {
            Extenstion = "jpg",
            Name = "File",
            Path = ".",
            SyncDate = DateTime.Now,
            OwnerId = _savedUser.id,
        };

        SyncFileData savedFile = FileRepository.SaveNewFile(fileToSave);

        SyncFileData fileInRepository = FileRepository.GetFileOfID(savedFile.Id);
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
                SyncFileData fileInRepository = FileRepository.GetFileOfID(new Guid());
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
        };

        SyncFileData savedFile = FileRepository.SaveNewFile(fileToSave);

        SyncFileData fileInRepository = FileRepository.getFileByPathNameExtensionAndUser(
            path: savedFile.Path,
            name: savedFile.Name,
            extension: savedFile.Extenstion,
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
        using (var context = new DatabaseContext())
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
            };
            Assert.Throws(
                typeof(KeyNotFoundException),
                () =>
                {
                    SyncFileData fileInRepository =
                        FileRepository.getFileByPathNameExtensionAndUser(
                            path: fileToSave.Path,
                            name: fileToSave.Name,
                            extension: fileToSave.Extenstion,
                            ownerId: _savedUser.id
                        );
                }
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
            };

            SyncFileData savedFile = FileRepository.SaveNewFile(fileToSave);
        }

        List<SyncFileData> files = FileRepository.GetAllUserFiles(_savedUser.id);
        Assert.That(files.Count == amountOfFile);
    }
}
