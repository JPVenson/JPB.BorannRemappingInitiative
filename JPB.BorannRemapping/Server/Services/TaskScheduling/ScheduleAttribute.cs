using System;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public abstract class ScheduleAttribute : Attribute
	{
		public abstract Schedule When();
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class ScheduleOnDemandAttribute : ScheduleAttribute
	{
		public override Schedule When()
		{
			return Schedule.Task();
		}
	}
}