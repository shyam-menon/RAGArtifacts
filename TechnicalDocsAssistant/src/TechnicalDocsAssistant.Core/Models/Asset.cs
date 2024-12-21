using System;
using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace TechnicalDocsAssistant.Core.Models
{
    [Table("assets")]
    public class Asset : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Column("markdown_content")]
        [Required]
        public string MarkdownContent { get; set; }

        [Column("content_vector")]
        public float[] ContentVector { get; set; }

        [Column("created_at")]
        public DateTime Created { get; set; }

        [Column("modified_at")]
        public DateTime Modified { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
