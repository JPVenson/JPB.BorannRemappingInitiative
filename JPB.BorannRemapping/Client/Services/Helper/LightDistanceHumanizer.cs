using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.BorannRemapping.Client.Services.Helper
{
	public static class LightDistanceHumanizer
	{
		static LightDistanceHumanizer()
		{
			LightDistances = new Dictionary<double, string>()
			{
				{31557600D, "LY"},
				{604800D, "LW"},
				{86400D, "LD"},
				{3600D, "LH"},
				{60D, "Lm"},
			};
		}

		public static IDictionary<double, string> LightDistances { get; set; }

		public static string Humanize(long distance)
		{
			foreach (var lightDistance in LightDistances)
			{
				if (distance / lightDistance.Key > 1)
				{
					return $"{Math.Round(distance / lightDistance.Key, 2)} {lightDistance.Value}";
				}
			}
			return $"{distance} Ls";
		}

		public static string Humanize(double distance)
		{
			foreach (var lightDistance in LightDistances)
			{
				if (distance / lightDistance.Key > 1)
				{
					return $"{Math.Round(distance / lightDistance.Key, 2)} {lightDistance.Value}";
				}
			}
			return $"{distance} Ls";
		}
	}
}
