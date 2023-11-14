using System;
using System.Threading;
using System.Threading.Tasks;

namespace DZALT
{
	public interface ISession : IRepository, IDisposable
	{
		Task SubmitChanges(CancellationToken cancellationToken);
	}
}
