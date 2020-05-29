
using System;

namespace JPB.Katana.CommonTasks.TaskScheduling {

	public interface IScheduler {
		TimeSpan NextInteration { get; }
		void Start();
		void Stop();
		bool TryAddTask(ITask task, Schedule schedule);
	}
}
