using System.Collections.Concurrent;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Services.Identity;

namespace NostrConnect.Web.Services.Identity
{
	public interface IWebIdentityService : IIdentityService
	{
		Task<string> CreateNewSession();
	}
}