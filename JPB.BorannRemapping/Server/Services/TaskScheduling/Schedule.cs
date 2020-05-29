
using System;

namespace JPB.Katana.CommonTasks.TaskScheduling {

	/// <summary>
	/// Represents a Schedule under which a task should be processed
	/// </summary>
	public class Schedule {

		public ScheduleType Type { get; private set; }

		private Schedule(ScheduleType type) {
			this.Type = type;
		}

		public int Frequency { get; private set; }
		public TimeSpan RunAt { get; private set; }
		public DateTime LastRun { get; private set; }
		public bool Async { get; set; }

		public Schedule RunAsync(bool value)
		{
			Async = value;
			return this;
		}


		// Daily
		// ---------------
		// Every Day
		// Week Days
		// Every X Days
		// Specific Days

		// Weekly

		/// <summary>
		/// Creates a Schedule which runs Daily at a specific time
		/// </summary>
		/// <param name="runat">The time at which the schedule should become active</param>
		/// <returns>The new Schedule</returns>
		public static Schedule At(TimeSpan runat)
		{
			return new Schedule(ScheduleType.Scheduled) { RunAt = runat };
		}

		/// <summary>
		/// Creates a Schedule which runs Daily at a specific time
		/// </summary>
		/// <param name="runat">The time at which the schedule should become active</param>
		/// <returns>The new Schedule</returns>
		public static Schedule At(int hour, int minute, int second)
		{
			var now = DateTime.UtcNow;
			var lastRun = new DateTime(now.Year, now.Month, now.Day - 1, hour, minute, second);

			return new Schedule(ScheduleType.Periodical)
			{
				Frequency = second + (minute * 60) + (hour * 60 * 60), LastRun = lastRun
			};
		}

		/// <summary>
		/// Creates a Schedule which runs Periodically after a predefined number of seconds
		/// </summary>
		/// <param name="frequency">How many seconds should pass before the schedule becomes active</param>
		/// <param name="lastRun">The date and time the task was last run, this will affect when this schedule will start for the first time</param>
		/// <returns>The new schedule</returns>
		public static Schedule Every(int frequency, DateTime lastRun) {
			return new Schedule(ScheduleType.Periodical) { Frequency = frequency, LastRun = lastRun };
		}

		public static Schedule Every(TimeSpan frequency) {
			return new Schedule(ScheduleType.Periodical) { Frequency = (int) frequency.TotalSeconds };
		}

		/// <summary>
		/// Creates a Schedule which runs Periodically after a predefined number of seconds
		/// </summary>
		/// <param name="frequency">How many seconds should pass before the schedule becomes active</param>
		/// <returns>The new schedule</returns>
		public static Schedule Every(int frequency)
		{
			return new Schedule(ScheduleType.Periodical) { Frequency = frequency };
		}

		/// <summary>
		/// Creates a Schedule which is manually managed by the task itself
		/// </summary>
		/// <returns>The new Schedule</returns>
		public static Schedule Task()
		{
			return new Schedule(ScheduleType.Task);
		}
	}
}
