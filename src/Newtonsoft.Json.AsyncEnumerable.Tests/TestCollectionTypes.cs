using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.AsyncEnumerable.Tests
{
	static class TestCollectionTypes
	{
		public class MyAsyncEnumerableList<T> : AsyncEnumerableList<T>
		{
		}

		public class Collection : ICollection<string>, IAsyncEnumerable<string>
		{
			List<string> _data = new List<string>();

			public int Count => _data.Count;
			public bool IsReadOnly => false;
			public void Add(string item) => _data.Add(item);
			public void Clear() => _data.Clear();
			public bool Contains(string item) => _data.Contains(item);
			public void CopyTo(string[] array, int arrayIndex) => _data.CopyTo(array, arrayIndex);
			public bool Remove(string item) => _data.Remove(item);

			public IEnumerator<string> GetEnumerator() => _data.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

#pragma warning disable 1998
			public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			{
				foreach (var element in _data)
					yield return element;
			}
#pragma warning restore 1998
		}

		public class CollectionWithoutDefaultCtor : Collection
		{
			private CollectionWithoutDefaultCtor()
			{
			}
		}

		public abstract class AbstractCollection : Collection
		{
		}

		public class ImplementationOfAbstractCollection : AbstractCollection
		{
		}

		public class ConstructWithIEnumerable : IAsyncEnumerable<string>
		{
			public List<string> Data;

			public ConstructWithIEnumerable(IEnumerable<string> sequence)
			{
				Data = sequence.ToList();
			}

#pragma warning disable 1998
			public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			{
				foreach (var element in Data)
					yield return element;
			}
#pragma warning restore 1998
		}

		public class ConstructWithIEnumerableIncompleteEnumeration : ConstructWithIEnumerable
		{
			public ConstructWithIEnumerableIncompleteEnumeration(IEnumerable<string> sequence)
				: base(sequence.Take(1))
			{
			}
		}

		public class ConstructWithIEnumerableEnumerateTwice : ConstructWithIEnumerable
		{
			public ConstructWithIEnumerableEnumerateTwice(IEnumerable<string> sequence)
				: base(sequence)
			{
				sequence.ToList();
			}
		}

		public abstract class AbstractConstructWithIEnumerable : ConstructWithIEnumerable
		{
			public AbstractConstructWithIEnumerable(IEnumerable<string> s) : base(s)
			{
			}
		}

		public class ImplementationOfAbstractConstructWithIEnumerable : AbstractConstructWithIEnumerable
		{
			public ImplementationOfAbstractConstructWithIEnumerable(IEnumerable<string> s) : base(s)
			{
			}
		}

		public class ConstructWithIAsyncEnumerable : ConstructWithIEnumerable
		{
			public ConstructWithIAsyncEnumerable(IAsyncEnumerable<string> sequence)
				: this(sequence, int.MaxValue)
			{
			}

			public ConstructWithIAsyncEnumerable(IAsyncEnumerable<string> sequence, int enumerationLimit)
				: base(Enumerable.Empty<string>())
			{
				PopulateData(sequence, enumerationLimit).Wait();
			}

			async Task PopulateData(IAsyncEnumerable<string> sequence, int enumerationLimit)
			{
				await foreach (var element in sequence)
				{
					Data.Add(element);

					enumerationLimit--;
					if (enumerationLimit == 0)
						break;
				}
			}
		}

		public class ConstructWithIAsyncEnumerableIncompleteEnumeration : ConstructWithIAsyncEnumerable
		{
			public ConstructWithIAsyncEnumerableIncompleteEnumeration(IAsyncEnumerable<string> sequence)
				: base(sequence, 1)
			{
			}
		}

		public class ConstructWithIAsyncEnumerableEnumerateTwice : ConstructWithIAsyncEnumerable
		{
			public ConstructWithIAsyncEnumerableEnumerateTwice(IAsyncEnumerable<string> sequence)
				: base(sequence)
			{
				sequence.GetAsyncEnumerator().MoveNextAsync().AsTask().Wait();
			}
		}

		public abstract class AbstractConstructWithIAsyncEnumerable : ConstructWithIAsyncEnumerable
		{
			public AbstractConstructWithIAsyncEnumerable(IAsyncEnumerable<string> s) : base(s)
			{
			}
		}

		public class ImplementationOfAbstractConstructWithIAsyncEnumerable : AbstractConstructWithIAsyncEnumerable
		{
			public ImplementationOfAbstractConstructWithIAsyncEnumerable(IAsyncEnumerable<string> s) : base(s)
			{
			}
		}
	}
}
