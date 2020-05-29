using System;
using System.Collections.Generic;
using System.Text;

namespace JPB.BorannRemapping.Shared.ViewModels
{
	public class SystemEvalPublicViewModel
	{
		public SystemEvalViewModel[] LatestEvalas { get; set; }
	}

	public class SystemEvalViewModel
	{
		public DateTimeOffset DateOfCreation { get; set; }
		public string UserName { get; set; }
		public string SystemName { get; set; }
	}

	public class EvaluatedSystemsInfoViewModel
	{
		public long SystemsKnown { get; set; }
		public int Submissions { get; set; }
		public int ExploredSystems { get; set; }
		public int SystemsToExplore { get; set; }
		public int ExploredBodies { get; set; }
	}
}
