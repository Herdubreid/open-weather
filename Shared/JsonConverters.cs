using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenWeather
{
    public class TupleJsonConverter<T1, T2, T3> : JsonConverter<(T1, T2, T3)>
    {
        public override (T1, T2, T3) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            (T1, T2, T3) result = default;

            if (!reader.Read())
            {
                throw new JsonException();
            }

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.ValueTextEquals("Item1") && reader.Read())
                {
                    result.Item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                }
                else if (reader.ValueTextEquals("Item2") && reader.Read())
                {
                    result.Item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                }
                else if (reader.ValueTextEquals("Item3") && reader.Read())
                {
                    result.Item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                }
                else
                {
                    throw new JsonException();
                }
                reader.Read();
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, (T1, T2, T3) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Item1");
            JsonSerializer.Serialize(writer, value.Item1, options);
            writer.WritePropertyName("Item2");
            JsonSerializer.Serialize(writer, value.Item2, options);
            writer.WritePropertyName("Item3");
            JsonSerializer.Serialize(writer, value.Item3, options);
            writer.WriteEndObject();
        }
    }
    public class TupleJsonConverter<T1, T2> : JsonConverter<(T1, T2)>
    {
        public override (T1, T2) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            (T1, T2) result = default;

            if (!reader.Read())
            {
                throw new JsonException();
            }

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.ValueTextEquals("Item1") && reader.Read())
                {
                    result.Item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                }
                else if (reader.ValueTextEquals("Item2") && reader.Read())
                {
                    result.Item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                }
                else
                {
                    throw new JsonException();
                }
                reader.Read();
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, (T1, T2) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Item1");
            JsonSerializer.Serialize<T1>(writer, value.Item1, options);
            writer.WritePropertyName("Item2");
            JsonSerializer.Serialize<T2>(writer, value.Item2, options);
            writer.WriteEndObject();
        }
    }
    public class DateJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                int json = JsonSerializer.Deserialize<int>(ref reader, options);
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return epoch.AddSeconds(json);
            }
            catch
            {
                return default(DateTime);
            }
        }
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    public class TimeSpanConverter : JsonConverter<(TimeSpan, decimal)>
    {
        public override (TimeSpan, decimal) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                decimal json = JsonSerializer.Deserialize<decimal>(ref reader, options);
                return (TimeSpan.FromSeconds(Convert.ToInt32(86400 * json / 360)), json);
            }
            catch
            {
                return (default(TimeSpan), 0);
            }
        }
        public override void Write(Utf8JsonWriter writer, (TimeSpan, decimal) value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
