using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    internal class TaskObject
    {
        public CancellationTokenSource token = new CancellationTokenSource();
        public ITaskToRun taksTaskToRun;
        public Task task;
    }

    public class RunningTaskController : ITaskRunController
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("RunningTaskController");
        private IConfiguration _configuration;

        public RunningTaskController(IConfiguration configuration)
        {
            _configuration = configuration;
            _taskFinished += _OnTaskFinished;
        }

        private object Locker = new object();
        private Dictionary<object, TaskObject> _RunningTask = new Dictionary<object, TaskObject>();
        private Queue<ITaskToRun> _QueuedTasks = new Queue<ITaskToRun>();
        public bool Active { get; set; } = true;

        private delegate void TaskFinished(object id);

        private TaskFinished _taskFinished;

        private TaskObject _CreateTaskObject(ITaskToRun taskToRun)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            TaskObject task = new TaskObject()
            {
                token = tokenSource,
                taksTaskToRun = taskToRun,
                task = new Task(
                    () =>
                    {
                        logger.LogDebug(
                            $"------------------------ >>>>>> Activated task {taskToRun.Id} of [[{taskToRun}]]"
                        );
                        taskToRun.ActionToRun.Invoke();
                        Task.Run(() =>
                        {
                            _taskFinished.Invoke(taskToRun.Id);
                        });
                    },
                    tokenSource.Token
                ),
            };
            return task;
        }

        private void _OnTaskFinished(object id)
        {
            logger.LogDebug("Task finished: ");
            lock (Locker)
            {
                this._RunningTask.Remove(id);
                if (
                    this._RunningTask.Count < this._configuration.MaxStimulationsFileSync
                    && this._QueuedTasks.Count > 0
                )
                {
                    List<ITaskToRun> allTaks = this._QueuedTasks.ToList();
                    List<ITaskToRun> readyToRun = this
                        ._QueuedTasks.Where(x => !this._RunningTask.ContainsKey(x.Id))
                        .ToList();
                    if (readyToRun.Count > 0)
                    {
                        int freeSpace =
                            this._configuration.MaxStimulationsFileSync - this._RunningTask.Count;
                        for (int i = 0; i < freeSpace && i < readyToRun.Count; i++)
                        {
                            allTaks.Remove(readyToRun[i]);
                            _AddAndActivateTaskObject(readyToRun[i]);
                        }
                        this._QueuedTasks = new Queue<ITaskToRun>(allTaks);
                    }
                }
            }
        }

        private void _AddAndActivateTaskObject(ITaskToRun task)
        {
            TaskObject takTaskObject = _CreateTaskObject(task);
            ITaskToRun taskWIthTheSameId = this._RunningTask.ContainsKey(task.Id)
                ? this._RunningTask[task.Id].taksTaskToRun
                : null;
            if (taskWIthTheSameId != null)
            {
                this._QueuedTasks.Enqueue(task);
            }
            else
            {
                _RunningTask.Add(task.Id, takTaskObject);

                takTaskObject.task.Start();
            }
        }

        private bool isTaksWithTheSameIDAlreadyRunning(ITaskToRun task)
        {
            return this._RunningTask.ContainsKey(task.Id);
        }

        public void AddTask(ITaskToRun TaskToRun)
        {
            lock (Locker)
            {
                if (
                    _RunningTask.Count < this._configuration.MaxStimulationsFileSync
                    && this.Active
                    && !isTaksWithTheSameIDAlreadyRunning(TaskToRun)
                )
                {
                    _AddAndActivateTaskObject(TaskToRun);
                }
                else
                {
                    this._QueuedTasks.Enqueue(TaskToRun);
                }
            }
        }

        public void CancelAllTasks()
        {
            lock (Locker)
            {
                this._QueuedTasks.Clear();
                foreach (TaskObject taskToRun in this._QueuedTasks)
                {
                    taskToRun.token.Cancel();
                }
                this._QueuedTasks.Clear();
            }
        }

        public void CancelTask(object key)
        {
            throw new NotImplementedException();
        }

        public int ActiveTasksCount
        {
            get
            {
                lock (Locker)
                {
                    return this._RunningTask.Count;
                }
            }
        }

        public int QueuedTasksCount
        {
            get
            {
                lock (Locker)
                {
                    return this._QueuedTasks.Count;
                }
            }
        }
        public int AllTasksCount
        {
            get
            {
                lock (Locker)
                {
                    return this._QueuedTasks.Count + this.ActiveTasksCount;
                }
            }
        }
    }
}
