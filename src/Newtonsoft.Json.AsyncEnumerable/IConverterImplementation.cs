using System.Threading.Tasks;

namespace Newtonsoft.Json.AsyncEnumerable
{
	interface IConverterImplementation
	{
		Task WriteJsonAsync(JsonWriter writer, object value);

		bool CanRead { get; }
		object ReadJson(JsonReader reader, object existingValue, JsonSerializer serializer);

		bool CanConstruct { get; }
		object ConstructFromJson(JsonReader reader, JsonSerializer serializer);
	}
}
