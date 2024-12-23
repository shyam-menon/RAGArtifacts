using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TechnicalDocsAssistant.Core.DTOs
{
    public class AssetResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("markdown_content")]
        public string MarkdownContent { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; }
    }

    public class CreateAssetDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "MarkdownContent is required")]
        [JsonProperty("markdown_content")]
        public string MarkdownContent { get; set; }
    }

    public class UpdateAssetDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "MarkdownContent is required")]
        [JsonProperty("markdown_content")]
        public string MarkdownContent { get; set; }
    }

    public class SearchAssetsDTO
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; } = 5;
    }
}
