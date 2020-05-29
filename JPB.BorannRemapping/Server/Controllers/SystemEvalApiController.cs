using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.Extensions;
using JPB.BorannRemapping.Server.Data.AppContext;
using JPB.BorannRemapping.Server.Services.EliteCompanion;
using JPB.BorannRemapping.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JPB.BorannRemapping.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SystemEvalApiController : ControllerBase
	{
		private readonly AppDbContext _db;
		private readonly IMapper _mapper;
		private readonly UserLocationTracker _userTracker;

		public SystemEvalApiController(AppDbContext db, IMapper mapper, UserLocationTracker userTracker)
		{
			_db = db;
			_mapper = mapper;
			_userTracker = userTracker;
		}

		[Route("ReviewQueue")]
		[Authorize(Policy = "IsReviewer")]
		public ActionResult ReviewQueue()
		{
			var nextEval = _db.SystemEval
				.Include(f => f.IdSubmittingUserNavigation)
				.Include(f => f.IdSystemBodyNavigation)
				.FirstOrDefault(e => e.IsReviewed == false && e.State == true);

			var systemEvalReviewViewModel = _mapper.Map<SystemEvalReviewViewModel>(nextEval);
			systemEvalReviewViewModel.IdSubmittingUser = nextEval.IdSubmittingUserNavigation.CommanderName;
			return Ok(systemEvalReviewViewModel);
		}

		[Route("SubmitReview")]
		[Authorize(Policy = "IsReviewer")]
		public ActionResult ReviewSubmission(long submissionId, bool state, string comment)
		{
			var nextEval = _db.SystemEval
				.FirstOrDefault(e => e.SubmissionId == submissionId);
			nextEval.IsReviewed = state;
			nextEval.ReviewComment = comment;

			_db.SaveChanges();
			return Ok();
		}

		[Route("LatestEvals")]
		[AllowAnonymous]
		public ActionResult LatestEvals()
		{
			var systemEvals = _db
				.SystemEval
				.OrderBy(e => e.DateOfCreation)
				.Take(20)
				.Select(f => new
				{
					DateOfCreation = f.DateOfCreation,
					UserName = f.IdSubmittingUserNavigation.CommanderName,
					SystemName = f.IdSystemBodyNavigation.Name
				})
				.ToArray()
				.Select(f => new SystemEvalViewModel
				{
					DateOfCreation = f.DateOfCreation,
					UserName = f.UserName,
					SystemName = f.SystemName
				})
				.ToArray();

			return Ok(new SystemEvalPublicViewModel()
			{
				LatestEvalas = systemEvals
			});
		}

		[Route("MarkSystemBodyAs")]
		[Authorize]
		public async Task<ActionResult> MarkSystemBodyAs(
			long systemBodyId, 
			string comment, 
			bool isPointOfInterest,
			string provingUrl = null)
		{
			var idSubmittingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var previousSubmission = _db
				.SystemEval
				.Where(e => e.IdSubmittingUser == idSubmittingUser)
				.OrderBy(e => e.DateOfCreation)
				.FirstOrDefault();
			if (previousSubmission != null)
			{
				if (previousSubmission.DateOfCreation.AddMinutes(2) > DateTimeOffset.UtcNow)
				{
					return new StatusCodeResult(StatusCodes.Status429TooManyRequests);
				}
			}

			var system = _db.System.FirstOrDefault(e => e.SystemBody.Any(f => f.SystemBodyId == systemBodyId));

			var lastKnownLocation = await _userTracker.LastKnownLocation(idSubmittingUser, _db);
			if (lastKnownLocation.SystemName != system.Name)
			{
				return BadRequest(
					$"You are not listed to be in the system '{lastKnownLocation.SystemName}' please try again at '{lastKnownLocation.NextLookupIn:G}'");
			}

			_db.SystemEval.Add(new SystemEval()
			{
				IdSystemBody = systemBodyId,
				Comment = comment,
				DateOfCreation = DateTimeOffset.UtcNow,
				IdSubmittingUser = idSubmittingUser,
				State = isPointOfInterest,
				ProveImage = provingUrl
			});

			await _db.SaveChangesAsync();
			if (_db.System
				.Where(e => e.SystemBody.Any(f => f.SystemBodyId == systemBodyId))
				.All(f =>
					f.SystemBody
						.Where(f => f.SystemBodyRing.Any(g => g.Type == "Icy"))
						.All(e => e.SystemEval.Any(g => g.IdSubmittingUser == idSubmittingUser))))
			{
				var firstOrDefault = _db.System.FirstOrDefault(e => e.SystemBody.Any(f => f.SystemBodyId == systemBodyId));
				await _db.Database.ExecuteSqlInterpolatedAsync(
					$"UPDATE [System] SET [System].[Explored] = [System].[Explored] + 1 WHERE [System].[SystemId] = {firstOrDefault.SystemId}");
			}
			return Ok();
		}

		[AllowAnonymous]
		[Route("GetSystemsStatus")]
		public ActionResult GetEvaluatedSystemsInfo()
		{
			var systemsCount = _db.System.Count();
			var submissions = _db.SystemEval.Count();
			var exploredBodies = _db.SystemBody.Count(f => f.SystemEval.Any());
			var systemsExplored = _db.System.Count(f => f.SystemBody.Any(e => e.SystemBodyRing.Any())
			                                            && f.Explored.HasValue);
			var systemsToExplore = _db.System.Count(f => f.SystemBody.Any(e => e.SystemBodyRing.Any()));

			return Ok(new EvaluatedSystemsInfoViewModel()
			{
				SystemsKnown = systemsCount,
				Submissions = submissions,
				SystemsToExplore = systemsToExplore,
				ExploredSystems = systemsExplored,
				ExploredBodies = exploredBodies
			});
		}
	}
}