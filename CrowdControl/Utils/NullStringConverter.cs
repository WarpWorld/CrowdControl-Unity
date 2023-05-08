using System;
using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl {
    public class NullStringConverter : JsonConverter<string> {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer) {

            if (reader.Value == null) { return null; }
            string text = reader.Value.ToString();
#if NET35
            return string.IsNullOrEmpty(text.Trim()) ? null : text;
#else
            return string.IsNullOrWhiteSpace(text) ? null : text;
#endif
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
#if NET35
            => writer.WriteValue(string.IsNullOrEmpty(value?.Trim()) ? null : value);
#else
            => writer.WriteValue(string.IsNullOrWhiteSpace(value) ? null : value);
#endif
    }

}
