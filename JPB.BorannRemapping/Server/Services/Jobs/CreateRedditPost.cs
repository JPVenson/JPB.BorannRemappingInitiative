using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.Katana.CommonTasks.TaskScheduling;
using JPB.MyWorksheet.Shared.Services.ScheduledTasks;
using Microsoft.Extensions.Logging;
using Reddit;

namespace JPB.BorannRemapping.Server.Services.Jobs
{
	//[ScheduleTaskAt(0, 0, 0)]
	//public class CreateRedditPost : BaseTask
	//{
	//	public override string NamedTask { get; protected set; } = "CreateRedditDailyPost";

	//	public override async Task DoWorkAsync(ILogger<ITask> logger)
	//	{
	//		var reddit = new RedditClient("YourRedditAppID", "YourBotUserRefreshToken");
	//		reddit.Subreddit("r/BorannRemapping")
	//			.SelfPost("Weekly Update", "TestPost", null, "BorannBot");
	//	}
	//}
}
