using System;

namespace TechnicalDocsAssistant.Core.DTOs
{
    public class AssetResponseDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string MarkdownContent { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }

    public class CreateAssetDTO
    {
        public string Title { get; set; }
        public string MarkdownContent { get; set; }
    }

    public class UpdateAssetDTO
    {
        public string Title { get; set; }
        public string MarkdownContent { get; set; }
    }

    public class SearchAssetsDTO
    {
        public string Query { get; set; }
        public int Limit { get; set; } = 5;
    }
}
