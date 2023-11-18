using System.Text.Json;
using MediatR;

namespace DZALT.Entities.Selection.PlayTimeByPlayer
{
	public record PlayTimeByPlayerQuery : IRequest<PlayTimeByPlayerResult[]>
	{
		public static readonly PlayTimeByPlayerQuery Instance = new PlayTimeByPlayerQuery();

		private PlayTimeByPlayerQuery()
		{
		}
	}
}
