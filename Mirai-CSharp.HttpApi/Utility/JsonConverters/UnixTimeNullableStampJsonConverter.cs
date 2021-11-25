using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mirai.CSharp.HttpApi.Utility.JsonConverters
{
    public class UnixTimeNullableStampJsonConverter:JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => Utils.UnixTime2DateTime(reader.GetInt32()),
                JsonTokenType.String => DateTime.Parse(reader.GetString()!),
                _ => null
            };
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }
            writer.WriteNumberValue(Utils.DateTime2UnixTimeSeconds((DateTime)value));
        }
    }
}
