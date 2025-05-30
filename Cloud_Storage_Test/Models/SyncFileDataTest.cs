using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using NUnit.Framework;
using NUnit.Framework.Constraints.Comparers;

namespace Cloud_Storage_Test.Models
{
    [TestFixture]
    internal class SyncFileDataTest
    {
        [Test]
        public void testGetWindowsStylePath()
        {
            FileData sync = new FileData();
            sync.Path = ".";
            sync.Name = "testFile";
            sync.Extenstion = ".mp4";
            String expected = ".\\testFile.mp4";
            Assert.That(
                sync.GetRealativePathWindowsStyle().Equals(expected),
                $"sync.GetRealativePathWindowsStyle() shuld be [[{expected}]] but is [[{sync.GetRealativePathWindowsStyle()}]]"
            );
        }
    }
}
