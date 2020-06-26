using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.AsyncEnumerable
{
	class ConverterImplementation<TElement> : IConverterImplementation
	{
		Type _collectionType;

		Func<ICollection<TElement>> _constructBlank;
		Func<IEnumerable<TElement>, object> _constructFromEnumerable;
		Func<IAsyncEnumerable<TElement>, object> _constructFromAsyncEnumerable;

		public bool CanRead => _constructBlank != null;
		public bool CanConstruct => (_constructFromEnumerable != null) || (_constructFromAsyncEnumerable != null);

		public ConverterImplementation(Type collectionType)
		{
			if (!typeof(IAsyncEnumerable<TElement>).IsAssignableFrom(collectionType))
				throw new Exception("Internal error: ConverterImplementation being constructed for a collection type that does not implement IAsyncEnumerable<TElement>");

			_collectionType = collectionType;

			if (collectionType.IsAssignableFrom(typeof(AsyncEnumerableList<TElement>)))
				_constructBlank = () => new AsyncEnumerableList<TElement>();
			else if (!collectionType.IsInterface && !collectionType.IsAbstract)
			{
				if (typeof(ICollection<TElement>).IsAssignableFrom(collectionType)
				 && (collectionType.GetConstructor(Type.EmptyTypes) is ConstructorInfo defaultConstructor))
				{
					_constructBlank =
						Expression.Lambda<Func<ICollection<TElement>>>(
							Expression.New(defaultConstructor)).Compile();
				}
				else if (collectionType.GetConstructor(new[] { typeof(IEnumerable<TElement>) }) is ConstructorInfo enumerableConstructor)
				{
					var param = Expression.Parameter(typeof(IEnumerable<TElement>));

					_constructFromEnumerable =
						Expression.Lambda<Func<IEnumerable<TElement>, object>>(
							Expression.New(
								enumerableConstructor,
								param),
							param).Compile();
				}
				else if (collectionType.GetConstructor(new[] { typeof(IAsyncEnumerable<TElement>) }) is ConstructorInfo asyncEnumerableConstructor)
				{
					var param = Expression.Parameter(typeof(IAsyncEnumerable<TElement>));

					_constructFromAsyncEnumerable =
						Expression.Lambda<Func<IAsyncEnumerable<TElement>, object>>(
							Expression.New(
								asyncEnumerableConstructor,
								param),
							param).Compile();
				}
			}
		}

		public Task WriteJsonAsync(JsonWriter writer, object value) => WriteJsonAsync(writer, (IAsyncEnumerable<TElement>)value);

		public async Task WriteJsonAsync(JsonWriter writer, IAsyncEnumerable<TElement> value)
		{
			writer.WriteStartArray();

			await foreach (var element in value)
				writer.WriteValue(element);

			writer.WriteEndArray();
		}

		public object ReadJson(JsonReader reader, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.StartArray)
				throw new JsonSerializationException($"Cannot deserialize the current JSON into a collection type because the type requires a JSON array and the current JSON node is of type: {reader.TokenType}");

			ICollection<TElement> container = null;

			if (existingValue != null)
			{
				container = existingValue as ICollection<TElement>;

				if (container == null)
					throw new JsonSerializationException($"Cannot deserialize into value of type '{existingValue.GetType()}' because it does not implement ICollection<T>");
			}
			else
			{
				if (_constructBlank == null)
					throw new Exception("Internal error: ReadJson called on a ConverterImplementation<T> that didn't identify a default constructor");

				container = _constructBlank();
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndArray)
					break;

				container.Add(serializer.Deserialize<TElement>(reader));
			}

			return container;
		}

		public object ConstructFromJson(JsonReader reader, JsonSerializer serializer)
		{
			var adapter = new ReadArrayElementsAsEnumerable(reader, serializer);

			try
			{
				if (_constructFromEnumerable != null)
					return _constructFromEnumerable(adapter);
				else if (_constructFromAsyncEnumerable != null)
					return _constructFromAsyncEnumerable(adapter);
				else
				{
					adapter = null;
					throw new Exception("Internal error: ConstructFromJson called on a ConverterImplementation<T> that didn't identify a compatible constructor");
				}
			}
			catch (AggregateException e) when (e.InnerException is EnumeratedTwiceException)
			{
				throw new JsonSerializationException($"Constructor for type '{_collectionType}' tried to enumerate the async enumerable object it was passed multiple times");
			}
			catch (EnumeratedTwiceException)
			{
				throw new JsonSerializationException($"Constructor for type '{_collectionType}' tried to enumerate the enumerable object it was passed multiple times");
			}
			finally
			{
				if ((adapter != null) && !adapter.IsComplete)
					throw new JsonSerializationException($"Constructor for type '{_collectionType}' did not read through to the end of the enumerable object it was passed");
			}
		}

		class EnumeratedTwiceException : Exception { }

		class ReadArrayElementsAsEnumerable : IEnumerable<TElement>, IAsyncEnumerable<TElement>
		{
			JsonReader _reader;
			JsonSerializer _serializer;
			bool _isConsumed = false;
			bool _isComplete = false;

			public bool IsComplete => _isComplete;

			public ReadArrayElementsAsEnumerable(JsonReader reader, JsonSerializer serializer)
			{
				_reader = reader;
				_serializer = serializer;
			}

			public IEnumerator<TElement> GetEnumerator()
			{
				if (_isConsumed)
					throw new EnumeratedTwiceException();

				_isConsumed = true;

				while (_reader.Read() && (_reader.TokenType != JsonToken.EndArray))
					yield return _serializer.Deserialize<TElement>(_reader);

				_isComplete = true;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#pragma warning disable 1998
			public async IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			{
				if (_isConsumed)
					throw new EnumeratedTwiceException();

				_isConsumed = true;

				while (_reader.Read() && (_reader.TokenType != JsonToken.EndArray))
					yield return _serializer.Deserialize<TElement>(_reader);

				_isComplete = true;
			}
#pragma warning restore 1998
		}
	}
}
