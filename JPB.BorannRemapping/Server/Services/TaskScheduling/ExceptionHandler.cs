namespace JPB.Katana.CommonTasks.TaskScheduling
{
	public class ExceptionHandler
	{
		public ExceptionHandler()
		{

		}

		public bool IsCancled { get; private set; }

		public void Cancel()
		{
			IsCancled = true;
		}
	}
}