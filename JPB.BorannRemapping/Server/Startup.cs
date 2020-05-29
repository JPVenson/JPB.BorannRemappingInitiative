using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using JPB.BorannRemapping.Server.Data;
using JPB.BorannRemapping.Server.Data.AppContext;
using JPB.BorannRemapping.Server.Data.Frontier;
using JPB.BorannRemapping.Server.Migrations;
using JPB.BorannRemapping.Server.Models;
using JPB.BorannRemapping.Server.Services.AutoMapper;
using JPB.BorannRemapping.Server.Services.EliteCompanion;
using JPB.BorannRemapping.Server.Services.Mail;
using JPB.BubbleSearch;
using JPB.Katana.CommonTasks.TaskScheduling;
using JPB.MyWorksheet.Shared.Services.ScheduledTasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using reCAPTCHA.AspNetCore;

namespace JPB.BorannRemapping.Server
{
	public class CProfileService : DefaultProfileService
	{
		public CProfileService(ILogger<DefaultProfileService> logger) : base(logger)
		{
		}

		public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			context.IssuedClaims ??= new List<Claim>();
			await base.GetProfileDataAsync(context);
			//add your role claims 
			context.IssuedClaims.AddRange(context.Subject.FindAll(JwtClaimTypes.Role));
			context.IssuedClaims.Add(context.Subject.FindFirst(FrontierClaims.CommanderName) ?? new Claim(FrontierClaims.CommanderName, "..."));
			await Task.CompletedTask;
		}
	}

	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			var connectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<ApplicationDbContext>(
				options => options.UseSqlServer(connectionString, f => f.UseNetTopologySuite()));
			services.AddDbContext<AppDbContext>(
				options => options.UseSqlServer(connectionString));

			services.AddDefaultIdentity<ApplicationUser>(options =>
				{
					options.SignIn.RequireConfirmedAccount = false;
				})
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			services.AddIdentityServer()
				.AddApiAuthorization<ApplicationUser, ApplicationDbContext>()
				.AddProfileService<CProfileService>();

			services.AddTransient<IEmailSender, TwilloMailService>();
			services.AddSingleton<UserLocationTracker>();
			services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(expression =>
			{
				expression.AddProfile(new EFModelMapProfile());
			})));

			services.Configure<AuthMessageSenderOptions>(Configuration);

			// Or configure recaptcha via options
			services.AddRecaptcha(Configuration.GetSection("RecaptchaSettings"));
			//IdentityModelEventSource.ShowPII = true;
			services.AddAuthorization(options =>
			{
				options.AddPolicy("IsReviewer", builder =>
				{
					builder.RequireRole("Reviewer");
				});
			});
			services.AddAuthentication(x =>
				{

				})
				.AddIdentityServerJwt()
				.AddCookie()
				.AddOAuth("Frontier", "Frontier", options =>
				{
					var httpsAuthFrontierstoreNet = "https://auth.frontierstore.net";
					options.AuthorizationEndpoint = $"{httpsAuthFrontierstoreNet}/auth";
					options.Scope.Add("auth");
					options.Scope.Add("capi");

					options.CallbackPath = "/auth-callback/frontier";
					options.ClientId =
						"24592b33-5595-4fa8-b7ec-bbcc10b83755"; //its ok, in the end this is publicly readable
					options.ClientSecret = "None";
					options.TokenEndpoint = $"{httpsAuthFrontierstoreNet}/token";
					options.UserInformationEndpoint = $"{httpsAuthFrontierstoreNet}/me";

					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "customer_id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Name, "firstname");
					options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
					options.UsePkce = true;

					options.Events = new OAuthEvents
					{
						OnRemoteFailure = async context =>
						{

						},
						OnAccessDenied = async context =>
						{

						},
						OnTicketReceived = async context =>
						{
							var identity = context.Principal.Identity as ClaimsIdentity;
							identity.AddClaim(new Claim("NOOOOOOOOOOOOOOPE", "Value"));
						},
						OnCreatingTicket = async context =>
						{
							// Get user info from the userinfo endpoint and use it to populate user claims
							var request =
								new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
							request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
							request.Headers.Authorization =
								new AuthenticationHeaderValue("Bearer", context.AccessToken);

							var response = await context.Backchannel.SendAsync(request,
								HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
							response.EnsureSuccessStatusCode();

							var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
							context.RunClaimActions(user.RootElement);

							var frontierApi = new FrontierApi();
							var dataFor = await frontierApi.GetDataFor(context.AccessToken);
							context.Identity.AddClaim(new Claim(FrontierClaims.CommanderName, dataFor.Commander.Name));
							context.Identity.AddClaim(new Claim("frontier:user:commanderId",
								dataFor.Commander.Id.ToString()));
							context.Identity.AddClaim(new Claim("frontier:token", context.AccessToken));
							context.Identity.AddClaim(new Claim("frontier:refreshtoken", context.RefreshToken));
							context.Identity.AddClaim(new Claim("frontier:tokenvalid",
								(context.ExpiresIn?.TotalSeconds ?? 3600).ToString()));
						},
						OnRedirectToAuthorizationEndpoint = context =>
						{

							//var codeVerifier = CryptoRandom.CreateUniqueId(32);

							//context.Properties.Items.Add("codeVerifier", codeVerifier);

							//string codeChallenge;
							//using (var sha256 = SHA256.Create())
							//{
							//	var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
							//	codeChallenge = Base64Url.Encode(challengeBytes);
							//}

							//context.RedirectUri += $"&audience=frontier&code_challenge={codeChallenge}&code_challenge_method=S256";

							context.Response.Redirect(context.RedirectUri);
							return Task.CompletedTask;
						},

					};
				});
				
				

			services.AddControllersWithViews();
			services.AddRazorPages();

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings.
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 6;
				options.Password.RequiredUniqueChars = 1;

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				// User settings.
				options.User.AllowedUserNameCharacters =
					"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
				options.User.RequireUniqueEmail = false;
			});

			//services.ConfigureApplicationCookie(options =>
			//{
			//	// Cookie settings
			//	options.Cookie.HttpOnly = true;
			//	options.ExpireTimeSpan = TimeSpan.FromHours(5);

			//	options.LoginPath = "/Identity/Account/Login";
			//	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
			//	options.SlidingExpiration = true;
			//});

			services.AddSingleton(provider =>
			{
				var service = provider.GetService<ILogger<ITask>>();
				var scheduler = new Scheduler(service);

				scheduler.AddByAttributes(Assembly.GetCallingAssembly());
				scheduler.AddByAttributes(typeof(BaseTask).Assembly);

				scheduler.OnFailedTask += (task, exception, handler) =>
				{
					service.LogError("The task with the name " + task.NamedTask + " has failed",
						new Dictionary<string, string>
						{
							{"Exception", exception.ToString()}
						});
				};
				scheduler.Start();
				return scheduler;
			});

		}

		private async Task AddRoles(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

				var roleNames = new string[] { "Admin", "User", "Reviewer" };

				foreach (var roleName in roleNames)
				{
					var roleExist = await roleManager.RoleExistsAsync(roleName);
					if (!roleExist)
					{
						//create the roles and seed them to the database: Question 2
						await roleManager.CreateAsync(new IdentityRole(roleName));
					}
				}	
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseIdentityServer(new IdentityServerMiddlewareOptions()
			{
				AuthenticationMiddleware = f =>
				{
					
				}
			});
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
			
			AddRoles(app.ApplicationServices).Wait();
			//MigrateSystems(app);
		}

		private static void MigrateSystems(IApplicationBuilder app)
		{
			using (var scope = app.ApplicationServices.CreateScope())
			{
				var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
				//var systemModels = InitAppDb.GetSystems();
				var systemsOfIntereset = InitAppDb.GetTargetSystems();

				foreach (var systemModel in systemsOfIntereset)
				{
					var system = new Data.AppContext.System
					{
						Name = systemModel.Name,
						ExtRelId = systemModel.Id,
						ExtRelId64 = systemModel.Id64,
						X = systemModel.Coordinates.X,
						Y = systemModel.Coordinates.Y,
						Z = systemModel.Coordinates.Z
					};
					system.SystemBody = systemModel.SystemBodies?
						.Where(f =>
						{
							return (f.ReserveLevel == "Pristine" || string.IsNullOrWhiteSpace(f.ReserveLevel))
							       && 
							       f.Rings?.Any(FilterRing) == true;
						})
						.Select(f =>
					{
						return new SystemBody
						{
							BodyId = f.BodyId?.ToString(),
							DistanceToArrival = f.DistanceToArrival,
							ExtRelId = f.Id,
							ExtRelId64 = f.Id64 ?? 0,
							Name = f.Name,
							SubType = f.SubType,
							Type = f.Type,
							SystemBodyRing = f.Rings?.Where(FilterRing)
								.Select(e =>
							{
								return new SystemBodyRing
								{
									Type = e.Type,
									Name = e.Name
								};
							}).ToArray()
						};
					}).ToArray();
					appDbContext.Add(system);
				}

				appDbContext.SaveChanges();
			}
		}

		private static bool FilterRing(RingModel e)
		{
			return e.Type == "Icy" || string.IsNullOrWhiteSpace(e.Type);
		}
	}
}