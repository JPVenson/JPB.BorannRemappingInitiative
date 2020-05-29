using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling.TaskRunners
{
    internal class ScheduledTaskRunner : ITaskRunner
    {
        private DateTime _lastRun;

        public ScheduledTaskRunner(ITask task, TimeSpan runAt)
        {
            Task = task;
            RunAt = runAt;

            if (runAt < DateTime.UtcNow.TimeOfDay)
            {
                _lastRun = DateTime.UtcNow;
            }
        }

        public ITask Task { get; }

        public TimeSpan? DetermininateNextRun()
        {
            var now = DateTime.UtcNow;
            if (_lastRun.Date == DateTime.UtcNow.Date)
            {
                var nextRun = RunAt.Add(new TimeSpan(1, 0, 0, 0)).Subtract(now.TimeOfDay);
                return nextRun;
            }

            return RunAt - now.TimeOfDay;
        }

        public TimeSpan DetermininateLastRun()
        {
            return DateTime.UtcNow - _lastRun;
        }

        public bool? LastRunSuccess { get; private set; }

        protected TimeSpan RunAt { get; }

        public async Task Execute(ILogger<ITask> logger)
        {
            try
            {
                await Task.Run(logger);
                LastRunSuccess = true;
            }
            catch
            {
                LastRunSuccess = false;
                throw;
            }
            finally
            {
                _lastRun = DateTime.UtcNow;
            }
        }

        public bool Check()
        {
            return _lastRun.Date != DateTime.UtcNow.Date && DateTime.UtcNow.TimeOfDay > RunAt && !Task.IsBusy;
        }
    }
}