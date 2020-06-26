using System;
using System.Collections.Generic;
using System.Threading;

namespace Newtonsoft.Json.AsyncEnumerable.Tests.Adapters
{
	class AsyncEnumerableAdapter : IEnumerable<string>, IAsyncEnumerable<string>
	{
		IAsyncEnumerable<string> _elements;

		public AsyncEnumerableAdapter(IAsyncEnumerable<string> elements)
		{
			_elements = elements;
		}

		public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken token = default) => _elements.GetAsyncEnumerator();

		public IEnumerator<string> GetEnumerator()
		{
			var enumerator = _elements.GetAsyncEnumerator();

			try
			{
				while (enumerator.MoveNextAsync().Result)
					yield return enumerator.Current;
			}
			finally
			{
				enumerator.DisposeAsync().AsTask().Wait();
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
	}
}
