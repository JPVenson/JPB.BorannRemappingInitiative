using System;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ScheduleTaskEveryAttribute : ScheduleAttribute
	{
		private readonly TimeSpan _runat;

		public ScheduleTaskEveryAttribute(int hour, int minute, int second) : this(new TimeSpan(hour, minute, second))
		{

		}

		public ScheduleTaskEveryAttribute(int day, int hour, int minute, int second) : this(new TimeSpan(day, hour, minute, second))
		{

		}

		public ScheduleTaskEveryAttribute(TimeSpan runat)
		{
			_runat = runat;
		}

		public override Schedule When()
		{
			return Schedule.Every(_runat);
		}
	}
}