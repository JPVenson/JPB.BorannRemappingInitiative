using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace JPB.BubbleSearch
{
	public class SystemModel
	{
		public string Name { get; set; }

		[JsonPropertyAttribute("coords")]
		public CoordinatesModel Coordinates { get; set; }
		public int Id { get; set; }
		public long Id64 { get; set; }
		[JsonProperty("bodies")]
		public SystemBodyModel[] SystemBodies { get; set; }

		public bool InRangeOf(CoordinatesModel coords, float distance)
		{
			return Distance(coords) < distance;
		}

		public double Distance(CoordinatesModel coords)
		{
			return Math.Sqrt(Coordinates.Distance()
				.Zip(coords.Distance(), (a, b) => (a - b) * (a - b)).Sum());
		}
	}

	public class SystemBodyModel
	{
		public int Id { get; set; }
		public long? Id64 { get; set; }
		public int? BodyId { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string SubType { get; set; }
		public long DistanceToArrival { get; set; }
		public int SystemId { get; set; }
		public long SystemId64 { get; set; }
		public RingModel[] Rings { get; set; }
		public string ReserveLevel { get; set; }
	}

	public class RingModel
	{
		public string Name { get; set; }
		public string Type { get; set; }
	}

	public class CoordinatesModel
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public float[] Distance()
		{
			return new float[] { X, Y, Z };
		}
	}
}
