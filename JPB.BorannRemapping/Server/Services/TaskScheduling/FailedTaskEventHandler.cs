using System;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
	public delegate void FailedTaskEventHandler(ITask task, Exception exception, ExceptionHandler handler);
}