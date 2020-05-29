using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JPB.Katana.CommonTasks.TaskScheduling.TaskRunners;
using Microsoft.Extensions.Logging;

namespace JPB.Katana.CommonTasks.TaskScheduling
{
    /// <summary>
    ///     Manages Tasks and the schedule that defines when they will run in the background
    /// </summary>
    public class Scheduler : IScheduler, IDisposable
    {
		/// <summary>
		/// When this token is set it indicates a stop operation is in progress and the interal Task should be stopped
		/// </summary>
		private readonly CancellationTokenSource _innerControlToken;
        private readonly ILogger<ITask> _logger;
		/// <summary>
		/// If the event is set and we are currently in a pending wait operation, this operation will be stopped.
		/// </summary>
		private readonly AutoResetEvent _reinvalidateEvent;
        private readonly Thread _runner;
        private readonly IList<ITaskRunner> _taskRunners;

        public Scheduler(ILogger<ITask> logger) : this(CancellationToken.None, logger)
        {
        }

        /// <summary>
        ///     Instantiate a new Scheduler to run background tasks
        /// </summary>
        public Scheduler(CancellationToken cancellationToken, ILogger<ITask> logger)
        {
            _logger = logger;
            _taskRunners = new List<ITaskRunner>();

            CancellationToken = cancellationToken;
            _innerControlToken = new CancellationTokenSource();
            _reinvalidateEvent = new AutoResetEvent(false);
            _runner = new Thread(RunScheduler);
        }

        public CancellationToken CancellationToken { get; }

        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Close and clean up any opened resources
        /// </summary>
        public void Dispose()
        {
            // ensure the timer is stopped
            Stop();

            // clean up any tasks which are disposable
            foreach (var runner in _taskRunners)
            {
                if (runner.Task is IDisposable)
                {
                    ((IDisposable)runner.Task).Dispose();
                }
            }
        }
        /// <summary>
        /// Gets the next interation.
        /// </summary>
        /// <value>
        /// The next interation.
        /// </value>
        public TimeSpan NextInteration
        {
            get
            {
                return _evaluatedTo - DateTime.UtcNow;
            }
        }

        private DateTime _evaluatedTo;

        /// <summary>
        /// Tries the add task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="reinvalidate">The reinvalidate.</param>
        /// <returns></returns>
        public DynamicTaskRunner TryAddTask(ITask task, Func<DateTime> reinvalidate)
        {
            var defedRunner = new DynamicTaskRunner(task, reinvalidate, this);
            _taskRunners.Add(defedRunner);
            return defedRunner;
        }

        /// <summary>
        ///     Adds a task onto the schedule so it will be checked and run as defined by the schedule
        /// </summary>
        /// <param name="task">The task to run</param>
        /// <param name="schedule">The schedule that determines when the task should run</param>
        public bool TryAddTask(ITask task, Schedule schedule)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            if (IsRunning)
            {
                return false;
            }

            switch (schedule.Type)
            {
                case ScheduleType.Periodical:
                    var p = new PeriodicalTaskRunner(task, schedule.Frequency);
                    _taskRunners.Add(p);
                    break;

                case ScheduleType.Scheduled:
                    var s = new ScheduledTaskRunner(task, schedule.RunAt);
                    _taskRunners.Add(s);
                    break;

                case ScheduleType.Task:
                    var neverRunner = new OnDemandTaskRunner(task);
                    _taskRunners.Add(neverRunner);
                    break;
                case ScheduleType.Defered:
             
                    break;
                default:
                    throw new Exception(schedule.Type + " is not a supported schedule type.");
            }

            return true;
        }

