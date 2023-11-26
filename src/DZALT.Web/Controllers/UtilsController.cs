using System;
using System.Security.Cryptography;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DZALT.Web.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class UtilsController : ControllerBase
	{
		[HttpGet]
		[Route("[action]")]
		public IActionResult PlayerGuid(
			[FromQuery] string steamId)
		{
			using (var hash = SHA256.Create())
			{
				byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(steamId));

				return Ok(Convert.ToBase64String(result));
			}
		}
	}
}
