using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
				var resultBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(steamId));
				var resultChars = Convert.ToBase64String(resultBytes).ToCharArray();

				for (int i = 0; i < resultChars.Length; i++)
				{
					resultChars[i] = resultChars[i] switch
					{
						'/' => '_',
						'+' => '-',
						_ => resultChars[i],
					};
				}

				return Ok(new String(resultChars));
			}
		}

		private record VppBanList
		{
			[JsonPropertyName("m_BanList")]
			public VppBanRecord[] Items { get; set; }
		}

		private record VppBanRecord
		{
			public string PlayerName { get; set; }
            public string Steam64Id { get; set; }
            public string GUID { get; set; }
            public string BanReason { get; set; }
            public string IssuedBy { get; set; }
			public VppBanRecordExpirationDate ExpirationDate { get; set; }
		}

		private record VppBanRecordExpirationDate
		{
			public int Hour { get; set; }
			public int Minute { get; set; }
			public int Year { get; set; }
			public int Month { get; set; }
			public int Day { get; set; }
			public int Permanent { get; set; }
		}

		public record BanListFile
		{
			public IFormFile File { get; set; }
		}

		[HttpPost]
		[Route("[action]")]
		public async Task ConvertBanList([FromForm] BanListFile form)
		{
			string[] banGUIDs = null;

			using (var stream = form.File.OpenReadStream())
			{
				var banList = JsonSerializer.Deserialize<VppBanList>(
					stream,
					new JsonSerializerOptions()
					{
						PropertyNameCaseInsensitive = true
					});

				banGUIDs = banList.Items
					.Where(x => x.ExpirationDate.Permanent == 1)
					.Select(x => x.GUID)
					.ToArray();
			}

			HttpContext.Response.ContentType = "text/plain";
			HttpContext.Response.StatusCode = 200;
			await HttpContext.Response.StartAsync();

			using (var stream = HttpContext.Response.BodyWriter.AsStream())
			{
				using (TextWriter textWriter = new StreamWriter(stream, Encoding.UTF8))
				{
					foreach (var guid in banGUIDs)
					{
						textWriter.WriteLine(guid);
					}
				}

				await stream.FlushAsync();
			}
			
			await HttpContext.Response.CompleteAsync();
		}
	}
}
