using System.Collections.Generic;
using MediatR;

namespace DZALT.Entities.Selection.NamesByPlayer
{
	public class NamesByPlayerQuery : IRequest<Dictionary<int, string>>
	{
		public static NamesByPlayerQuery Instance = new NamesByPlayerQuery();

		private NamesByPlayerQuery()
		{
		}
	}
}
