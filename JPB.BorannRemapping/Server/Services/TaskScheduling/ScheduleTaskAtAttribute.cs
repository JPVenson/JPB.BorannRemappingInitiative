using System;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
	/// <summary>
	///		Schedules the task at the same time each day
	/// </summary>
	/// <seealso cref="JPB.Katana.CommonTasks.TaskScheduling.ScheduleAttribute" />
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ScheduleTaskAtAttribute : ScheduleAttribute
	{
		private readonly TimeSpan _runat;

		public ScheduleTaskAtAttribute(int hour, int minute, int second) : this(new TimeSpan(hour, minute, second))
		{

		}

		public ScheduleTaskAtAttribute(int day, int hour, int minute, int second) : this(new TimeSpan(day, hour, minute, second))
		{

		}

		public ScheduleTaskAtAttribute(TimeSpan runat)
		{
			_runat = runat;
		}

		public override Schedule When()
		{
			return Schedule.At(_runat);
		}
	}
}