using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling.TaskRunners
{
	internal class PeriodicalTaskRunner : ITaskRunner
	{

		private DateTime _lastRun;
		private DateTime _nextRun;
		private readonly int _frequency;

		public ITask Task { get; private set; }

		public PeriodicalTaskRunner(ITask task, int frequency) : this(task, frequency, DateTime.UtcNow)
		{

		}

		public PeriodicalTaskRunner(ITask task, int frequency, DateTime lastRun)
		{
			Task = task;
			_frequency = frequency;
			_lastRun = lastRun;
			_nextRun = _lastRun.AddSeconds(_frequency);
		}

		public TimeSpan? DetermininateNextRun()
		{
			return _nextRun - DateTime.UtcNow;
		}

		public TimeSpan DetermininateLastRun()
		{
			return DateTime.UtcNow - _lastRun;
		}

		public bool? LastRunSuccess { get; private set; }
		public bool IsAwaitable { get; set; }

		public async Task Execute(ILogger<ITask> logger)
		{
			try
			{
				await Task.Run(logger);
				_lastRun = _nextRun;
				LastRunSuccess = true;
			}
			catch
			{
				LastRunSuccess = false;
				throw;
			}
			finally
			{
				_nextRun = DateTime.UtcNow.AddSeconds(_frequency);
			}
		}

		public bool Check()
		{
			return DateTime.UtcNow > _nextRun && !Task.IsBusy;
		}
	}
}
