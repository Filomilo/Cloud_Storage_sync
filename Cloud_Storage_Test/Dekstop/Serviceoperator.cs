using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_desktop.Logic;
using NUnit.Framework;

namespace Cloud_Storage_Test.Dekstop
{
    [TestFixture]
    internal class ServiceOperatorTest
    {
        ServiceOperator serviceOperator = new ServiceOperator();

        private void EnsureServiceExist()
        {
            if (!serviceOperator.Exist)
            {
                serviceOperator.CreateService();
                TestHelpers.EnsureTrue((() => serviceOperator.Exist));
            }
            Assert.That(serviceOperator.Exist);
        }

        private void EnsureServiceNotExist()
        {
            if (serviceOperator.Exist)
            {
                serviceOperator.DeleteService();
                TestHelpers.EnsureTrue((() => !serviceOperator.Exist));
            }
            Assert.That(!serviceOperator.Exist);
        }

        private void EnsureServiceIsRunning()
        {
            EnsureServiceExist();
            if (!serviceOperator.IsServiceRunning())
            {
                serviceOperator.StartService();
                TestHelpers.EnsureTrue((() => serviceOperator.IsServiceRunning()));
            }
        }

        private void EnsureServiceIsNotRunning()
        {
            EnsureServiceExist();
            if (serviceOperator.IsServiceRunning())
            {
                serviceOperator.StopService();
                TestHelpers.EnsureTrue((() => !serviceOperator.IsServiceRunning()));
            }
        }

        [SetUp]
        public void Setup()
        {
            EnsureServiceExist();
            EnsureServiceIsNotRunning();
        }

        [TearDown]
        public void TearDown()
        {
            EnsureServiceIsNotRunning();
            EnsureServiceNotExist();
        }

        [Test]
        public void TestServiceStart()
        {
            serviceOperator.StartService();
            Assert.That(serviceOperator.IsServiceRunning());
        }

        [Test]
        public void TestServiceStop()
        {
            EnsureServiceIsRunning();
            serviceOperator.StopService();
            Assert.That(!serviceOperator.IsServiceRunning());
        }

        [Test]
        public void TestServiceDelete()
        {
            EnsureServiceIsRunning();
            serviceOperator.DeleteService();
            TestHelpers.EnsureTrue((() => !serviceOperator.Exist), 5000);
            Assert.That(!serviceOperator.Exist, $"serivec existence is {serviceOperator.Exist}");
        }

        [Test]
        public void TestServiceUpdate()
        {
            EnsureServiceIsRunning();
            serviceOperator.UpdateService();
            Assert.That(serviceOperator.IsServiceRunning());
        }

        [Test]
        public void TestServiceCreate()
        {
            EnsureServiceNotExist();
            serviceOperator.CreateService();
            Assert.That(serviceOperator.Exist);
        }
    }
}
