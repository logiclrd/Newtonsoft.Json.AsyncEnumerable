using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Bogus;

using FluentAssertions;

namespace Newtonsoft.Json.AsyncEnumerable.Tests
{
	public class ConverterImplementationTests
	{
		public void Should_throw_if_element_type_does_not_match()
		{
			// Act & Assert
			Assert.Throws<Exception>(
				() =>
				{
					new ConverterImplementation<string>(typeof(AsyncEnumerableList<int>));
				});
		}

		public void Should_throw_if_used_with_a_non_IAsyncEnumerable_type()
		{
			// Act & Assert
			Assert.Throws<Exception>(
				() =>
				{
					new ConverterImplementation<string>(typeof(string));
				});
		}

		[TestCase(true, typeof(AsyncEnumerableList<string>), "internal AsyncEnumerableList type")]
		[TestCase(true, typeof(TestCollectionTypes.Collection), "type on which Add can be called because it implements ICollection<T>")]
		[TestCase(false, typeof(TestCollectionTypes.CollectionWithoutDefaultCtor), "type that implements ICollection<T> but which can't be constructed")]
		[TestCase(false, typeof(TestCollectionTypes.AbstractCollection), "abstract type that implements ICollection<T>")]
		[TestCase(false, typeof(TestCollectionTypes.ConstructWithIEnumerable), "type with ctor accepting IEnumerable<T>")]
		[TestCase(false, typeof(TestCollectionTypes.AbstractConstructWithIEnumerable), "abstract type with ctor accepting IEnumerable<T>")]
		[TestCase(false, typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable), "type with ctor accepting IAsyncEnumerable<T>")]
		[TestCase(false, typeof(TestCollectionTypes.AbstractConstructWithIAsyncEnumerable), "abstract type with ctor accepting IAsyncEnumerable<T>")]
		public void Should_determine_CanRead(bool expected, Type collectionType, string collectionTypeDescription)
		{
			// Arrange
			var sut = new ConverterImplementation<string>(collectionType);

			// Act
			var actual = sut.CanRead;

			// Assert
			actual.Should().Be(expected, $"CanRead should be {expected} for {collectionTypeDescription}");
		}

		[TestCase(false, typeof(AsyncEnumerableList<string>), "internal AsyncEnumerableList type")]
		[TestCase(false, typeof(TestCollectionTypes.Collection), "type on which Add can be called because it implements ICollection<T>")]
		[TestCase(false, typeof(TestCollectionTypes.CollectionWithoutDefaultCtor), "type that implements ICollection<T> but which can't be constructed")]
		[TestCase(false, typeof(TestCollectionTypes.AbstractCollection), "abstract type that implements ICollection<T>")]
		[TestCase(true, typeof(TestCollectionTypes.ConstructWithIEnumerable), "type with ctor accepting IEnumerable<T>")]
		[TestCase(false, typeof(TestCollectionTypes.AbstractConstructWithIEnumerable), "abstract type with ctor accepting IEnumerable<T>")]
		[TestCase(true, typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable), "type with ctor accepting IAsyncEnumerable<T>")]
		[TestCase(false, typeof(TestCollectionTypes.AbstractConstructWithIAsyncEnumerable), "abstract type with ctor accepting IAsyncEnumerable<T>")]
		public void Should_determine_CanConstruct(bool expected, Type collectionType, string collectionTypeDescription)
		{
			// Arrange
			var sut = new ConverterImplementation<string>(collectionType);

			// Act
			var actual = sut.CanConstruct;

			// Assert
			actual.Should().Be(expected, $"CanRead should be {expected} for {collectionTypeDescription}");
		}

		[Test]
		public async Task Should_write_JSON_array_with_correct_elements()
		{
			// Arrange
			var faker = new Faker();

			var elements = faker.Random.WordsArray(10);

			var asyncEnumerableElements = new AsyncEnumerableList<string>();

			asyncEnumerableElements.AddRange(elements);

			var expected = JsonConvert.SerializeObject(elements);

			var sut = new ConverterImplementation<string>(typeof(AsyncEnumerableList<string>));

			// Act
			var buffer = new StringWriter();

			await sut.WriteJsonAsync(new JsonTextWriter(buffer), asyncEnumerableElements);

			var actual = buffer.ToString();

			// Assert
			actual.Should().Be(expected);
		}

		[Test]
		public void Should_read_JSON_array_into_ICollection_type()
		{
			// Arrange
			var faker = new Faker();

			var elements = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var jsonBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(jsonBuffer), elements);

			var json = jsonBuffer.ToString();

			var sut = new ConverterImplementation<string>(typeof(AsyncEnumerableList<string>));

			var reader = new JsonTextReader(new StringReader(json));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			// Act
			var result = sut.ReadJson(reader, existingValue: null, serializer);

			// Assert
			result.Should().BeOfType<AsyncEnumerableList<string>>();

			var resultList = (AsyncEnumerableList<string>)result;

			resultList.Should().BeEquivalentTo(elements);
		}

		[Test]
		public void Should_read_JSON_array_into_existing_ICollection_object()
		{
			// Arrange
			var faker = new Faker();

			var elements = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var jsonBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(jsonBuffer), elements);

			var json = jsonBuffer.ToString();

			var sut = new ConverterImplementation<string>(typeof(AsyncEnumerableList<string>));

			var reader = new JsonTextReader(new StringReader(json));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			var existingValue = new AsyncEnumerableList<string>();

			// Act
			var result = sut.ReadJson(reader, existingValue, serializer);

			// Assert
			result.Should().BeSameAs(existingValue);
			existingValue.Should().BeEquivalentTo(elements);
		}

		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerable))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable))]
		public void Should_read_JSON_array_into_constructor_using_IEnumerable_or_IAsyncEnumerable(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var elements = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var jsonBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(jsonBuffer), elements);

			var json = jsonBuffer.ToString();

			var sut = new ConverterImplementation<string>(collectionType);

			var reader = new JsonTextReader(new StringReader(json));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			// Act
			var result = sut.ConstructFromJson(reader, serializer);

			// Assert
			result.Should().BeOfType(collectionType);

			var resultList = ((TestCollectionTypes.ConstructWithIEnumerable)result).Data;

			resultList.Should().BeEquivalentTo(elements);
		}

		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerableIncompleteEnumeration))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerableIncompleteEnumeration))]
		public void Should_detect_incomplete_enumeration_when_reading_JSON_array_into_constructor_using_IEnumerable_or_IAsyncEnumerable(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var elements = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var jsonBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(jsonBuffer), elements);

			var json = jsonBuffer.ToString();

			var sut = new ConverterImplementation<string>(collectionType);

			var reader = new JsonTextReader(new StringReader(json));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			// Act & Assert
			Assert.Throws<JsonSerializationException>(
				() =>
				{
					sut.ConstructFromJson(reader, serializer);
				});
		}

		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerableEnumerateTwice))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerableEnumerateTwice))]
		public void Should_detect_repeated_enumeration_when_reading_JSON_array_into_constructor_using_IEnumerable_or_IAsyncEnumerable(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var elements = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var jsonBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(jsonBuffer), elements);

			var json = jsonBuffer.ToString();

			var sut = new ConverterImplementation<string>(collectionType);

			var reader = new JsonTextReader(new StringReader(json));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			// Act & Assert
			Assert.Throws<JsonSerializationException>(
				() =>
				{
					sut.ConstructFromJson(reader, serializer);
				});
		}
	}
}
