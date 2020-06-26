using System.Collections.Generic;
using System.Threading;

namespace Newtonsoft.Json.AsyncEnumerable
{
	class AsyncEnumerableList<T> : List<T>, IAsyncEnumerable<T>
	{
#pragma warning disable 1998
		public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			foreach (var element in this)
				yield return element;
		}
#pragma warning restore 1998
	}
}
