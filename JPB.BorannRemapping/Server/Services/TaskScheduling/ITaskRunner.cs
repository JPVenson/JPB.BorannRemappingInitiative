
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling{
	public interface ITaskRunner
	{
		bool Check();
		Task Execute(ILogger<ITask> logger);
		ITask Task { get; }
		TimeSpan? DetermininateNextRun();
		TimeSpan DetermininateLastRun();
		bool? LastRunSuccess { get; }
	}
}
