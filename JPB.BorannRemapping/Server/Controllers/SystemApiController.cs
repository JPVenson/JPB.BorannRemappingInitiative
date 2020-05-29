using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using JPB.BorannRemapping.Server.Data.AppContext;
using JPB.BorannRemapping.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace JPB.BorannRemapping.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SystemApiController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IMapper _mapper;

		public SystemApiController(AppDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[Route("SearchSystems")]
		public ActionResult GetSystemLookup(string systemName)
		{
			var lookupSystems = _mapper.ProjectTo<SystemLookupViewModel>(_context.System
				.Where(e => e.Name.Contains(systemName))
				.Take(10)).ToArray();
			return Ok(lookupSystems);
		}

		[Route("Search")]
		public ActionResult GetSystem(long systemId)
		{
			var lookupSystems = _mapper.Map<SystemLookupViewModel>(_context.System
				.Find(systemId));
			return Ok(lookupSystems);
		}

		[Route("NextPossibleSystem")]
		public ActionResult FindNextPossibleSystem(long systemId)
		{
			var sys = _context.System
				.Find(systemId);
			var idSubmittingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var sysX = sys.X;
			var sysY = sys.Y;
			var sysZ = sys.Z;
			var nextSystemToExplore = _context.System
				.Where(e => !e.Explored.HasValue || e.Explored < 3)
				//.Where(e => e.SystemBody.Any(f => f.SystemBodyRing.Any(g => g.Type == "Icy")))
				.Where(e => e.SystemBody.Any(f => f.SystemBodyRing.Any(g => g.Type == "Icy") 
				                                   && 
				                                   f.SystemEval.All(w => w.IdSubmittingUser != idSubmittingUser)))
				.OrderBy(f => Math.Sqrt((Math.Pow((f.X - sysX), 2) +
				                         Math.Pow((f.Y - sysY), 2) +
				                         Math.Pow((f.Z - sysZ), 2))))
				.FirstOrDefault();

			var nextSystem = _context.System
				.Include(f => f.SystemBody)
				.ThenInclude(f => f.SystemBodyRing)
				.Select(f => new
				{
					System = f,
					SystemBodies = f
						.SystemBody
						.Where(e => e.SystemEval.All(w => w.IdSubmittingUser != idSubmittingUser))
						.Where(e => e.SystemBodyRing.Any(w => w.Type == "Icy"))
				})
				.FirstOrDefault(f => f.System.SystemId == nextSystemToExplore.SystemId);
			var systemVm = _mapper.Map<SystemViewModel>(nextSystem.System);
			systemVm.SystemBody = _mapper.Map<List<SystemBodyViewModel>>(nextSystem.SystemBodies);
			return Ok(new NextSystemEval()
			{
				TargetSystem = systemVm,
				NoBodies = nextSystem.System.SystemBody.Count,
				DistanceFromReferencePoint = Math.Sqrt((Math.Pow((nextSystemToExplore.X - sysX), 2) +
				                                        Math.Pow((nextSystemToExplore.Y - sysY), 2) +
				                                        Math.Pow((nextSystemToExplore.Z - sysZ), 2)))
			});
		}
	}

	/*
	 *var nextSystem = _context.System
				.Select(f => new
				{
					System = f,
					SystemBodies = f
						.SystemBody
						.Where(e => e.SystemEval.All(w => w.IdSubmittingUser != idSubmittingUser))
						.Where(e => e.SystemBodyRing.Any(w => w.Type == "Icy"))
				})
				.FirstOrDefault(f => f.System.SystemId == nextSystemToExplore.SystemId);
			var systemVm = _mapper.Map<SystemViewModel>(nextSystem.System);
			systemVm.SystemBody = _mapper.Map<List<SystemBodyViewModel>>(nextSystem.SystemBodies);
			return Ok(new NextSystemEval()
			{
				TargetSystem = systemVm
			});
	 *
	 */
}