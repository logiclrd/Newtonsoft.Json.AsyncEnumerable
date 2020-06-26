using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using NUnit.Framework;

using Bogus;

using FluentAssertions;
using Newtonsoft.Json.AsyncEnumerable.Tests.Adapters;
using System.Linq;

namespace Newtonsoft.Json.AsyncEnumerable.Tests
{
	public class JsonAsyncEnumerableConverterTests
	{
		[TestCase(true, typeof(IAsyncEnumerable<string>))]
		[TestCase(true, typeof(IAsyncEnumerable<int>))]
		[TestCase(true, typeof(TestCollectionTypes.Collection))]
		[TestCase(true, typeof(TestCollectionTypes.ConstructWithIEnumerable))]
		[TestCase(true, typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable))]
		[TestCase(true, typeof(TestCollectionTypes.CollectionWithoutDefaultCtor))]
		[TestCase(true, typeof(TestCollectionTypes.AbstractCollection))]
		[TestCase(true, typeof(TestCollectionTypes.AbstractConstructWithIEnumerable))]
		[TestCase(true, typeof(TestCollectionTypes.AbstractConstructWithIAsyncEnumerable))]
		[TestCase(false, typeof(object))]
		[TestCase(false, typeof(string))]
		[TestCase(false, typeof(int))]
		[TestCase(false, typeof(List<string>))]
		public void Should_calculate_CanConvert_correctly(bool expected, Type objectType)
		{
			// Arrange
			var sut = new JsonAsyncEnumerableConverter();

			// Act
			var actual = sut.CanConvert(objectType);

			// Assert
			actual.Should().Be(expected);
		}

		IAsyncEnumerable<string> ConstructTestCollection(Type collectionType, IEnumerable<string> elements)
		{
			if (collectionType.IsInterface)
				return new EnumerableAdapter(elements);

			if (collectionType.IsAbstract)
			{
				foreach (var type in collectionType.Assembly.GetTypes())
					if (collectionType.IsAssignableFrom(type) && !type.IsAbstract)
					{
						collectionType = type;
						break;
					}
			}

			if (typeof(ICollection<string>).IsAssignableFrom(collectionType))
			{
				var collection = (ICollection<string>)Activator.CreateInstance(collectionType, true);

				foreach (var element in elements)
					collection.Add(element);

				return (IAsyncEnumerable<string>)collection;
			}
			else
			{
				var adapter = new EnumerableAdapter(elements);

				return (IAsyncEnumerable<string>)Activator.CreateInstance(collectionType, new object[] { adapter });
			}
		}

