using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mirai.CSharp.HttpApi.Utility.JsonConverters
{
    public class TimeSpanNullableSecondsConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return TimeSpan.FromSeconds(reader.GetDouble());
            }
            catch
            {
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }
            writer.WriteNumberValue(((TimeSpan)value).TotalSeconds);
        }
    }
}
