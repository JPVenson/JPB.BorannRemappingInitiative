using System;
using System.Collections.Generic;
using System.Text;

namespace JPB.BorannRemapping.Shared.ViewModels
{
	public class CurrentSystemLookupViewModel
	{
		public string CurrentSystem { get; set; }
		public SystemViewModel SystemViewModel { get; set; }
		public DateTimeOffset NextPossibleLookup { get; set; }
	}
}
