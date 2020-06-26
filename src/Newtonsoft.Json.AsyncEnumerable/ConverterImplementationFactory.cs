using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Newtonsoft.Json.AsyncEnumerable
{
	static class ConverterImplementationFactory
	{
		static readonly ConcurrentDictionary<Type, IConverterImplementation> s_converterImplementations = new ConcurrentDictionary<Type, IConverterImplementation>();

		public static IConverterImplementation GetConverterImplementation(Type objectType)
		{
			if (!s_converterImplementations.TryGetValue(objectType, out var implementation))
			{
				var asyncEnumerableType = AsyncEnumerableTypeUtility.FindAsyncEnumerableInterface(objectType);

				if (asyncEnumerableType == null)
					throw new Exception("Internal error: GetConverterImplementation called with data type that should not have passed JsonAsyncEnumerableConverter.CanConvert");

				var elementType = asyncEnumerableType.GetGenericArguments().Single();

				var implementationType = typeof(ConverterImplementation<>).MakeGenericType(elementType);

				implementation = (IConverterImplementation)Activator.CreateInstance(implementationType, new[] { objectType });

				s_converterImplementations.TryAdd(objectType, implementation);
			}

			return implementation;
		}
	}
}
