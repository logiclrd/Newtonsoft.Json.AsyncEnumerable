using System;

namespace Newtonsoft.Json.AsyncEnumerable
{
	public class JsonAsyncEnumerableConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => AsyncEnumerableTypeUtility.IsAsyncEnumerable(objectType);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
				writer.WriteNull();
			else
			{
				var implementation = ConverterImplementationFactory.GetConverterImplementation(value.GetType());

				implementation.WriteJsonAsync(writer, value).Wait();
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;
			else if (reader.TokenType != JsonToken.StartArray)
				throw new JsonSerializationException("Expected start of JSON array");
			else
			{
				var implementation = ConverterImplementationFactory.GetConverterImplementation(objectType);

				if (implementation.CanRead)
					return implementation.ReadJson(reader, existingValue, serializer);

				if (implementation.CanConstruct)
				{
					if (existingValue != null)
						throw new JsonSerializationException($"Cannot deserialize into existing value of type '{existingValue.GetType()}' because it does not implement ICollection<T>");

					return implementation.ConstructFromJson(reader, serializer);
				}

				throw new JsonSerializationException($"Cannot deserialize value of type '{objectType}' because it does not implement ICollection<T> and it does not have a constructor that accepts an IEnumerable<T> or IAsyncEnumerable<T>.");
			}
		}
	}
}
