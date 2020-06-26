using System;
using System.Collections.Generic;
using System.Threading;

namespace Newtonsoft.Json.AsyncEnumerable.Tests.Adapters
{
	class EnumerableAdapter : IEnumerable<string>, IAsyncEnumerable<string>
	{
		IEnumerable<string> _elements;

		public EnumerableAdapter(IEnumerable<string> elements)
		{
			_elements = elements;
		}

		public IEnumerator<string> GetEnumerator() => _elements.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _elements.GetEnumerator();

#pragma warning disable 1998
		public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			foreach (var element in _elements)
				yield return element;
		}
#pragma warning restore 1998
	}

}
