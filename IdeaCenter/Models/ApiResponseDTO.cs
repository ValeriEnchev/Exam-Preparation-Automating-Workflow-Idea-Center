using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

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
