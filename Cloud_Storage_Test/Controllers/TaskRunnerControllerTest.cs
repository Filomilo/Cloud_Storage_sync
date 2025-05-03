using Cloud_Storage_Desktop_lib.Interfaces;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class TestTask : ITaskToRun
    {
        private Int32 _key;
        private Action _action;

        public object Id
        {
            get { return _key; }
        }

        public Action ActionToRun
        {
            get { return _action; }
        }

        public TestTask(Int32 id, Action task)
        {
            _key = id;
            this._action = task;
            ;
        }
    }

    internal class TestConfiuguration : IConfiguration
    {
        public string ApiUrl { get; set; }

        public int MaxStimulationsFileSync
        {
            get { return 5; }
            set { }
        }

        public string DeviceUUID
        {
            get { return "test"; }
        }

        public string StorageLocation { get; set; }

        public void LoadConfiguration() { }

        public void SaveConfiguration() { }

        public event ConfigurationChange? OnConfigurationChange;
    }

    [TestFixture()]
    class TaskRunnerControllerTest
    {
        private IConfiguration _configuration = new TestConfiuguration();
        private ITaskRunController _controller = new RunningTaskController(
            new TestConfiuguration()
        );

        [Test]
        public void AddCancelTasks()
        {
            int AimTasks = 50;
            int activatedTasks = 0;
            _controller.Active = true;

            for (int i = 0; i < 50; i++)
            {
                _controller.AddTask(
                    new TestTask(
                        i,
                        () =>
                        {
                            activatedTasks++;
                            Thread.Sleep(1000);
                        }
                    )
                );
            }

            Thread.Sleep(100);
            Assert.That(activatedTasks == _configuration.MaxStimulationsFileSync);
            Assert.That(_controller.ActiveTasksCount == _configuration.MaxStimulationsFileSync);
            Assert.That(_controller.AllTasksCount == AimTasks);
            Thread.Sleep(1010);
            Assert.That(activatedTasks == _configuration.MaxStimulationsFileSync * 2);
            Assert.That(
                _controller.AllTasksCount == AimTasks - _configuration.MaxStimulationsFileSync
            );
            _controller.CancelAllTasks();
            Thread.Sleep(1010);
            Assert.That(activatedTasks == _configuration.MaxStimulationsFileSync * 2);
            Assert.That(_controller.AllTasksCount == 0);
        }
    }
}
