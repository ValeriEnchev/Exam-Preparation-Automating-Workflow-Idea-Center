using System.Text.Json.Serialization;

namespace IdeaCenter.Models
{
    internal class ApiResponseDTO
    {
        // Msg of type string to capture response messages.
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        // IdeaId of type string to capture the unique identifier of an idea.This field may be null for responses that do not include idea ID.
        [JsonPropertyName("id")]
        public string? IdeaId { get; set; }
    }
}
