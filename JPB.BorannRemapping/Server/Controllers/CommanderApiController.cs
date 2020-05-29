using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using JPB.BorannRemapping.Server.Data.AppContext;
using JPB.BorannRemapping.Server.Models;
using JPB.BorannRemapping.Server.Services.EliteCompanion;
using JPB.BorannRemapping.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JPB.BorannRemapping.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CommanderApiController : ControllerBase
	{
		private readonly UserLocationTracker _tracker;
		private readonly AppDbContext _db;
		private readonly IMapper _mapper;
		private readonly UserManager<ApplicationUser> _userManager;

		public CommanderApiController(UserLocationTracker tracker, 
			AppDbContext db, 
			IMapper mapper,
			UserManager<ApplicationUser> userManager)
		{
			_tracker = tracker;
			_db = db;
			_mapper = mapper;
			_userManager = userManager;
		}

		[Route("GetPosition")]
		public async Task<ActionResult> GetCurrentPosition()
		{
			var lastKnownLocation = await _tracker.LastKnownLocation(User.FindFirstValue(ClaimTypes.NameIdentifier), _db);
			var knownSystem = _db.System.FirstOrDefault(e => e.Name == lastKnownLocation.SystemName);
			return Ok(new CurrentSystemLookupViewModel()
			{
				SystemViewModel = _mapper.Map<SystemViewModel>(knownSystem),
				CurrentSystem = lastKnownLocation.SystemName,
				NextPossibleLookup = lastKnownLocation.NextLookupIn
			});
		}

		[Route("Me")]
		public async Task<ActionResult> Me()
		{
			var userData = new UserData();
			var findFirstValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var claimsAsync = await _userManager.GetClaimsAsync(new ApplicationUser()
			{
				Id = findFirstValue
			});
			
			userData.CommanderName = claimsAsync.FirstOrDefault(e => e.Type == FrontierClaims.CommanderName)?.Value;
			userData.UserName = claimsAsync.FirstOrDefault(e => e.Type == ClaimTypes.Name)?.Value;
			return Ok(userData);
		}
	}
}
