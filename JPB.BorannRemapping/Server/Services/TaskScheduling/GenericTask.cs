using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
	public class GenericTask : ITask
	{
		public GenericTask(string namedTask, Action<ILogger<ITask>> taskItem, Action<Exception> onFailed)
		{
			TaskItem = taskItem;
			OnFailed = onFailed;
			NamedTask = namedTask;
		}

		public Action<ILogger<ITask>> TaskItem { get; private set; }
		public Action<Exception> OnFailed { get; private set; }
		public async Task Run(ILogger<ITask> logger)
		{
			IsBusy = true;
			try
			{
				TaskItem(logger);
				await Task.CompletedTask;
			}
			catch (Exception e)
			{
				OnFailed(e);
			}
			finally
			{
				IsBusy = false;
			}
		}

		public bool IsBusy { get; private set; }
		public string NamedTask { get; private set; }
	}
}