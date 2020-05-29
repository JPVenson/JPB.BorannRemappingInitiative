using System;
using System.Collections.Generic;

namespace JPB.BorannRemapping.Shared.ViewModels 
{
	public class SystemEvalReviewViewModel
	{
		public long SubmissionId { get; set; }
		public bool State { get; set; }
		public string Comment { get; set; }
		public long IdSystemBody { get; set; }
		public string IdSubmittingUser { get; set; }
		public DateTimeOffset DateOfCreation { get; set; }
		public string ProveImage { get; set; }
		public string CommanderName { get; set; }
		public SystemBodyViewModel IdSystemBodyNavigation { get; set; }
	}

	public class SystemLookupViewModel
	{
		public long SystemId { get; set; }
		public string Name { get; set; }
		public long ExtRelId64 { get; set; }
	}

	public class SystemViewModel : SystemLookupViewModel
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public List<SystemBodyViewModel> SystemBody { get; set; }
	}

	public class SystemBodyViewModel
	{
		public long SystemBodyId { get; set; }
		public string Name { get; set; }
		public int ExtRelId { get; set; }
		public long ExtRelId64 { get; set; }
		public string BodyId { get; set; }
		public string Type { get; set; }
		public string SubType { get; set; }
		public long DistanceToArrival { get; set; }

		public SystemBodyRingViewModel[] SystemBodyRing { get; set; }
	}

	public class SystemBodyRingViewModel
	{
		public long SystemBodyRingId { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
	}

	public class NextSystemEval
	{
		public SystemViewModel TargetSystem { get; set; }
		public int NoBodies { get; set; }
		public double DistanceFromReferencePoint { get; set; }
	}
}
