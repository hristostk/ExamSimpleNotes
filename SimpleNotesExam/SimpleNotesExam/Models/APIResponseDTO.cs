using System.Text.Json.Serialization;

namespace SimpleNotesExam.Models
{
    internal class ApiResponseDto
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
    }
}