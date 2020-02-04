using System.Text.Json.Serialization;

namespace RFI.MicroserviceFramework._Api
{
    public class ApiSafeData
    {
        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("des")]
        public string Des { get; set; }
    }
}