		[TestCase(typeof(IAsyncEnumerable<string>))]
		[TestCase(typeof(TestCollectionTypes.Collection))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerable))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable))]
		[TestCase(typeof(TestCollectionTypes.CollectionWithoutDefaultCtor))]
		[TestCase(typeof(TestCollectionTypes.AbstractCollection))]
		[TestCase(typeof(TestCollectionTypes.AbstractConstructWithIEnumerable))]
		[TestCase(typeof(TestCollectionTypes.AbstractConstructWithIAsyncEnumerable))]
		public void Should_write_JSON_array_with_correct_elements(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var sequence = faker.Random.WordsArray(10);

			var collection = ConstructTestCollection(collectionType, sequence);

			var serializer = new JsonSerializer();

			var expectedBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(expectedBuffer), sequence);

			var expected = expectedBuffer.ToString();

			var sut = new JsonAsyncEnumerableConverter();

			var actualBuffer = new StringWriter();
			var actualWriter = new JsonTextWriter(actualBuffer);

			// Act
			sut.WriteJson(actualWriter, collection, serializer);

			// Assert
			var actual = actualBuffer.ToString();

			actual.Should().Be(expected);
		}

		[Test]
		public void Should_write_null_JSON_array()
		{
			// Arrange
			var serializer = new JsonSerializer();

			var sut = new JsonAsyncEnumerableConverter();

			var actualBuffer = new StringWriter();
			var actualWriter = new JsonTextWriter(actualBuffer);

			// Act
			sut.WriteJson(actualWriter, null, serializer);

			// Assert
			var actual = actualBuffer.ToString();

			actual.Should().Be("null");
		}

		[TestCase(typeof(IAsyncEnumerable<string>))]
		[TestCase(typeof(AsyncEnumerableList<string>))]
		[TestCase(typeof(TestCollectionTypes.Collection))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerable))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable))]
		public void Should_read_JSON_array_into_supported_collection_type(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var expected = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var expectedJSONBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(expectedJSONBuffer), expected);

			var expectedJSON = expectedJSONBuffer.ToString();

			var reader = new JsonTextReader(new StringReader(expectedJSON));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			var sut = new JsonAsyncEnumerableConverter();

			// Act
			var result = sut.ReadJson(reader, collectionType, null, serializer);

			// Assert
			result.Should().BeAssignableTo(collectionType);

			new AsyncEnumerableAdapter((IAsyncEnumerable<string>)result).Should().BeEquivalentTo(expected);
		}

		[TestCase(typeof(AsyncEnumerableList<string>))]
		[TestCase(typeof(TestCollectionTypes.Collection))]
		public void Should_read_JSON_array_into_existing_instance_of_supported_collection_type(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var expected = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var expectedJSONBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(expectedJSONBuffer), expected);

			var expectedJSON = expectedJSONBuffer.ToString();

			var reader = new JsonTextReader(new StringReader(expectedJSON));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			var sut = new JsonAsyncEnumerableConverter();

			var existingInstance = ConstructTestCollection(collectionType, Enumerable.Empty<string>());

			// Act
			var result = sut.ReadJson(reader, collectionType, existingInstance, serializer);

			// Assert
			result.Should().BeSameAs(existingInstance);

			new AsyncEnumerableAdapter((IAsyncEnumerable<string>)existingInstance).Should().BeEquivalentTo(expected);
		}

		[TestCase(typeof(IAsyncEnumerable<string>))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerable))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable))]
		public void Should_throw_when_reading_JSON_array_into_existing_instance_of_unsupported_collection_type(Type collectionType)
		{
			// Arrange
			var faker = new Faker();

			var input = faker.Random.WordsArray(10);

			var serializer = new JsonSerializer();

			var inputJSONBuffer = new StringWriter();

			serializer.Serialize(new JsonTextWriter(inputJSONBuffer), input);

			var inputJSON = inputJSONBuffer.ToString();

			var reader = new JsonTextReader(new StringReader(inputJSON));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.StartArray);

			var sut = new JsonAsyncEnumerableConverter();

			var existingInstance = ConstructTestCollection(collectionType, Enumerable.Empty<string>());

			// Act & Assert
			Assert.Throws<JsonSerializationException>(
				() =>
				{
					sut.ReadJson(reader, collectionType, existingInstance, serializer);
				});
		}

		[TestCase(typeof(IAsyncEnumerable<string>))]
		[TestCase(typeof(AsyncEnumerableList<string>))]
		[TestCase(typeof(TestCollectionTypes.Collection))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIEnumerable))]
		[TestCase(typeof(TestCollectionTypes.ConstructWithIAsyncEnumerable))]
		public void Should_read_null_array(Type collectionType)
		{
			// Arrange
			var serializer = new JsonSerializer();

			var reader = new JsonTextReader(new StringReader("null"));

			reader.Read().Should().BeTrue();
			reader.TokenType.Should().Be(JsonToken.Null);

			var sut = new JsonAsyncEnumerableConverter();

			// Act
			var result = sut.ReadJson(reader, collectionType, null, serializer);

			// Assert
			result.Should().BeNull();
		}
		/*
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;
			else if (reader.TokenType != JsonToken.StartArray)
				throw new FormatException("Expected start of JSON array");
			else
			{
				var implementation = ConverterImplementationFactory.GetConverterImplementation(objectType);

				if (implementation.CanRead)
					return implementation.ReadJson(reader, existingValue, serializer);

				if (implementation.CanConstruct)
				{
					if (existingValue != null)
						throw new Exception($"Cannot deserialize into existing value of type '{existingValue.GetType()}' because it does not implement ICollection<T>");

					return implementation.ConstructFromJson(reader, serializer);
				}

				throw new Exception($"Cannot deserialize value of type '{objectType}' because it does not implement ICollection<T> and it does not have a constructor that accepts an IEnumerable<T> or IAsyncEnumerable<T>.");
			}
		}
		*/
	}
}
