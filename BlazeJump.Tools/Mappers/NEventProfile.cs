using AutoMapper;
using BlazeJump.Tools.Models;

namespace BlazeJump.Tools.Mappers
{
	/// <summary>
	/// AutoMapper profile for mapping between NEvent-related objects.
	/// </summary>
	public class NEventProfile : Profile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NEventProfile"/> class and configures mappings.
		/// </summary>
		public NEventProfile()
		{
			CreateMap<NEvent, NEvent>();
			CreateMap<EventTag, EventTag>();
			CreateMap<NEvent, SignableNEvent>()
				.ForMember(x => x.Id, opt => opt.Ignore())
				.ReverseMap();
		}
	}
}
