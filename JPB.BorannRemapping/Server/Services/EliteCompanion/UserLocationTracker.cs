using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.BorannRemapping.Server.Data.AppContext;
using JPB.BorannRemapping.Server.Data.Frontier;

namespace JPB.BorannRemapping.Server.Services.EliteCompanion
{
	public class ResultLookup
	{
		public DateTimeOffset NextLookupIn { get; set; }
		public string SystemName { get; set; }
	}

	public class UserLocationTracker
	{
		public UserLocationTracker()
		{
			FrontierApi = new FrontierApi();
			UserLocations = new ConcurrentDictionary<string, UserLocation>();
		}

		public ConcurrentDictionary<string, UserLocation> UserLocations { get; set; }

		public FrontierApi FrontierApi { get; set; }

		public async Task<ResultLookup> LastKnownLocation(string userId, AppDbContext context)
		{
			if (UserLocations.TryGetValue(userId, out var location))
			{
				var nextLookupPossible = location.LastUpdated.AddMinutes(2);
				if (nextLookupPossible > DateTimeOffset.UtcNow)
				{
					return new ResultLookup()
					{
						SystemName = location.SystemName,
						NextLookupIn = nextLookupPossible
					};
				}
			}

			if (location == null)
			{
				location = new UserLocation();
				UserLocations.TryAdd(userId, location);
			}
			var eliteData = await FrontierApi.GetDataFor(context, userId);
			location.LastUpdated = DateTimeOffset.UtcNow;
			location.UserId = userId;
			location.SystemName = eliteData.LastSystem?.Name.ToString();
			return new ResultLookup()
			{
				SystemName = location.SystemName,
				NextLookupIn = location.LastUpdated.AddMinutes(2)
			};
		}
	}

	public class UserLocation
	{
		public string UserId { get; set; }
		public string SystemName { get; set; }
		public DateTimeOffset LastUpdated { get; set; }
	}
}
