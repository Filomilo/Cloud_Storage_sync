using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Test;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib.Services
{
    [TestFixture]
    class FileSystemWatcherTest
    {
        IFIleSystemWatcher fIleSystemWatcher =
            new Cloud_Storage_Desktop_lib.Services.FileSystemWatcher();
        private string _Directory = TestHelpers.TmpDirecotry;

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(_Directory);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_Directory, true);
        }

        [Test]
        public void FileOpartionWatch()
        {
            string fileToCreate = "TestFile.txt";
            string fileInitalContent = "test";

            bool wasOnCreatedEventHandler = false;
            bool wasOnChangeEventHadnler = false;
            bool wasOnDeltedeEventHadnler = false;
            this.fIleSystemWatcher.OnCreatedEventHandler += (
                args =>
                {
                    wasOnCreatedEventHandler = true;
                    Assert.That(args.Name == fileToCreate);
                }
            );
            this.fIleSystemWatcher.OnChangedEventHandler += (
                args =>
                {
                    if (args.ChangeType == WatcherChangeTypes.Changed)
                    {
                        wasOnChangeEventHadnler = true;
                    }
                }
            );

            this.fIleSystemWatcher.OnDeletedEventHandler += (
                args =>
                {
                    wasOnDeltedeEventHadnler = true;

                    Assert.That(args.Name == fileToCreate);
                }
            );

            this.fIleSystemWatcher.Directory = _Directory;

            File.WriteAllText($"{_Directory}{fileToCreate}", fileInitalContent);
            File.WriteAllText($"{_Directory}{fileToCreate}", "    ");
            File.Delete($"{_Directory}{fileToCreate}");
            Thread.Sleep(100);
            Assert.That(wasOnCreatedEventHandler);
            Assert.That(wasOnChangeEventHadnler);
            Assert.That(wasOnDeltedeEventHadnler);
        }
    }
}
