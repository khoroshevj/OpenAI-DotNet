using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.Threads
{
    public sealed class ThreadMessageFilesList
    {
        [JsonPropertyName("object")]
        public string Object { get; private set; }
    
        [JsonPropertyName("data")]
        public IReadOnlyList<ThreadMessageFile> Data { get; private set; }

        [JsonPropertyName("first_id")]
        public string FirstId { get; private set; }

        [JsonPropertyName("last_id")]
        public string LastId { get; private set; }

        [JsonPropertyName("has_more")]
        public bool HasMore { get; private set; }
    }
}