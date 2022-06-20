using System.Text.Json;

namespace _1.RabbitMQ.Producer
{
    public static class JsonExtensions
    {
        public static string Serialize<T>(this T input)
        {
            return JsonSerializer.Serialize(input);
        }

        public static T Deserialize<T>(this string input)
        {
            return !string.IsNullOrEmpty(input) ? JsonSerializer.Deserialize<T>(input) : default!;
        }
    }
}
