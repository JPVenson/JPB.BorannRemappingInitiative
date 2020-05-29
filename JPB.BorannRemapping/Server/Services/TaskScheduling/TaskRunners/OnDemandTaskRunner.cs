using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling.TaskRunners
{
	public class OnDemandTaskRunner : ITaskRunner
	{
		private DateTime _lastRun;

		public OnDemandTaskRunner(ITask task)
		{
			Task = task;
		}

		public bool Check()
		{
			return false;
		}

		public async Task Execute(ILogger<ITask> logger)
		{
			try
			{
				await Task.Run(logger);
				_lastRun = DateTime.UtcNow;
				LastRunSuccess = true;
			}
			catch
			{
				LastRunSuccess = false;
				throw;
			}
		}

		public ITask Task { get; }
		public TimeSpan? DetermininateNextRun()
		{
			return null;
		}

		public TimeSpan DetermininateLastRun()
		{
			return DateTime.UtcNow - _lastRun;
		}

		public bool? LastRunSuccess { get; set; }
	}
}