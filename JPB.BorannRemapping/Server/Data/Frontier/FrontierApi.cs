using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Threading.Tasks;
using JPB.BorannRemapping.Server.Data.AppContext;
using Newtonsoft.Json.Linq;

namespace JPB.BorannRemapping.Server.Data.Frontier
{
	public class FrontierApi
	{
		public FrontierApi()
		{

		}

		public async Task<EliteData> GetDataFor(string token)
		{
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			try
			{
				return await httpClient.GetFromJsonAsync<EliteData>("https://companion.orerve.net/profile");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		public async Task<EliteData> GetDataFor(AppDbContext db, string userId)
		{
			var frontierOauthToken = db.FrontierOauthToken.FirstOrDefault(e => e.IdUser == userId);
			if (frontierOauthToken.TokenValidUntil < DateTimeOffset.UtcNow)
			{
				var httpClient = new HttpClient();
				var httpResponseMessage = await httpClient.SendAsync(
					new HttpRequestMessage(HttpMethod.Post, "https://auth.frontierstore.net/token")
					{
						Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("grant_type", "refresh_token"),
							new KeyValuePair<string, string>("client_id", "24592b33-5595-4fa8-b7ec-bbcc10b83755"),
							new KeyValuePair<string, string>("refresh_token", frontierOauthToken.RefreshToken),
						})
					});
				var newToken = await httpResponseMessage.Content.ReadAsStringAsync();
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return null;
				}

				var jObject = JObject.Parse(newToken);
				frontierOauthToken.TokenValidUntil = DateTimeOffset.UtcNow.AddSeconds(jObject["expires_in"].ToObject<int>());
				frontierOauthToken.Token = jObject["access_token"].ToString();
				frontierOauthToken.RefreshToken = jObject["refresh_token"].ToString();
				await db.SaveChangesAsync();
			}

			return await GetDataFor(frontierOauthToken.Token);
		}
	}

	public class EliteData
	{
		public CommanderData Commander { get; set; }
		public EliteSystemData LastSystem { get; set; }
	}

	public class EliteSystemData
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Faction { get; set; }
	}

	public class CommanderData
	{
		public string Name { get; set; }
		public long Id { get; set; }
		public bool Docked { get; set; }
		public bool Alive { get; set; }
		public long Credits { get; set; }
	}
}
