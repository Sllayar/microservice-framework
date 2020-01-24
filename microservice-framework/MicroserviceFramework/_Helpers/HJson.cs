using Newtonsoft.Json;

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HJson
    {
        public static T JsonDeserialize<T>(this string source) => JsonConvert.DeserializeObject<T>(source);

        public static string JsonSerialize<T>(this T source, bool indented = false) => JsonConvert.SerializeObject(source, indented ? Formatting.Indented : Formatting.None);

        public static void PopulateFromJson<T>(this T source, string json) => JsonConvert.PopulateObject(json, source);


        /*public static T JsonDeserialize<T>(this string source) => JsonSerializer.Deserialize<T>(source, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        public static string JsonSerialize<T>(this T source, bool writeIndented = false) => JsonSerializer.Serialize(source, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = writeIndented,
        });*/
    }
}