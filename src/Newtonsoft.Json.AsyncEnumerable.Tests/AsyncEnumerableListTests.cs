using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using NUnit.Framework;

using Bogus;

using FluentAssertions;

namespace Newtonsoft.Json.AsyncEnumerable.Tests
{
	public class AsyncEnumerableListTests
	{
		[Test]
		public void Should_be_ICollection_T()
		{
			// Arrange
			var sut_int = new AsyncEnumerableList<int>();
			var sut_string = new AsyncEnumerableList<string>();

			// Assert
			sut_int.Should().BeAssignableTo<ICollection<int>>();
			sut_string.Should().BeAssignableTo<ICollection<string>>();
		}

		[Test]
		public void Should_be_IEnumerable_T()
		{
			// Arrange
			var sut_int = new AsyncEnumerableList<int>();
			var sut_string = new AsyncEnumerableList<string>();

			// Assert
			sut_int.Should().BeAssignableTo<IEnumerable<int>>();
			sut_string.Should().BeAssignableTo<IEnumerable<string>>();
		}

		[Test]
		public void Should_be_IAsyncEnumerable_T()
		{
			// Arrange
			var sut_int = new AsyncEnumerableList<int>();
			var sut_string = new AsyncEnumerableList<string>();

			// Assert
			sut_int.Should().BeAssignableTo<IAsyncEnumerable<int>>();
			sut_string.Should().BeAssignableTo<IAsyncEnumerable<string>>();
		}

		[Test]
		public void Should_enumerate_added_elements_with_IEnumerable()
		{
			// Arrange
			var faker = new Faker();

			var sequence = faker.Random.WordsArray(10);

			var sut = new AsyncEnumerableList<string>();

			foreach (var element in sequence)
				sut.Add(element);

			// Act
			var result = sut.ToList();

			// Assert
			result.Should().BeEquivalentTo(sequence);
		}

		[Test]
		public async Task Should_enumerate_added_elements_with_IAsyncEnumerable()
		{
			// Arrange
			var faker = new Faker();

			var sequence = faker.Random.WordsArray(10);

			var sut = new AsyncEnumerableList<string>();

			foreach (var element in sequence)
				sut.Add(element);

			// Act
			var result = new List<string>();

			await foreach (var element in sut)
				result.Add(element);

			// Assert
			result.Should().BeEquivalentTo(sequence);
		}
	}
}
