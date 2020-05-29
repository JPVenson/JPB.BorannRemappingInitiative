using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JPB.BorannRemapping.Shared.ViewModels;

namespace JPB.BorannRemapping.Server.Services.AutoMapper
{
	public class EFModelMapProfile : Profile
	{
		public EFModelMapProfile()
		{
			AddMaps();
		}

		private void AddMaps()
		{
			var entitesNamespace = typeof(Data.AppContext.System).Namespace;
			var vmNamespace = typeof(SystemLookupViewModel).Namespace;

			var selectMany = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(f => f.GetTypes())
				.ToArray();
			foreach (var vmType in 
				selectMany
					.Where(e => e.Namespace == vmNamespace))
			{
				var possibleTypes = selectMany.Where(e => e.Namespace == entitesNamespace)
					.Where(e => vmType.Name.StartsWith(e.Name));
				foreach (var possibleType in possibleTypes)
				{
					CreateMap(possibleType, vmType)
						.ReverseMap();
				}
			}
		}
	}
}
