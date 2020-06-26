using System;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;

using FluentAssertions;

namespace Newtonsoft.Json.AsyncEnumerable.Tests
{
	public class AsyncEnumerableTypeUtilityTests
	{
		class TestTypeImplements : IAsyncEnumerable<string>
		{
			public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
				=> throw new NotImplementedException();
		}

		class TestTypeInherits : TestTypeImplements
		{
		}

		interface ITestIntermediateInterface : IAsyncEnumerable<string>
		{
		}

		class TestTypeImplementsIntermediate : ITestIntermediateInterface
		{
			public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
				=> throw new NotImplementedException();
		}

		class TestTypeInheritsIntermediate : TestTypeImplementsIntermediate
		{
		}

		[TestCase(typeof(IAsyncEnumerable<string>))]
		[TestCase(typeof(TestTypeImplements))]
		[TestCase(typeof(TestTypeInherits))]
		[TestCase(typeof(ITestIntermediateInterface))]
		[TestCase(typeof(TestTypeImplementsIntermediate))]
		[TestCase(typeof(TestTypeInheritsIntermediate))]
		public void Should_find_IAsyncEnumerable_interface(Type type)
		{
			// Act
			var iface = AsyncEnumerableTypeUtility.FindAsyncEnumerableInterface(type);

			// Assert
			iface.Should().Be(typeof(IAsyncEnumerable<string>));
		}

		[TestCase(true, typeof(IAsyncEnumerable<string>))]
		[TestCase(true, typeof(TestTypeImplements))]
		[TestCase(true, typeof(TestTypeInherits))]
		[TestCase(true, typeof(ITestIntermediateInterface))]
		[TestCase(true, typeof(TestTypeImplementsIntermediate))]
		[TestCase(true, typeof(TestTypeInheritsIntermediate))]
		[TestCase(false, typeof(object))]
		[TestCase(false, typeof(int))]
		[TestCase(false, typeof(string))]
		[TestCase(false, typeof(AsyncEnumerableTypeUtility))]
		public void Should_determine_is_IAsyncEnumerable(bool expected, Type type)
		{
			// Act
			var actual = AsyncEnumerableTypeUtility.IsAsyncEnumerable(type);

			// Assert
			actual.Should().Be(expected);
		}
	}
}
