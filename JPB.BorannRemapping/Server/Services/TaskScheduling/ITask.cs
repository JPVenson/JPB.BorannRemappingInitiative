using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
    public interface ITask
    {
        Task Run(ILogger<ITask> logger);
        bool IsBusy { get; }

        string NamedTask { get; }

        // TODO MaxRunTime - maximum time this should be able to run for before scheduler thinks its bombed out
        // TODO Priority - used to prioritise scheduledtasks
        // TODO IdleWait - how long the scheduler has to be idle after starting before this task is "run" for the first time
    }
}
