using System.Text.Json.Serialization;

namespace IdeaCenter.Models
{
    internal class IdeaDTO
    {
        // Title of type string for the idea's title.
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        // Description of type string for the idea's description.
        [JsonPropertyName ("description")]
        public string? Description { get; set; }

        // An optional Url of type string representing a link to the idea's picture, if applicable.
        [JsonPropertyName("url")]
        public string? Url { get; set; } = null;

    }
}
