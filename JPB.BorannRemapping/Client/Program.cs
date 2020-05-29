using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Blazored.LocalStorage;
using JPB.BorannRemapping.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JPB.BorannRemapping.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");

			builder.Services.AddHttpClient("JPB.BorannRemapping.ServerAPI", client =>
				{
					client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
				})
				.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
			builder.Services.AddHttpClient("JPB.BorannRemapping.ServerAPI.Anonymous", client =>
			{
				client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
			});

			// Supply HttpClient instances that include access tokens when making requests to the server project
	
			builder.Services.AddSingleton(e => new HttpService(e.GetRequiredService<IHttpClientFactory>().CreateClient("JPB.BorannRemapping.ServerAPI")));
			builder.Services.AddSingleton(e => new HttpServiceAnonymous(e.GetRequiredService<IHttpClientFactory>().CreateClient("JPB.BorannRemapping.ServerAPI.Anonymous")));
			builder.Services.AddApiAuthorization();
			builder.Services.AddBlazoredLocalStorage();
			
			await builder.Build().RunAsync();
		}
	}
}
