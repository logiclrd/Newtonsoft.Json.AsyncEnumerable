using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using NUnit.Framework;

namespace Newtonsoft.Json.AsyncEnumerable.Tests
{
	class ConverterImplementationFactoryTests
	{
		[Test]
		public void Should_detect_non_AsyncEnumerable_type()
		{
			// Act & Assert
			Assert.Throws<Exception>(
				() =>
				{
					ConverterImplementationFactory.GetConverterImplementation(typeof(string));
				});
		}

		[Test]
		public void Should_create_converter_implementation_with_correct_element_type()
		{
			// Act
			var result = ConverterImplementationFactory.GetConverterImplementation(typeof(AsyncEnumerableList<string>));

			// Assert
			result.Should().BeOfType<ConverterImplementation<string>>();
		}

		[Test]
		public void Should_cache_converter_implementation_instances()
		{
			// Arrange
			var expected = ConverterImplementationFactory.GetConverterImplementation(typeof(AsyncEnumerableList<string>)); ;

			ConverterImplementationFactory.GetConverterImplementation(typeof(AsyncEnumerableList<int>));

			// Act
			var actual = ConverterImplementationFactory.GetConverterImplementation(typeof(AsyncEnumerableList<string>));

			// Assert
			actual.Should().BeSameAs(expected);
		}

		[Test]
		public void Should_not_get_confused_with_different_collection_types_with_same_element_type()
		{
			// Arrange
			var first = ConverterImplementationFactory.GetConverterImplementation(typeof(AsyncEnumerableList<string>)); ;
			var second = ConverterImplementationFactory.GetConverterImplementation(typeof(TestCollectionTypes.MyAsyncEnumerableList<string>));

			// Act
			var actual = ConverterImplementationFactory.GetConverterImplementation(typeof(AsyncEnumerableList<string>));

			// Assert
			actual.Should().BeSameAs(first);
			actual.Should().NotBeSameAs(second);
		}
	}
}
