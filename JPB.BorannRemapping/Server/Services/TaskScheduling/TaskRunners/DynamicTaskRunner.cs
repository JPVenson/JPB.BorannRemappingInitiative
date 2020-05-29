using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling.TaskRunners
{
	public class DynamicTaskRunner : ITaskRunner
	{
		private readonly Func<DateTime> _reinvalidate;
		private readonly Scheduler _attachedTo;

		public DynamicTaskRunner(ITask task, Func<DateTime> reinvalidate, Scheduler attachedTo)
		{
			_reinvalidate = reinvalidate;
			_attachedTo = attachedTo;
			Task = task;
		}

		public void Reinvalidate()
		{
			RunAt = _reinvalidate();
			_attachedTo.Reinvalidate(RunAt.Value);
		}

		public DateTime? RunAt { get; set; }
		private DateTime _lastRun;

		public bool Check()
		{
			return RunAt <= DateTime.UtcNow;
		}

		public async Task Execute(ILogger<ITask> logger)
		{
			_lastRun = DateTime.UtcNow;
			
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
			Reinvalidate();
		}

		public ITask Task { get; }
		public TimeSpan? DetermininateNextRun()
		{
			if (!RunAt.HasValue)
			{
				RunAt = _reinvalidate();
			}

			if (RunAt.HasValue)
			{
				return RunAt - DateTime.UtcNow;
			}

			return null;
		}

		public TimeSpan DetermininateLastRun()
		{
			return DateTime.UtcNow - _lastRun;
		}

		public bool? LastRunSuccess { get; set; }
		public Task ExecuteAsync()
		{
			throw new NotImplementedException();
		}
	}
}