        /// <summary>
        ///     Start checking if schedules are due to run their tasks
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            lock (this)
            {
                _runner.Start();
            }
        }

        /// <summary>
        ///     Stop schedules checking if they are due to run
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            lock (this)
            {
                _innerControlToken.Cancel();
            }
        }

        public event FailedTaskEventHandler OnFailedTask;

        /// <summary>
        /// Reinvalidates this instance. This will stop any pending wait operation and will ask all tasks for changed determinatedNextRun times.
        /// </summary>
        public void Reinvalidate()
        {
            _reinvalidateEvent.Set();
        }

        /// <summary>
        /// Reinvalidates this instance. This will stop any pending wait operation and will ask all tasks for changed determinatedNextRun times.
        /// </summary>
        public void Reinvalidate(DateTime future)
        {
            if (future < _evaluatedTo)
            {
                Reinvalidate();
            }
        }

        /// <summary>
        /// Returns a list of all Runners
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<ITaskRunner> Runners()
        {
            return _taskRunners.ToArray();
        }

        ~Scheduler()
        {
            Dispose();
        }

        private void RunScheduler()
        {
            IsRunning = true;

            var waitAnyToken =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(_innerControlToken.Token,
                    CancellationToken);

            try
            {
                while (!waitAnyToken.IsCancellationRequested)
                {
                    var maybeNextTaskRunner = _taskRunners.Select(f => f.DetermininateNextRun()).Min();
                    var whenNext = maybeNextTaskRunner ?? TimeSpan.FromMinutes(1);

                    _evaluatedTo = DateTime.UtcNow + whenNext;
                    if (whenNext > TimeSpan.Zero)
                    {
                        var waitTaskForInternal = _innerControlToken.Token.WaitHandle.WaitOneAsync();
                        var waitTaskForExternal = CancellationToken.WaitHandle.WaitOneAsync();
                        var reinvalidateExternal = _reinvalidateEvent.WaitOneAsync();

                        var whenAny = Task.WhenAny(waitTaskForInternal,
                            waitTaskForExternal,
                            Task.Delay(whenNext, _innerControlToken.Token),
                            reinvalidateExternal)
                            .Result;
                        if (whenAny == waitTaskForExternal || whenAny == waitTaskForInternal)
                        {
                            if (_innerControlToken.IsCancellationRequested || CancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                        }

                        if (whenAny == reinvalidateExternal)
                        {
                            continue;
                        }
                    }

                    // run tasks sequentially one ofter the other
                    // could use threads here
                    if (_taskRunners == null || _taskRunners.Count <= 0)
                    {
                        continue;
                    }

                    var tasks = new List<Task>();

                    foreach (var runner in _taskRunners.ToArray())
                    {
                        try
                        {
                            if (runner.Check())
                            {
                                tasks.Add(runner.Execute(_logger));
                            }
                        }
                        catch (Exception e)
                        {
                            OnOnFailedTask(runner.Task, e, new ExceptionHandler());
                        }
                    }

                    var whenAll = Task.WhenAll(tasks);

                    try
                    {
                        whenAll.Wait(_innerControlToken.Token);
                    }
                    catch (Exception)
                    {
                    }

                    // make sure any tasks are running, should be on their own threads
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        protected virtual void OnOnFailedTask(ITask task, Exception exception, ExceptionHandler handler)
        {
            OnFailedTask?.Invoke(task, exception, handler);
        }

        public void AddByAttributes(Assembly inAssembly)
        {
            foreach (var type in inAssembly.GetTypes())
            {
                var scheduleAt = type.GetCustomAttributes<ScheduleAttribute>(true).ToArray();
                if (!scheduleAt.Any())
                {
                    continue;
                }

                var task = Activator.CreateInstance(type) as ITask;
                foreach (var scheduleAttribute in scheduleAt)
                {
                    TryAddTask(task, scheduleAttribute.When());
                }
            }
        }
    }

    public static class WaitHandleExtentions
    {
	    public static Task WaitOneAsync(this WaitHandle waitHandle)
	    {
		    if (waitHandle == null)
			    throw new ArgumentNullException("waitHandle");

		    var tcs = new TaskCompletionSource<bool>();
		    var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
			    delegate { tcs.TrySetResult(true); }, null, -1, true);
		    var t = tcs.Task;
		    t.ContinueWith((antecedent) => rwh.Unregister(null));
		    return t;
	    }
    }
}