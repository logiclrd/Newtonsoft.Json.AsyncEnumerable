using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.AsyncEnumerable
{
	internal static class AsyncEnumerableTypeUtility
	{
		private static readonly Dictionary<Type, Type> _asyncEnumerableInterfaceTypeCache = new Dictionary<Type, Type>();

		internal static Type FindAsyncEnumerableInterface(Type type)
		{
			if (_asyncEnumerableInterfaceTypeCache.TryGetValue(type, out var alreadyKnown))
				return alreadyKnown;

			Type result;

			if (type.IsInterface
			 && type.IsGenericType
			 && (type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)))
			{
				result = type;
			}
			else
			{
				// Note: May exit loop with result == null, if none of the interfaces match.
				result = null;

				foreach (var iface in type.GetInterfaces())
				{
					result = FindAsyncEnumerableInterface(iface);

					if (result != null)
						break;
				}
			}

			_asyncEnumerableInterfaceTypeCache[type] = result;

			return result;
		}

		internal static bool IsAsyncEnumerable(Type type)
			=> (FindAsyncEnumerableInterface(type) != null);
	}
}
