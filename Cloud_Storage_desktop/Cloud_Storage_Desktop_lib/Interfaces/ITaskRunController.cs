﻿namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface ITaskToRun
    {
        public object Id { get; }
        public Action ActionToRun { get; }
    }

    public interface ITaskRunController
    {
        public void AddTask(ITaskToRun TaskToRun);
        public void CancelTask(object key);
        public void CancelAllTasks();
        public int ActiveTasksCount { get; }
        public int QueuedTasksCount { get; }
        public int AllTasksCount { get; }
        public bool Active { get; set; }
    }
}
