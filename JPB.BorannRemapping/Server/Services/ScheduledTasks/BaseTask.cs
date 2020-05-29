using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JPB.Katana.CommonTasks.TaskScheduling;
using Microsoft.Extensions.Logging;

namespace JPB.MyWorksheet.Shared.Services.ScheduledTasks
{
	public abstract class BaseTask : ITask
	{
		public async Task Run(ILogger<ITask> logger)
		{
			var failed = false;
			var sp = new Stopwatch();
			sp.Start();
			logger
				.LogInformation(string.Format("Started {1} at {0}", DateTime.UtcNow, NamedTask));
			try
			{
				IsBusy = true;
				DoWork(logger);
				await DoWorkAsync(logger);
			}
			catch (Exception e)
			{
				failed = true;
				throw;
			}
			finally
			{
				sp.Stop();
				logger.LogInformation
				(string.Format("Stopped " + (failed ? "Failed" : "") + " {1} at {0}", DateTime.UtcNow, NamedTask), new Dictionary<string, string>()
				{
					{"Duration", sp.Elapsed.ToString("G") }
				});
				IsBusy = false;
			}
		}

		public virtual void DoWork(ILogger<ITask> logger)
		{

		}
		public virtual Task DoWorkAsync(ILogger<ITask> logger)
		{
			return Task.CompletedTask;
		}

		public bool IsBusy { get; protected set; }
		public abstract string NamedTask { get; protected set; }
	}
}